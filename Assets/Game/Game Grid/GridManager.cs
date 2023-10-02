using info.jacobingalls.jamkit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using static UnityEngine.RuleTile.TilingRuleOutput;

public struct TileData
{
    public Vector2Int Position;

    public Entity Entity;

    public GridManager GridManager;

    public bool IsEmpty()
    {
        return Entity == null && IsWalkable();
    }

    public bool IsWalkable()
    {
        return GridManager.Walkable.HasTile((Vector3Int)Position);
    }

    public List<Selectable> Selectables
    {
        get
        {
            var selectables = new List<Selectable>();

            if (Entity != null && Entity.TryGetComponent<Selectable>(out var entitySelectable))
            {
               selectables.Add(entitySelectable);
            }

            return selectables;
        }
    }
}

[RequireComponent(typeof(PubSubSender))]
public class GridManager : MonoBehaviour
{

    [Header("Tilemap")]
    public Tilemap Walls;
    public Tilemap Walkable;

    private const float _tileCenterOffset = 0.5f;

    private Dictionary<Vector2Int, TileData> _tileData = new();
    private Dictionary<object, Vector2Int> _objectTilePositions = new();

    public HashSet<Entity> Entities { get { return _entities; } }
    private HashSet<Entity> _entities = new();
    private PubSubSender _pubSubSender;


    // Start is called before the first frame update
    void Awake()
    {
        _pubSubSender = GetComponent<PubSubSender>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public TileData GetTileData(Vector2Int position)
    {
        if (_tileData.ContainsKey(position))
        {
            return _tileData[position];
        }
        else
        {
            var tileData = new TileData
            {
                Position = position,
                GridManager = this,
            };
            return tileData;
        }
    }

    public void UpdateTileData(TileData newTileData)
    {
        if (newTileData.IsEmpty())
        {
            if (_tileData.ContainsKey(newTileData.Position))
            {
                _tileData.Remove(newTileData.Position);
            }
        }
        else
        {
            _tileData[newTileData.Position] = newTileData;
        }

        if (_pubSubSender == null) { _pubSubSender = GetComponent<PubSubSender>(); }
        _pubSubSender.Publish("grid.tile.updated");
    }

    // An ordered list of selectables at a given tile.
    public List<Selectable> GetSelectables(Vector2Int position)
    {
        var selectables = new List<Selectable>();

        var tile = GetTileData(position);

        // Get the Entity, if it exists
        if (tile.Entity != null && tile.Entity.TryGetComponent<Selectable>(out var selectable)) { selectables.Add(selectable); }

        return selectables;
    }

    public Vector2Int? PositionForSelectable(Selectable selectable)
    {
        // This whole method is gross.
        foreach(var (pos, tile) in _tileData)
        {
            foreach (var s in tile.Selectables)
            {
                if (s == selectable) { return pos; }
            }
        }

        return null;
    }

    public Vector2Int PositionForEntity(Entity entity)
    {
        return _objectTilePositions[entity];
    }

    public void RegisterEntity(Entity entity, Vector2Int position)
    {
        var tileData = GetTileData(position);
        if (tileData.Entity != null)
        {
            Debug.LogError("Cannot register entity " + entity + " at " + position + " as " + tileData.Entity + " is already occupying that space!");
            return;
        }

        if (!Walkable.HasTile((Vector3Int)position))
        {
            Debug.LogError("Cannot register entity " + entity + " at " + position + " as a walkable tile does not exist there!");
            return;
        }

        _objectTilePositions[entity] = position;
        _entities.Add(entity);

        SnapEntityToGrid(entity, position);
        _setEntityPositions_unsafe(entity, position);

        Debug.Log("Registered " + entity + " at " + position + ".");

        _pubSubSender.Publish("grid.entity.registered");
    }

    public void UnregisterEntity(Entity entity)
    {
        var position = _objectTilePositions[entity];
        _objectTilePositions.Remove(entity);
        _entities.Remove(entity);
        
        var data = GetTileData(position);
        data.Entity = null;
        UpdateTileData(data);

        _pubSubSender.Publish("grid.entity.unregistered");
    }

    public void SetEntityPosition(Entity entity, Vector2Int position)
    {
        if (!_objectTilePositions.ContainsKey(entity))
        {
            Debug.Log("Unable to set position for entity that has not be registered.");
            return;
        }

        _setEntityPositions_unsafe(entity, position);
    }

    private void _setEntityPositions_unsafe(Entity entity, Vector2Int position)
    {
        var oldPosition = _objectTilePositions[entity];
        _objectTilePositions[entity] = position;

        var oldTileData = GetTileData(oldPosition);
        oldTileData.Entity = null;
        UpdateTileData(oldTileData);

        var newTileData = GetTileData(position);
        newTileData.Entity = entity;
        UpdateTileData(newTileData);
    }

    public Entity GetEntity(Vector2Int position)
    { 
        return GetTileData(position).Entity;
    }

    public bool HasEntity(Vector2Int position)
    {
        return GetTileData(position).Entity != null;
    }

    void SnapEntityToGrid(Entity entity, Vector2Int position)
    {
        entity.transform.position = TileCoordinateToWorldPosition(position);
    }

    public List<Vector2Int> CalculatePath(Vector2Int startPosition, Vector2Int endPosition, int range = 0, bool alwaysIncludeTarget = false, bool debugVisuals = false, bool ignoringObstacles = false) 
    {
        return CalculatePath((Vector3Int)startPosition, (Vector3Int)endPosition, range:range, debugVisuals: debugVisuals, alwaysIncludeTarget: alwaysIncludeTarget, ignoringObstacles: ignoringObstacles);
    }


    public List<Vector2Int> CalculatePath(Vector3Int startPosition, Vector3Int endPosition, int range = 0, bool alwaysIncludeTarget = false, bool debugVisuals = false, bool ignoringObstacles = false)
    {
        if (startPosition == endPosition)
        {
            var quickPath = new List<Vector2Int>();
            quickPath.Add((Vector2Int)startPosition);
            return quickPath;
        }

        Dictionary<Vector3Int, Vector3Int> visited = new Dictionary<Vector3Int, Vector3Int>();
        Queue<Vector3Int> queue = new Queue<Vector3Int>();

        foreach (var neighbor in NeighborsForTileAtPosition(startPosition, target: endPosition, alwaysIncludeTarget: alwaysIncludeTarget, ignoringObstacles: ignoringObstacles))
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
                var neighbors = NeighborsForTileAtPosition(currentTile, target: endPosition, alwaysIncludeTarget: alwaysIncludeTarget, ignoringObstacles: ignoringObstacles);
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
            return new List<Vector2Int>();
        }

        var path = BuildPath(startPosition, endPosition, visited);

        if (range > 0)
        {
            path = path.GetRange(0, Mathf.Min(range + 1, path.Count));
        }

        return path;
    }

