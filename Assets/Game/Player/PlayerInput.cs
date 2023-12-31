using info.jacobingalls.jamkit;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.EventSystems;
using static UnityEngine.EventSystems.EventTrigger;

public class ActionSelectionRequest
{
    public Action Action;
    public Entity Entity;
}

[RequireComponent(typeof(PubSubSender))]
public class PlayerInput : MonoBehaviour
{
    [Header("User Interaction")]
    public bool UserInteractionEnabled = true;
    public Vector2Int? SelectedTilePosition = null;
    public Vector2Int? HoveredTilePosition = null;
    public Selectable SelectedSelectable { get; private set; }

    [Header("Visuals")]
    public GameObject EntitySelectionPrefab;
    public GameObject TileSelectionPrefab;
    public GameObject TileOutlinePrefab;
    public GameObject TileHoverPrefab;

    public GameObject GridRangeIndicatorPrefab;

    public bool ShowTileHover = true;
    public bool ShowTileSelection = false;

    private GameObject _entitySelectionGO;
    private GameObject _tileSelectionGO;
    private GameObject _tileHoverGO;
    private GameObject _tileOutlinesGroupGO;

    private GridManager _gridManager;
    private TurnManager _turnManager;
    private CameraControls _cameraControls;
    private Selectable _selectable;

    private GridRangeIndicator _gridRangeIndicator;

    public Action SelectedAction
    {
        get
        {
            return _selectedAction;
        }
        set
        {
            if (_selectedAction == value) { return; }
            _selectedAction = value;

            GetComponent<PubSubSender>().Publish("player_input.selected_action.changed", SelectedAction);

            if (_selectedAction == null)
            {
                Debug.Log($"No longer selecting an action.");
            }
            else
            {
                Debug.Log($"Selected action {_selectedAction.Name} for {_selectedAction.Entity.Name}.");
            }
        }
    }
    private Action _selectedAction;

    // Start is called before the first frame update
    void Awake()
    {
        _tileSelectionGO = Instantiate(TileSelectionPrefab);
        _tileSelectionGO.transform.name = "Tile Selection";
        _tileSelectionGO.transform.parent = transform;
        _tileSelectionGO.SetActive(false);

        _tileHoverGO = Instantiate(TileHoverPrefab);
        _tileHoverGO.transform.name = "Tile Hover";
        _tileHoverGO.transform.parent = transform;
        _tileHoverGO.SetActive(false);

        _tileOutlinesGroupGO = new GameObject("Tile Outlines");
        _tileOutlinesGroupGO.transform.parent = transform;

        _gridManager = FindObjectOfType<GridManager>();
        _turnManager = FindObjectOfType<TurnManager>();
        _cameraControls = FindObjectOfType<CameraControls>();

        _gridRangeIndicator = Instantiate(GridRangeIndicatorPrefab).GetComponent<GridRangeIndicator>();
        _gridRangeIndicator.transform.parent = transform;
    }

    private void Start()
    {
        SelectableDidChange();
    }

    private void UpdateSelectableVisuals()
    {
        if (SelectedSelectable == null)
        {
            if (_entitySelectionGO != null)
            {
                _entitySelectionGO.transform.parent = transform;
                _entitySelectionGO.gameObject.SetActive(false);
            }
            return;
        }

        if (SelectedSelectable.TryGetComponent<Entity>(out var entity))
        {
            SetEntitySelectionVisuals(entity);
        }
    }

    private void SetEntitySelectionVisuals(Entity entity)
    {
        if (_entitySelectionGO == null)
        {
            _entitySelectionGO = Instantiate(EntitySelectionPrefab);
            _entitySelectionGO.transform.name = "Entity Selection";
            _entitySelectionGO.transform.parent = transform;
            _entitySelectionGO.SetActive(false);
        }

        if (entity == null)
        {
            _entitySelectionGO.SetActive(false);
            return;
        }

        _entitySelectionGO.transform.position = entity.transform.position;
        _entitySelectionGO.transform.parent = entity.transform;
        _entitySelectionGO.SetActive(true);
    }

    private Entity SelectedEntity()
    {
        if (SelectedSelectable == null) { return null; }

        return SelectedSelectable.GetComponent<Entity>();
    }

    public void SelectableDidChange()
    {
        _selectable = SelectedSelectable;
    }

    private bool CanExecuteAction()
    {
        return SelectedEntity() != null && 
            !CameraControlsTakingPriority() &&
            HoveredTilePosition != null;
    }

    private bool CameraControlsTakingPriority()
    {
        if (_cameraControls == null) { return false; }
        return _cameraControls.CameraIsPanning;
    }

