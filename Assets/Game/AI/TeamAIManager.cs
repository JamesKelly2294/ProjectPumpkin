using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TeamAIManager : MonoBehaviour
{
    public Entity.OwnerKind ControlledTeam;

    private TurnManager _turnManager;
    public  bool TakingTurn { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        _turnManager = FindObjectOfType<TurnManager>();
    }

    void BeginTurn()
    {
        Debug.Log("Begin turn...");
        foreach (var entity in _turnManager.OwnedEntities(Entity.OwnerKind.Enemy))
        {
            Debug.Log($"{entity} owned");
        }
        foreach (var entity in _turnManager.CurrentTeamEntitiesThatCanTakeAction)
        {
            Debug.Log($"{entity} can act!");
        }
    }

    public void CurrentTeamDidChange()
    {
        var wasTakingTurn = TakingTurn;
        TakingTurn = _turnManager.CurrentTeam == ControlledTeam;
        if (wasTakingTurn != TakingTurn)
        {
            BeginTurn();
        }
    }

    void Update()
    {

    }
}