    public HashSet<Vector2Int> BFS(Vector3Int startPosition, int range, bool ignoringObstacles = false)
    {
        if (range < 0)
        {
            Debug.LogError("Bruh.");
            return new HashSet<Vector2Int>();
        }

        if (range == 0)
        {
            var quickPath = new HashSet<Vector2Int>();
            quickPath.Add((Vector2Int)startPosition);
            return quickPath;
        }

        Dictionary<Vector3Int, int> visited = new Dictionary<Vector3Int, int>();
        Queue<Vector3Int> queue = new Queue<Vector3Int>();

        foreach (var neighbor in NeighborsForTileAtPosition(startPosition, ignoringObstacles: ignoringObstacles))
        {
            visited[neighbor] = 1;
            queue.Enqueue(neighbor);
        }

        while (queue.Count > 0)
        {
            var currentTile = queue.Dequeue();
            if (visited[currentTile] > range)
            {
                break;
            }
            else
            {
                var neighbors = NeighborsForTileAtPosition(currentTile, ignoringObstacles: ignoringObstacles);
                foreach (var neighbor in neighbors)
                {
                    if (visited.ContainsKey(neighbor)) { continue; }
                    visited[neighbor] = visited[currentTile] + 1;
                    queue.Enqueue(neighbor);
                }
            }
        }

        HashSet<Vector2Int> results = new();
        foreach (var (position, distance) in visited)
        {
            if (distance <= range)
            {
                results.Add((Vector2Int)position);
            }
        }

        return results;
    }