    private void ExecuteSelectedAction()
    {
        var entity = SelectedEntity();

        if (entity == null || SelectedAction == null || (entity.IsWaiting && !SelectedAction.CanExecuteWhileWaiting) || entity.IsBusy) { return; }

        var context = new Action.ExecutionContext();
        TileData? data = null;
        if (HoveredTilePosition != null)
        {
            data = _gridManager.GetTileData(HoveredTilePosition.Value);
        }
        context.action = SelectedAction;
        context.source = entity;
        context.range = entity.Range(SelectedAction);
        context.gridManager = _gridManager;
        context.target = data;

        if (!SelectedAction.Validate(context)) { return; }

        Debug.Log("Executing selected action!");

        _turnManager.SubmitAction(SelectedAction, context);
    }

    public void TurnDidChange()
    {
        SelectedAction = null;
        UpdateDefaultSelectedAction();
    }

    public void RequestActionSelection(PubSubListenerEvent e)
    {
        ActionSelectionRequest asr = e.value as ActionSelectionRequest;

        RequestActionSelectionHelper(asr);
    }

    public void RequestActionSelection(ActionSelectionRequest actionSelectionRequest)
    {
        RequestActionSelectionHelper(actionSelectionRequest);
    }

    private void RequestActionSelectionHelper(ActionSelectionRequest asr, bool autoExecuteNonTargetable = true)
    {
        if (asr.Entity.Owner != Entity.OwnerKind.Player)
        {
            SelectedAction = null;
            return;
        }

        if (!asr.Entity.CanAffordAction(asr.Action) || (!asr.Action.CanExecuteWhileWaiting && asr.Entity.IsWaiting))
        {
            return;
        }

        SelectedSelectable = asr.Entity.GetComponent<Selectable>();
        SelectedAction = asr.Action;

        if (!asr.Action.Targetable && autoExecuteNonTargetable)
        {
            ExecuteSelectedAction();
        }
    }

    private void Update()
    {
        if (UserInteractionEnabled == false) { return; }

        UpdateTileHighlight();
        UpdateTileSelection();

        var entity = SelectedEntity();
        if (SelectedAction != null && entity != null)
        {
            if (!entity.CanAffordAction(SelectedAction))
            {
                Debug.Log("Nullifying action");
                SelectedAction = null;
            }
            else
            {
                var shouldUpdateVisuals = !_turnManager.BlockingEventIsExecuting && SelectedAction.Targetable && !entity.IsWaiting;
                if (_turnManager.CurrentTeam == Entity.OwnerKind.Player && shouldUpdateVisuals)
                {
                    UpdateSelectedActionRangeVisuals();
                    UpdateSelectedActionPathVisuals();
                }
                else
                {
                    _gridRangeIndicator.HidePath();
                    _gridRangeIndicator.HideRange();
                }
            }
        }

        var shouldClearVisuals = SelectedAction == null || entity == null || _turnManager.BlockingEventIsExecuting || !SelectedAction.Targetable || entity.IsWaiting;
        if (shouldClearVisuals)
        {
            _gridRangeIndicator.ClearRangeVisuals();
            _gridRangeIndicator.ClearPathVisuals();
        }

        if (CanExecuteAction() && Input.GetMouseButtonUp(1))
        {
            ExecuteSelectedAction();
        }

        ProcessKeyboardShortcuts();
    }

