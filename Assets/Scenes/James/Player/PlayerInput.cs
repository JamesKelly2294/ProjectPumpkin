using info.jacobingalls.jamkit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.EventSystems;
using static UnityEditor.Timeline.TimelinePlaybackControls;
using static UnityEngine.GraphicsBuffer;

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
    void Start()
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

        if (entity == null || SelectedAction == null || !entity.CanAffordAction(SelectedAction))
        {
            return;
        }
        Debug.Log("Executing selected action!");

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

        _turnManager.SubmitAction(SelectedAction, context);
    }

    public void RequestActionSelection(PubSubListenerEvent e)
    {
        ActionSelectionRequest asr = e.value as ActionSelectionRequest;

        SelectedSelectable = asr.Entity.GetComponent<Selectable>();
        SelectedAction = asr.Action;

        if (!asr.Action.Targetable)
        {
            ExecuteSelectedAction();
        }
    }

    private void Update()
    {
        if (UserInteractionEnabled == false) { return; }

        UpdateTileHighlight();
        UpdateTileSelection();

        if (SelectedAction != null)
        {
            var entity = SelectedEntity();
            if (!entity.CanAffordAction(SelectedAction))
            {
                Debug.Log("Nullifying action");
                SelectedAction = null;
            }
            else
            {
                Debug.Log("UpdateSelectedActionRangeVisuals");
                UpdateSelectedActionRangeVisuals();
                UpdateSelectedActionPathVisuals();
            }
        }

        if (SelectedAction == null || SelectedEntity() == null || SelectedEntity().IsBusy || !SelectedAction.Targetable)
        {
            _gridRangeIndicator.ClearRangeVisuals();
            _gridRangeIndicator.ClearPathVisuals();
        }

        if (CanExecuteAction() && Input.GetMouseButtonUp(1))
        {
            ExecuteSelectedAction();
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
        }

        return selectionDidChange;
    }

    void UpdateDefaultSelectedAction()
    {
        var selectedEntity = SelectedEntity();
        if (selectedEntity != null)
        {
            SelectedAction = selectedEntity.Actions.FirstOrDefault();
        }
        else
        {
            SelectedAction = null;
        }
    }

    void UpdateSelectedActionPathVisuals()
    {
        var selectedEntity = SelectedEntity();
        var selectedAction = SelectedAction;
        if (selectedAction == null || selectedEntity == null)
        {
            _gridRangeIndicator.gameObject.SetActive(false);
            return;
        }

        if (selectedAction.Kind != Action.ActionKind.Movement)
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

    void UpdateSelectedActionRangeVisuals()
    {
        var selectedEntity = SelectedEntity();
        var selectedAction = SelectedAction;
        if (selectedAction == null || selectedEntity == null)
        {
            _gridRangeIndicator.gameObject.SetActive(false);
            return;
        }

        int range = selectedEntity.Range(selectedAction);
        GridRangeIndicator.Configuration configuration = new GridRangeIndicator.Configuration
        {
            range = range,
            origin = selectedEntity.Position
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
}