    public List<Vector2Int> OrderedBFS(Vector3Int startPosition, int range, bool ignoringObstacles = false)
    {
        if (range < 0)
        {
            Debug.LogError("Bruh.");
            return new List<Vector2Int>();
        }

        if (range == 0)
        {
            var quickPath = new List<Vector2Int>();
            quickPath.Add((Vector2Int)startPosition);
            return quickPath;
        }

        List<Vector2Int> results = new();
        Dictionary<Vector3Int, int> visited = new Dictionary<Vector3Int, int>();
        Queue<Vector3Int> queue = new Queue<Vector3Int>();

        foreach (var neighbor in NeighborsForTileAtPosition(startPosition, ignoringObstacles: ignoringObstacles))
        {
            visited[neighbor] = 1;
            queue.Enqueue(neighbor);
        }

        while (queue.Count > 0)
        {
            var currentTile = queue.Dequeue();
            if (visited[currentTile] > range)
            {
                break;
            }
            else
            {
                results.Add((Vector2Int)currentTile);
                var neighbors = NeighborsForTileAtPosition(currentTile, ignoringObstacles: ignoringObstacles);
                foreach (var neighbor in neighbors)
                {
                    if (visited.ContainsKey(neighbor)) { continue; }
                    visited[neighbor] = visited[currentTile] + 1;
                    queue.Enqueue(neighbor);
                }
            }
        }

        return results;
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

    bool TileIsWalkable(Vector3Int tilePosition, Vector3Int? target, bool alwaysIncludeTarget, bool ignoringObstacles)
    {
        var tileExists = Walkable.HasTile(tilePosition);

        var obstaclesInTheWay = false;
        if (!ignoringObstacles) {
            obstaclesInTheWay = HasEntity((Vector2Int)tilePosition);
        }

        var overrideObstacleCheck = false;
        if (target != null && alwaysIncludeTarget)
        {
            overrideObstacleCheck = (target.Value == tilePosition);
        }

        return tileExists && (!obstaclesInTheWay || overrideObstacleCheck);
    }

    List<Vector3Int> NeighborsForTileAtPosition(Vector3Int tilePosition, Vector3Int? target = null, bool includeDiagonal = false, bool alwaysIncludeTarget = false, bool ignoringObstacles = false)
    {
        var neighborPositions = new List<Vector3Int>();

        var northPos = tilePosition + new Vector3Int(0, -1, 0);
        var eastPos = tilePosition + new Vector3Int(1, 0, 0);
        var southPos = tilePosition + new Vector3Int(0, 1, 0);
        var westPos = tilePosition + new Vector3Int(-1, 0, 0);
        if (TileIsWalkable(northPos, target, alwaysIncludeTarget, ignoringObstacles)) { neighborPositions.Add(northPos); }
        if (TileIsWalkable(eastPos, target, alwaysIncludeTarget, ignoringObstacles)) { neighborPositions.Add(eastPos); }
        if (TileIsWalkable(southPos, target, alwaysIncludeTarget, ignoringObstacles)) { neighborPositions.Add(southPos); }
        if (TileIsWalkable(westPos, target, alwaysIncludeTarget, ignoringObstacles)) { neighborPositions.Add(westPos); }

        if (includeDiagonal)
        {
            var northEastPos = tilePosition + new Vector3Int(1, -1, 0);
            var southEastPos = tilePosition + new Vector3Int(1, 1, 0);
            var northWestPos = tilePosition + new Vector3Int(-1, -1, 0);
            var southWestPos = tilePosition + new Vector3Int(-1, 1, 0);
            if (TileIsWalkable(northEastPos, target, alwaysIncludeTarget, ignoringObstacles)) { neighborPositions.Add(northEastPos); }
            if (TileIsWalkable(southEastPos, target, alwaysIncludeTarget, ignoringObstacles)) { neighborPositions.Add(southEastPos); }
            if (TileIsWalkable(northWestPos, target, alwaysIncludeTarget, ignoringObstacles)) { neighborPositions.Add(northWestPos); }
            if (TileIsWalkable(southWestPos, target, alwaysIncludeTarget, ignoringObstacles)) { neighborPositions.Add(southWestPos); }
        }

        return neighborPositions;
    }

    public Vector3 TileCoordinateToWorldPosition(Vector2Int position)
    {
        return new Vector3(position.x + _tileCenterOffset, position.y + _tileCenterOffset, 0.0f);
    }
}
