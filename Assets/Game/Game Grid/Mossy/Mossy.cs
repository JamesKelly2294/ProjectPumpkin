using info.jacobingalls.jamkit;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(PubSubSender))]
public class Mossy : MonoBehaviour
{
    private GridManager _gridManager;
    private TurnManager _turnManager;

    [SerializeField]
    private Vector3Int _gridPosition;

    [SerializeField]
    private int _globalCrystalCount;

    [SerializeField]
    private int _collectedCrystals;

    // Start is called before the first frame update
    void Start()
    {
        _gridManager = FindObjectOfType<GridManager>();
        _turnManager = FindObjectOfType<TurnManager>();

        _gridPosition = (Vector3Int)_gridManager.ToWorldPositionTileCoordinate(transform.position);

        _globalCrystalCount = _gridManager.Items.Where(i => i.Definition.Name.Contains("Crystal")).Count();
        Debug.Log($"M.O.S.E found {_globalCrystalCount} crystals");

        GetComponent<PubSubSender>().Publish("mossy.crystals.global_total_changed", _globalCrystalCount);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void PlaySuckAudio()
    {
        AudioManager.Instance.Play("SFX/MossySuck",
            pitchMin: 0.6f, pitchMax: 0.7f,
            volumeMin: 0.7f, volumeMax: 0.7f,
            position: transform.position,
            minDistance: 10, maxDistance: 20);
    }

    public void EvaluateWinState()
    {
        if (_collectedCrystals >= _globalCrystalCount)
        {
            Debug.Log("You're winner!");
            GetComponent<PubSubSender>().Publish("gameManager.showWin");
        }
    }

    public void ProcessNeighbors()
    {
        var neighbors = _gridManager.NeighborsForTileAtPosition(_gridPosition, ignoringObstacles: true);
        foreach (var neighbor in neighbors)
        {
            var tileData = _gridManager.GetTileData((Vector2Int)neighbor);
            if (tileData.Entity != null)
            {
                var inventory = tileData.Entity.GetComponent<Inventory>();

                var suckedThisTurn = false;
                if (inventory.Items.Count > 0)
                {
                    _collectedCrystals += inventory.Items.Where(i => i.Name.Contains("Crystal")).Count();

                    Debug.Log($"M.O.S.E sucked up {inventory.Items.Count} items, including {_collectedCrystals} crystals");

                    GetComponent<PubSubSender>().Publish("mossy.cystals.delivered", _collectedCrystals);
                    GetComponent<PubSubSender>().Publish("points.gained", _collectedCrystals * 10);

                    inventory.RemoveAllItems();
                    suckedThisTurn = true;
                    EvaluateWinState();
                }

                if (suckedThisTurn)
                {
                    PlaySuckAudio();
                }
            }
        }
    }
}