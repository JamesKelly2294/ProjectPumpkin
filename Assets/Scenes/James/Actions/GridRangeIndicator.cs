using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridRangeIndicator : MonoBehaviour
{
    public struct Configuration
    {
        public Vector2Int origin;
        public int range;
    }

    public GameObject TileOutlinePrefab;


    [Header("Pathing")]
    public GameObject PathStartPrefab;
    public GameObject PathEndPrefab;
    public GameObject PathNodePrefab;

    private GameObject _pathStartGO;
    private GameObject _pathEndGO;
    private GameObject _pathGroupGO;


    private GameObject _tileOutlineContainerGO;

    public void Start()
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
    }

    void ClearPathVisuals()
    {
        //foreach (var node in path)
        //{
        //    var pathVisual = Instantiate(PathNodePrefab);
        //    pathVisual.transform.position = TileCoordinateToWorldPosition((Vector2Int)node);
        //    pathVisual.transform.name = "(" + node.x + ", " + node.y + ")";
        //    pathVisual.transform.parent = _pathGroupGO.transform;
        //}

        for (int i = 0; i < _pathGroupGO.transform.childCount; i++)
        {
            Destroy(_pathGroupGO.transform.GetChild(i).gameObject);
        }
    }

    public GameObject CreateTileOutline(GridManager gridManager, int x, int y)
    {
        var tileOutline = Instantiate(TileOutlinePrefab);
        tileOutline.transform.position = gridManager.TileCoordinateToWorldPosition(new Vector2Int(x, y));
        tileOutline.transform.name = "(" + x + ", " + y + ")";
        tileOutline.transform.parent = _tileOutlineContainerGO.transform;

        return tileOutline;
    }

    public void Visualize(GridManager gridManager, Configuration configuration)
    {
        if (_tileOutlineContainerGO == null)
        {
            _tileOutlineContainerGO = new GameObject();
            _tileOutlineContainerGO.transform.parent = transform;
            _tileOutlineContainerGO.transform.name = "Tile Outline Container";
        }

        var minX = configuration.origin.x - configuration.range;
        var maxX = configuration.origin.x + configuration.range;
        var minY = configuration.origin.y - configuration.range;
        var maxY = configuration.origin.y + configuration.range;

        var width = maxX - minX + 1;
        var height = maxY - minY + 1;

        //var tiles = gridManager.Walkable.GetTilesBlock(new BoundsInt(minX, minY, 0, width, height, 1));

        //for (int i = 0; i < tiles.Length; i++)
        //{
        //    var x = (i % width) + minX;
        //    var y = (i / width) + minY;
        //    var tile = tiles[i];
        //    if (tile != null)
        //    {
        //        CreateTileOutline(gridManager, x, y);
        //    }
        //}

        var tiles = gridManager.BFS((Vector3Int)configuration.origin, configuration.range);

        foreach (var tile in tiles)
        { 
            CreateTileOutline(gridManager, tile.x, tile.y);
        }
    }
}