    void ProcessKeyboardShortcuts()
    {
        // Player squad selection
        var playerSelectables = _turnManager.OwnedEntities(Entity.OwnerKind.Player).Select(e => e.GetComponent<Selectable>()).ToList();
        if (playerSelectables.Count > 0)
        {
            for (KeyCode keyCode = KeyCode.Alpha1; keyCode <= KeyCode.Alpha9; keyCode++)
            {
                var index = keyCode - KeyCode.Alpha1;
                if (index >= playerSelectables.Count) { break; }
                if (Input.GetKeyDown(keyCode))
                {
                    var selectable = playerSelectables[index];
                    Select(selectable);
                    var camera = Camera.main;
                    if (camera != null) { camera.transform.position = new Vector3(selectable.transform.position.x, selectable.transform.position.y, camera.transform.position.z); }
                }
            }
        }

        var entity = SelectedEntity();
        // Ability selection
        if (entity != null && entity.Owner == Entity.OwnerKind.Player)
        {
            List<KeyCode> actionMappings = new() { KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.R, KeyCode.F, KeyCode.V };
            for (int i = 0; i < actionMappings.Count; i++)
            {
                var keyCode = actionMappings[i];
                if (Input.GetKeyDown(keyCode) && i < entity.Actions.Count)
                {
                    ActionSelectionRequest asr = new ActionSelectionRequest();
                    asr.Entity = entity;
                    asr.Action = entity.Actions[i];
                    RequestActionSelectionHelper(asr, autoExecuteNonTargetable: false);
                }
            }

            var waitAction = entity.Actions.FirstOrDefault(a => a.Name.ToLower() == "wait");
            if (waitAction != null && Input.GetKeyDown(KeyCode.Space))
            {
                RequestActionSelection(new ActionSelectionRequest { Action = waitAction, Entity = entity });

                var entitiesWithAvailableActions = _turnManager.CurrentTeamEntitiesThatCanTakeAction.Where(e => !e.ActedThisTurn && e.Owner == Entity.OwnerKind.Player).ToList();
                var remainingSelectables = entitiesWithAvailableActions.Select(e => e.GetComponent<Selectable>()).Where(s => s != null).ToList();

                if (remainingSelectables.Count > 0)
                {
                    var selectable = remainingSelectables.First();
                    Select(selectable);
                    var camera = Camera.main;
                    if (camera != null) { camera.transform.position = new Vector3(selectable.transform.position.x, selectable.transform.position.y, camera.transform.position.z); }
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
        {
            GetComponent<PubSubSender>().Publish("end_turn_button.pressed");
        }
    }

    void UpdateTileHighlight()
    {
        if (UserInteractionEnabled == false || EventSystem.current.IsPointerOverGameObject())
        {
            HoveredTilePosition = null;
            _tileHoverGO.SetActive(false);
            return;
        }

        var cursorWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        HoveredTilePosition = new Vector2Int(Mathf.FloorToInt(cursorWorldPos.x), Mathf.FloorToInt(cursorWorldPos.y));

        if (HoveredTilePosition == null)
        {
            _tileHoverGO.SetActive(false);
            return;
        }

        _tileHoverGO.SetActive(true && ShowTileHover);
        _tileHoverGO.transform.position = _gridManager.TileCoordinateToWorldPosition(HoveredTilePosition.Value);
    }

    bool UpdateTileSelection()
    {
        if (!Input.GetMouseButtonDown(0) || EventSystem.current.IsPointerOverGameObject())
        {
            return false;
        }

        var prospectiveSelectedTilePosition = HoveredTilePosition;
        if (prospectiveSelectedTilePosition != null)
        {
            // Select the selectable at the tile, if one exists
            var prevSelectable = SelectedSelectable;
            var selectables = _gridManager.GetSelectables(prospectiveSelectedTilePosition.Value);
            Selectable prospectiveSelectable = null;
            if (SelectedSelectable != null)
            {
                var index = selectables.IndexOf(SelectedSelectable);
                if (index != -1)
                {
                    // Cycle through the selectables
                    prospectiveSelectable = selectables[(index + 1) % selectables.Count];
                }
                else
                {
                    prospectiveSelectable = selectables.FirstOrDefault();
                }
            }
            else
            {
                prospectiveSelectable = selectables.FirstOrDefault();
            }

            return Select(prospectiveSelectable);
        }

        return false;
    }

    public bool Select(Selectable selectable)
    {
        var positionForSelectable = _gridManager.PositionForSelectable(selectable);

        Selectable newlySelectedObject = null;
        Selectable newlyDeselectedObject = null;

        bool selectionDidChange = false;
        var prevSelectable = SelectedSelectable;

        if (selectable != prevSelectable)
        {
            selectionDidChange = true;
            newlyDeselectedObject = prevSelectable;
            SelectedSelectable = selectable;
            if (selectable != null)
            {
                if (positionForSelectable != null)
                {
                    SelectedTilePosition = positionForSelectable;
                    _tileSelectionGO.transform.position = _gridManager.TileCoordinateToWorldPosition(positionForSelectable.Value);
                    _tileSelectionGO.SetActive(true);
                }
                else
                {
                    SelectedTilePosition = null;
                    _tileSelectionGO.SetActive(false);
                }
                newlySelectedObject = SelectedSelectable;
            }
            else
            {
                Debug.Log("Cleared selection.");
            }
        }

        _tileSelectionGO.SetActive(SelectedTilePosition != null && ShowTileSelection);

        if (newlyDeselectedObject)
        {
            newlyDeselectedObject.Deselect();
        }

        if (newlySelectedObject)
        {
            newlySelectedObject.Select();
        }

        if (selectionDidChange)
        {
            UpdateSelectableVisuals();
            UpdateDefaultSelectedAction();
            GetComponent<PubSubSender>().Publish("player_input.selection.changed", newlySelectedObject);
        }

        return selectionDidChange;
    }

    public void UpdateDefaultSelectedAction()
    {
        var selectedEntity = SelectedEntity();
        if (selectedEntity != null)
        {
            if (selectedEntity.Owner == Entity.OwnerKind.Player)
            {
                var currentlySelectedActionIsStillValid = (selectedEntity.Actions.Contains(SelectedAction) && selectedEntity.CanAffordAction(SelectedAction));
                if (!currentlySelectedActionIsStillValid)
                {
                    foreach (var action in selectedEntity.Actions)
                    {
                        var waitingIsABlocker = selectedEntity.IsWaiting && !action.CanExecuteWhileWaiting;
                        if (selectedEntity.CanAffordAction(action) && !waitingIsABlocker)
                        {
                            SelectedAction = action;
                            break;
                        }
                    }
                }
            }
            else
            {
                SelectedAction = null;
            }
        }
        else
        {
            SelectedAction = null;
        }
    }

    void UpdateSelectedActionPathVisuals(bool forceRefresh = false)
    {
        var selectedEntity = SelectedEntity();
        var selectedAction = SelectedAction;

        if (forceRefresh)
        {
            _gridRangeIndicator.ClearPathVisuals(purgeCache: true);
        }

        if (selectedAction == null || selectedEntity == null || _turnManager.CurrentTeam != Entity.OwnerKind.Player)
        {
            _gridRangeIndicator.gameObject.SetActive(false);
            return;
        }

        if (selectedAction.Kind != Action.ActionKind.Movement || !selectedAction.Targetable)
        {
            _gridRangeIndicator.HidePath();
            return;
        }
        _gridRangeIndicator.ShowPath();

        Vector2Int? startPosition = SelectedEntity() != null ? SelectedEntity().Position : null;
        var endPosition = HoveredTilePosition;

        if (startPosition == null || endPosition == null) { return; }

        int range = selectedEntity.Range(selectedAction);
        GridRangeIndicator.Configuration configuration = new GridRangeIndicator.Configuration
        {
            range = range,
            origin = startPosition.Value
        };
        _gridRangeIndicator.gameObject.SetActive(true);
        _gridRangeIndicator.VisualizePath(startPosition.Value, endPosition.Value, configuration, _gridManager);
    }

    void UpdateSelectedActionRangeVisuals(bool forceRefresh = false)
    {
        var selectedEntity = SelectedEntity();
        var selectedAction = SelectedAction;

        if (forceRefresh)
        {
            _gridRangeIndicator.ClearRangeVisuals(purgeCache: true);
        }

        if (selectedAction == null || selectedEntity == null || _turnManager.CurrentTeam != Entity.OwnerKind.Player)
        {
            _gridRangeIndicator.gameObject.SetActive(false);
            return;
        }

        if (!selectedAction.Targetable)
        {
            _gridRangeIndicator.HideRange();
            return;
        }
        _gridRangeIndicator.ShowRange();

        int range = selectedEntity.Range(selectedAction);
        bool ignoringEntities = false;

        Dictionary<Entity.OwnerKind, Entity.OwnerAlignment> ownerToAlignmentMapping = null;
        if (selectedAction.Targetable && selectedAction.Kind == Action.ActionKind.Attack) {
            ignoringEntities = true;
            ownerToAlignmentMapping = selectedEntity.Owner.GetAlignmentMapping();
        }

        GridRangeIndicator.Configuration configuration = new GridRangeIndicator.Configuration
        {
            range = range,
            origin = selectedEntity.Position,
            ownerToAlignmentMapping = ownerToAlignmentMapping,
            ignoringEntities = ignoringEntities,
        };

        _gridRangeIndicator.gameObject.SetActive(true);
        _gridRangeIndicator.VisualizeRange(configuration, _gridManager);
    }

    public void RequestSelectEntity(PubSubListenerEvent e)
    {
        var entity = e.value as Entity;

        if (entity != null)
        {
            Select(entity.GetComponent<Selectable>());
        }
    }

    public void ForceVisualsRefresh()
    {
        var shouldClearVisuals = SelectedAction == null || SelectedEntity() == null || _turnManager.BlockingEventIsExecuting || !SelectedAction.Targetable;

        if (!shouldClearVisuals)
        {
            UpdateSelectedActionPathVisuals(forceRefresh: true);
            UpdateSelectedActionRangeVisuals(forceRefresh: true);
        }
    }
}
