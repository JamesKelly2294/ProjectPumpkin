using System;
using System.Collections.Generic;
using UnityEngine;

public class GridRangeIndicator : MonoBehaviour
{
    public struct Configuration : IEquatable<Configuration>
    {
        public Vector2Int origin;
        public int range;
        public bool ignoringEntities;

        public Dictionary<Entity.OwnerKind, Entity.OwnerAlignment> ownerToAlignmentMapping;

        public override bool Equals(object? obj) => obj is Configuration other && this.Equals(other);

        public bool Equals(Configuration p) => origin == p.origin && range == p.range;

        public override int GetHashCode() => (origin, range).GetHashCode();

        public static bool operator ==(Configuration lhs, Configuration rhs) => lhs.Equals(rhs);

        public static bool operator !=(Configuration lhs, Configuration rhs) => !(lhs == rhs);
    }

    public GameObject TileOutlinePrefab;


    [Header("Pathing")]
    public GameObject PathStartPrefab;
    public GameObject PathEndPrefab;
    public GameObject PathNodePrefab;

    public Color ReachablePathNodeColor = Color.green;
    public Color UnreachablePathNodeColor = Color.red;

    public Color AlliedEntityColor = Color.green;
    public Color EnemyyEntityColor = Color.red;
    public Color AttackBaseColor = Color.red;

    private GameObject _pathStartGO;
    private GameObject _pathEndGO;
    private GameObject _pathGroupGO;

    private GameObject _tileOutlineContainerGO;

    private Vector2Int? PathStartPosition = null;
    private Vector2Int? PathEndPosition = null;

    private Configuration? _cachedPathConfiguration;
    private Configuration? _cachedRangeConfiguration;

    public void Awake()
    {
        _pathStartGO = Instantiate(PathStartPrefab);
        _pathStartGO.transform.name = "Path Start";
        _pathStartGO.transform.parent = transform;
        _pathStartGO.SetActive(false);

        _pathEndGO = Instantiate(PathEndPrefab);
        _pathEndGO.transform.name = "Path End";
        _pathEndGO.transform.parent = transform;
        _pathEndGO.SetActive(false);

        _pathGroupGO = new GameObject("Path");
        _pathGroupGO.transform.parent = transform;

        _tileOutlineContainerGO = new GameObject("Tile Outline Container");
        _tileOutlineContainerGO.transform.parent = transform;
    }

    public void ClearRangeVisuals(bool purgeCache = true)
    {
        for (int i = 0; i < _tileOutlineContainerGO.transform.childCount; i++)
        {
            Destroy(_tileOutlineContainerGO.transform.GetChild(i).gameObject);
        }
        if (purgeCache) { _cachedRangeConfiguration = null; }
    }

    public void ClearPathVisuals(bool purgeCache = true)
    {
        _pathStartGO.SetActive(false);
        _pathEndGO.SetActive(false);

        for (int i = 0; i < _pathGroupGO.transform.childCount; i++)
        {
            Destroy(_pathGroupGO.transform.GetChild(i).gameObject);
        }

        PathStartPosition = null;
        PathEndPosition = null;
        if (purgeCache) { _cachedPathConfiguration = null; }
    }

    public void HidePath()
    {
        SetPathActive(false);

    }

    public void ShowPath()
    {
        SetPathActive(true);
    }

    public void SetPathActive(bool active)
    {
        //_pathStartGO.SetActive(active);
        //_pathEndGO.SetActive(active);
        _pathGroupGO.SetActive(active);
    }

    public void HideRange()
    {
        SetRangeActive(false);

    }

    public void ShowRange()
    {
        SetRangeActive(true);
    }

    public void SetRangeActive(bool active)
    {
        _tileOutlineContainerGO.SetActive(active);
    }

    public GameObject CreateTileOutline(GridManager gridManager, int x, int y, Configuration configuration)
    {
        var tileOutline = Instantiate(TileOutlinePrefab);
        tileOutline.transform.position = gridManager.TileCoordinateToWorldPosition(new Vector2Int(x, y));
        tileOutline.transform.name = "(" + x + ", " + y + ")";
        tileOutline.transform.parent = _tileOutlineContainerGO.transform;

        var ownerToAlignmentMapping = configuration.ownerToAlignmentMapping;
        if (ownerToAlignmentMapping != null)
        {
            var entity = gridManager.GetTileData(new Vector2Int(x, y)).Entity;
            if (entity != null)
            {
                var alignment = Entity.OwnerAlignment.Neutral;

                if (ownerToAlignmentMapping.ContainsKey(entity.Owner))
                {
                    alignment = ownerToAlignmentMapping[entity.Owner];
                }

                switch (alignment)
                {
                    case Entity.OwnerAlignment.Good:
                        tileOutline.GetComponent<SpriteRenderer>().color = AlliedEntityColor;
                        break;
                    case Entity.OwnerAlignment.Bad:
                        tileOutline.GetComponent<SpriteRenderer>().color = EnemyyEntityColor;
                        break;
                    default:
                        break;
                }
            }
            else
            {
                // FIXME, HACK, NEEDS MORE INFO CONFIGURATION
                tileOutline.GetComponent<SpriteRenderer>().color = AttackBaseColor;
            }
        }

        return tileOutline;
    }

    public GameObject CreatePathTile(GridManager gridManager, int x, int y, int index, Configuration configuration)
    {
        var tileOutline = Instantiate(PathNodePrefab);
        tileOutline.transform.position = gridManager.TileCoordinateToWorldPosition(new Vector2Int(x, y));
        tileOutline.transform.name = "(" + x + ", " + y + ")";
        tileOutline.transform.parent = _pathGroupGO.transform;

        var sprite = tileOutline.GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            bool reachable = index <= configuration.range;
            sprite.color = reachable ? ReachablePathNodeColor : UnreachablePathNodeColor;
        }

        return tileOutline;
    }

    public void VisualizeRange(Configuration configuration, GridManager gridManager)
    {
        if (_cachedRangeConfiguration != null)
        {
            var cachedConfig = _cachedRangeConfiguration.Value;

            if (cachedConfig.Equals(configuration))
            {
                return;
            }
        }

        _cachedRangeConfiguration = configuration;
        ClearRangeVisuals(purgeCache: false);

        var tiles = gridManager.BFS((Vector3Int)configuration.origin, configuration.range, ignoringObstacles: configuration.ignoringEntities);
        int i = 0;
        foreach (var tile in tiles)
        { 
            CreateTileOutline(gridManager, tile.x, tile.y, configuration);
        }
    }

    public void VisualizePath(Vector2Int startPosition, Vector2Int endPosition, Configuration configuration, GridManager gridManager)
    {
        var sameConfig = false;
        if (_cachedPathConfiguration != null)
        {
            var cachedConfig = _cachedPathConfiguration.Value;
            sameConfig = cachedConfig.Equals(configuration);
        }

        if (PathStartPosition == startPosition && endPosition == PathEndPosition && sameConfig)
        {
            return;
        }

        ClearPathVisuals(purgeCache: false);

        if (startPosition == null || endPosition == null)
        {
            return;
        }

        _cachedPathConfiguration = configuration;

        PathStartPosition = startPosition;
        PathEndPosition = endPosition;

        if (PathStartPosition != null && PathEndPosition != null)
        {
            var path = gridManager.CalculatePath((Vector3Int)PathStartPosition.Value, (Vector3Int)PathEndPosition.Value);

            int i = 0;
            foreach (var tile in path)
            {
                CreatePathTile(gridManager, tile.x, tile.y, i, configuration);
                i += 1;
            }
        }
    }
}
