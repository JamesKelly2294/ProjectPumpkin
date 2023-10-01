using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

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

    public bool ShowTileHover = true;
    public bool ShowTileSelection = false;

    private GameObject _entitySelectionGO;
    private GameObject _tileSelectionGO;
    private GameObject _tileHoverGO;
    private GameObject _tileOutlinesGroupGO;

    private Vector2Int? PathStartPosition = null;
    private Vector2Int? PathEndPosition = null;

    private GridManager _gridManager;
    private TurnManager _turnManager;
    private CameraControls _cameraControls;
    private Selectable _selectable;

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

        var vertExtent = Camera.main.orthographicSize;
        var horzExtent = vertExtent * Screen.width / Screen.height;

        // Calculations assume map is position at the origin
        var cameraMinX = Mathf.RoundToInt(transform.position.x - horzExtent);
        var cameraMaxX = Mathf.RoundToInt(transform.position.x + horzExtent);
        var cameraMinY = Mathf.RoundToInt(transform.position.y - vertExtent);
        var cameraMaxY = Mathf.RoundToInt(transform.position.y + vertExtent);

        var minX = cameraMinX;
        var minY = cameraMinY;
        var width = cameraMaxX - cameraMinX + 1;
        var height = cameraMaxY - cameraMinY + 1;
        var tiles = _gridManager.Walkable.GetTilesBlock(new BoundsInt(minX, minY, 0, width, height, 1));

        for (int i = 0; i < tiles.Length; i++)
        {
            var x = (i % width) + minX;
            var y = (i / width) + minY;
            var tile = tiles[i];
            if (tile != null)
            {
                var tileOutline = Instantiate(TileOutlinePrefab);
                tileOutline.transform.position = _gridManager.TileCoordinateToWorldPosition(new Vector2Int(x, y));
                tileOutline.transform.name = "(" + x + ", " + y + ")";
                tileOutline.transform.parent = _tileOutlinesGroupGO.transform;
            }
        }



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

    private bool CanTakePrimaryAction()
    {
        return false;
    }

    private void TakePrimaryAction()
    {
        // THIS IS A TEST
        //Debug.Log("Taking primary action!");

        //var entity = SelectedEntity();

        //if (entity != null)
        //{
        //    entity.ExecuteAction(entity.Actions.First(), ignoringCost: true);
        //}
    }

    private bool CanTakeSecondaryAction()
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

    private void TakeSecondaryAction()
    {
        Debug.Log("Taking secondary action!");

        var highlitedTilePosition = HoveredTilePosition.Value;

        var entity = SelectedEntity();

        if (entity != null)
        {
            var entityPos = entity.Position;
            var targetPos = highlitedTilePosition;

            var path = _gridManager.CalculatePath(entityPos, targetPos, debugVisuals: false);
            entity.Move(path);
        }
    }

    private bool SkipUpdate
    {
        get
        {
            return _selectable == null;
        }
    }

    private void Update()
    {
        if (UserInteractionEnabled == false) { return; }

        UpdateTileHighlight();
        UpdateTileSelection();

        if (SkipUpdate) { return; }

        if (CanTakePrimaryAction() && Input.GetMouseButtonUp(0))
        {
            TakePrimaryAction();
        }

        if (CanTakeSecondaryAction() && Input.GetMouseButtonUp(1))
        {
            TakeSecondaryAction();
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
        }

        return selectionDidChange;
    }

    //void SelectPath()
    //{
    //    ClearPath();
    //    if (PathStartPosition == null || (HoveredTilePosition == PathStartPosition && PathEndPosition == null))
    //    {
    //        SelectPathStartPosition(HoveredTilePosition);
    //    }
    //    else
    //    {
    //        SelectPathEndPosition(HoveredTilePosition);
    //    }

    //    if (PathStartPosition != null && PathEndPosition != null)
    //    {
    //        CalculatePath((Vector3Int)PathStartPosition.Value, (Vector3Int)PathEndPosition.Value);
    //    }
    //}

    //void SelectPathStartPosition(Vector2Int? position)
    //{
    //    if (position == PathStartPosition)
    //    {
    //        position = null;
    //    }

    //    PathStartPosition = position;

    //    if (PathStartPosition != null)
    //    {
    //        _pathStartGO.transform.position = TileCoordinateToWorldPosition(PathStartPosition.Value);
    //        _pathStartGO.SetActive(true);
    //    }
    //    else
    //    {
    //        _pathStartGO.SetActive(false);
    //    }
    //}

    //void SelectPathEndPosition(Vector2Int? position)
    //{
    //    if (position == PathEndPosition || position == PathStartPosition)
    //    {
    //        position = null;
    //    }

    //    PathEndPosition = position;

    //    if (PathEndPosition != null)
    //    {
    //        _pathEndGO.transform.position = TileCoordinateToWorldPosition(PathEndPosition.Value);
    //        _pathEndGO.SetActive(true);
    //    }
    //    else
    //    {
    //        _pathEndGO.SetActive(false);
    //    }
    //}
}
