using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class GridManager : MonoBehaviour
{

    [Header("Tilemap")]
    public Tilemap Walls;
    public Tilemap Walkable;

    public GameObject TileOutlinePrefab;
    public GameObject TileHoverPrefab;
    public GameObject TileSelectionPrefab;

    [Header("Pathing")]
    public GameObject PathStartPrefab;
    public GameObject PathEndPrefab;
    public GameObject PathNodePrefab;

    private GameObject _tileSelectionGO;
    private GameObject _tileHoverGO;
    private GameObject _pathStartGO;
    private GameObject _pathEndGO;
    private GameObject _pathGroupGO;
    private GameObject _tileOutlinesGroupGO;

    public Vector2Int? SelectedTilePosition = null;
    public Vector2Int? HoveredTilePosition = null;
    private Vector2Int? PathStartPosition = null;
    private Vector2Int? PathEndPosition = null;

    public Selectable SelectedSelectable { get; private set; }

    private const float _tileCenterOffset = 0.5f;

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

        _pathStartGO = Instantiate(PathStartPrefab);
        _pathStartGO.transform.name = "Path Start";
        _pathStartGO.transform.parent = transform;
        _pathStartGO.SetActive(false);

        _pathEndGO = Instantiate(PathEndPrefab);
        _pathEndGO.transform.name = "Path End";
        _pathEndGO.transform.parent = transform;
        _pathEndGO.SetActive(false);

        _tileOutlinesGroupGO = new GameObject("Tile Outlines");
        _tileOutlinesGroupGO.transform.parent = transform;

        _pathGroupGO = new GameObject("Path");
        _pathGroupGO.transform.parent = transform;

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
        var tiles = Walkable.GetTilesBlock(new BoundsInt(minX, minY, 0, width, height, 1));

        for (int i = 0; i < tiles.Length; i++)
        {
            var x = (i % width) + minX;
            var y = (i / width) + minY;
            var tile = tiles[i];
            if (tile != null)
            {
                var tileOutline = Instantiate(TileOutlinePrefab);
                tileOutline.transform.position = TileCoordinateToWorldPosition(new Vector2Int(x, y));
                tileOutline.transform.name = "(" + x + ", " + y + ")";
                tileOutline.transform.parent = _tileOutlinesGroupGO.transform;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateTileHighlight();
        UpdateTileSelection();
    }

    public bool UserInteractionEnabled = true;

    void UpdateTileHighlight()
    {
        if (UserInteractionEnabled == false)
        {
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

        _tileHoverGO.SetActive(true);
        _tileHoverGO.transform.position = TileCoordinateToWorldPosition(HoveredTilePosition.Value);
    }

    void UpdateTileSelection()
    {
        if (!Input.GetMouseButtonDown(0))
        {
            return;
        }

        var prospectiveSelectedTilePosition = HoveredTilePosition;

        Selectable newlySelectedObject = null;
        Selectable newlyDeselectedObject = null;

        if (prospectiveSelectedTilePosition != null)
        {
            // Select the selectable at the tile, if one exists
            var prevSelectable = SelectedSelectable;
            var selectables = GetSelectables(prospectiveSelectedTilePosition.Value);
            if (SelectedSelectable != null)
            {
                var index = selectables.IndexOf(SelectedSelectable);
                if (index != -1)
                {
                    // Cycle through the selectables
                    SelectedSelectable = selectables[(index + 1) % selectables.Count];
                }
                else
                {
                    SelectedSelectable = selectables.FirstOrDefault();
                }
            }
            else
            {
                SelectedSelectable = selectables.FirstOrDefault();
            }

            if (SelectedSelectable != prevSelectable)
            {
                newlyDeselectedObject = prevSelectable;
                if (SelectedSelectable != null)
                {
                    Debug.Log("Selected " + SelectedSelectable.gameObject + ".");
                    SelectedTilePosition = prospectiveSelectedTilePosition;
                    _tileSelectionGO.transform.position = TileCoordinateToWorldPosition(prospectiveSelectedTilePosition.Value);
                    newlySelectedObject = SelectedSelectable;
                }
                else
                {
                    Debug.Log("Cleared selection.");
                }

                ClearPath();
            }

            _tileSelectionGO.SetActive(SelectedSelectable != null);
        }
        else
        {
            _tileSelectionGO.SetActive(false);
        }

        if (newlyDeselectedObject)
        {
            newlyDeselectedObject.Deselect();
        }

        if (newlySelectedObject)
        {
            newlySelectedObject.Select();
        }
    }

    private Dictionary<Vector2Int, Entity> entities = new Dictionary<Vector2Int, Entity>();

    // An ordered list of selectables at a given tile.
    public List<Selectable> GetSelectables(Vector2Int position)
    {
        var selectables = new List<Selectable>();

        if (entities.ContainsKey(position))
        {
            var entity = entities[position];
            var selectable = entity.GetComponent<Selectable>();
            if (selectable != null) { selectables.Add(selectable); }
        }

        return selectables;
    }

    public void RegisterEntity(Entity entity, Vector2Int position)
    {
        if (entities.ContainsKey(position))
        {
            Debug.LogError("Cannot register entity " + entity + " at " + position + " as " + entities[position] + " is already occupying that space!");
            return;
        }

        if (!Walkable.HasTile((Vector3Int)position))
        {
            Debug.LogError("Cannot register entity " + entity + " at " + position + " as a walkable tile does not exist there!");
            return;
        }

        entities[position] = entity;
        SnapEntityToGrid(entity, position);
        Debug.Log("Registered " + entity + " at " + position + ".");
    }

    public Entity GetEntity(Vector2Int position)
    {
        return entities[position];
    }

    public bool HasEntity(Vector2Int position)
    {
        return entities.ContainsKey(position);
    }

    void SnapEntityToGrid(Entity entity, Vector2Int position)
    {
        entity.transform.position = TileCoordinateToWorldPosition(position);
    }

    void SelectPath()
    {
        ClearPath();
        if (PathStartPosition == null || (HoveredTilePosition == PathStartPosition && PathEndPosition == null))
        {
            SelectPathStartPosition(HoveredTilePosition);
        }
        else
        {
            SelectPathEndPosition(HoveredTilePosition);
        }

        if (PathStartPosition != null && PathEndPosition != null)
        {
            CalculatePath((Vector3Int)PathStartPosition.Value, (Vector3Int)PathEndPosition.Value);
        }
    }

    void SelectPathStartPosition(Vector2Int? position)
    {
        if (position == PathStartPosition)
        {
            position = null;
        }

        PathStartPosition = position;

        if (PathStartPosition != null)
        {
            _pathStartGO.transform.position = TileCoordinateToWorldPosition(PathStartPosition.Value);
            _pathStartGO.SetActive(true);
        }
        else
        {
            _pathStartGO.SetActive(false);
        }
    }

    void SelectPathEndPosition(Vector2Int? position)
    {
        if (position == PathEndPosition || position == PathStartPosition)
        {
            position = null;
        }

        PathEndPosition = position;

        if (PathEndPosition != null)
        {
            _pathEndGO.transform.position = TileCoordinateToWorldPosition(PathEndPosition.Value);
            _pathEndGO.SetActive(true);
        }
        else
        {
            _pathEndGO.SetActive(false);
        }
    }

    void ClearPath()
    {
        for (int i = 0; i < _pathGroupGO.transform.childCount; i++)
        {
            Destroy(_pathGroupGO.transform.GetChild(i).gameObject);
        }
    }

    public List<Vector2Int> CalculatePath(Vector2Int startPosition, Vector2Int endPosition, bool debugVisuals = false) 
    {
        return CalculatePath((Vector3Int)startPosition, (Vector3Int)endPosition, debugVisuals: debugVisuals);
    }


    public List<Vector2Int> CalculatePath(Vector3Int startPosition, Vector3Int endPosition, bool debugVisuals=false)
    {
        if (startPosition == endPosition)
        {
            var quickPath = new List<Vector2Int>();
            quickPath.Add((Vector2Int)startPosition);
            return quickPath;
        }

#if DEBUG
        Debug.Log("Calculate path from " + startPosition + " to " + endPosition);
#endif

        Dictionary<Vector3Int, Vector3Int> visited = new Dictionary<Vector3Int, Vector3Int>();
        Queue<Vector3Int> queue = new Queue<Vector3Int>();

        ClearPath();

        foreach (var neighbor in NeighborsForTileAtPosition(startPosition))
        {
            visited[neighbor] = startPosition;
            queue.Enqueue(neighbor);
        }

        bool pathFound = false;
        while (queue.Count > 0)
        {
            var currentTile = queue.Dequeue();
            if (currentTile == endPosition) { 
                pathFound = true;
                break;
            }
            else
            {
                var neighbors = NeighborsForTileAtPosition(currentTile);
                foreach (var neighbor in neighbors)
                {
                    if (visited.ContainsKey(neighbor)) { continue; }
                    visited[neighbor] = currentTile;
                    queue.Enqueue(neighbor);
                }
            }
        }

        if (!pathFound)
        {
            return null;
        }

        var path = BuildPath(startPosition, endPosition, visited);

        if (debugVisuals)
        {
            foreach (var node in path)
            {
                var pathVisual = Instantiate(PathNodePrefab);
                pathVisual.transform.position = TileCoordinateToWorldPosition((Vector2Int)node);
                pathVisual.transform.name = "(" + node.x + ", " + node.y + ")";
                pathVisual.transform.parent = _pathGroupGO.transform;
            }
        }

        return path;
    }

    List<Vector2Int> BuildPath(Vector3Int startPosition, Vector3Int endPosition, Dictionary<Vector3Int, Vector3Int> visited)
    {
        List<Vector2Int> path = new List<Vector2Int>();

        var currentTile = endPosition;

        while (currentTile != startPosition) {
            path.Add((Vector2Int)currentTile);
            currentTile = visited[currentTile];
        }
        path.Add((Vector2Int)startPosition);

        path.Reverse();
        return path;
    }

    bool TileIsWalkable(Vector3Int tilePosition)
    {
        return Walkable.HasTile(tilePosition) &&
            !HasEntity((Vector2Int)tilePosition);
    }

    List<Vector3Int> NeighborsForTileAtPosition(Vector3Int tilePosition, bool includeDiagonal = false)
    {
        var neighborPositions = new List<Vector3Int>();

        var northPos = tilePosition + new Vector3Int(0, -1, 0);
        var eastPos = tilePosition + new Vector3Int(1, 0, 0);
        var southPos = tilePosition + new Vector3Int(0, 1, 0);
        var westPos = tilePosition + new Vector3Int(-1, 0, 0);
        if (TileIsWalkable(northPos)) { neighborPositions.Add(northPos); }
        if (TileIsWalkable(eastPos)) { neighborPositions.Add(eastPos); }
        if (TileIsWalkable(southPos)) { neighborPositions.Add(southPos); }
        if (TileIsWalkable(westPos)) { neighborPositions.Add(westPos); }

        if (includeDiagonal)
        {
            var northEastPos = tilePosition + new Vector3Int(1, -1, 0);
            var southEastPos = tilePosition + new Vector3Int(1, 1, 0);
            var northWestPos = tilePosition + new Vector3Int(-1, -1, 0);
            var southWestPos = tilePosition + new Vector3Int(-1, 1, 0);
            if (TileIsWalkable(northEastPos)) { neighborPositions.Add(northEastPos); }
            if (TileIsWalkable(southEastPos)) { neighborPositions.Add(southEastPos); }
            if (TileIsWalkable(northWestPos)) { neighborPositions.Add(northWestPos); }
            if (TileIsWalkable(southWestPos)) { neighborPositions.Add(southWestPos); }
        }

        return neighborPositions;
    }

    Vector3 TileCoordinateToWorldPosition(Vector2Int position)
    {
        return new Vector3(position.x + _tileCenterOffset, position.y + _tileCenterOffset, 0.0f);
    }
}
