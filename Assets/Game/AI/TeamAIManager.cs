using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class TeamAIManager : MonoBehaviour
{
    public Entity.OwnerKind ControlledTeam;
    [Range(0, 50)]
    public int AggroRange = 10;
    public bool TakingTurn { get; private set; }

    private TurnManager _turnManager;
    private Entity _selectedEntity;
    private GridManager _gridManager;
    private Entity _aggroTarget;

    // Start is called before the first frame update
    void Start()
    {
        _turnManager = FindObjectOfType<TurnManager>();
    }

    void BeginTurn()
    {
        Debug.Log($"Begin turn for {ControlledTeam}.");

        foreach (var entity in _turnManager.CurrentTeamEntitiesThatCanTakeAction)
        {
            Debug.Log($"{entity} can act!");
        }
    }

    public void CurrentTeamDidChange()
    {
        var wasTakingTurn = TakingTurn;
        TakingTurn = _turnManager.CurrentTeam == ControlledTeam;
        if (TakingTurn && wasTakingTurn != TakingTurn)
        {
            BeginTurn();
        }
    }

    void Update()
    {
        if (!TakingTurn || _turnManager.BlockingEventIsExecuting) { return; }

        var foundEntity = SelectEntity();
        if (!foundEntity) { 
            _turnManager.EndTeamTurn();
            Debug.Log($"End turn for {ControlledTeam}.");
            return; 
        }

        AnalyzeSituation();
        TakeAction();
    }

    bool SelectEntity()
    {
        if (_selectedEntity != null)
        {
            if (_selectedEntity.CanAffordAnyAction) 
            { 
                return true; 
            }
            else 
            { 
                _selectedEntity = null; 
            }
        }

        var availableEntities = _turnManager.CurrentTeamEntitiesThatCanTakeAction;
        if (availableEntities.Count == 0) { return false; }
        var randomIndex = Random.Range(0, availableEntities.Count);
        var nextEntity = availableEntities[randomIndex];
        _turnManager.CurrentTeamEntitiesThatCanTakeAction.RemoveAt(randomIndex);

        _selectedEntity = nextEntity;
        Debug.Log($"Selected next entity {_selectedEntity}");

        return true;
    }

    void AnalyzeSituation()
    {
        if (_selectedEntity == null) { return; }
        _gridManager = _selectedEntity.GridManager;

        var exploration = _gridManager.OrderedBFS((Vector3Int)_selectedEntity.Position, AggroRange, ignoringObstacles: true);
        var alignmentMapping = _selectedEntity.Owner.GetAlignmentMapping();
        _aggroTarget = null;
        foreach (var tile in exploration)
        {
            var data = _gridManager.GetTileData(tile);
            var tileEntity = data.Entity;
            if (tileEntity == null) { continue; }
            var prospectiveTargetAligment = alignmentMapping[tileEntity.Owner];
            if (prospectiveTargetAligment == Entity.OwnerAlignment.Bad)
            {
                _aggroTarget = tileEntity;
                break;
            }
        }

        Debug.Log("Found target " + _aggroTarget);
    }

    Action GetWaitActionForSelectedEntity()
    {
        Action waitAction = null;
        bool foundWaitAction = false;
        foreach (var action in _selectedEntity.Actions)
        {
            foreach (var behavior in action.BehaviorRecipes)
            {
                if (behavior is WaitBehavior)
                {
                    foundWaitAction = true;
                    break;
                }
            }
            if (foundWaitAction) { waitAction = action; break; }
        }

        return waitAction;
    }

    Action GetMoveActionForSelectedEntity()
    {
        Action moveAction = null;
        bool foundMoveAction = false;
        foreach (var action in _selectedEntity.Actions)
        {
            foreach (var behavior in action.BehaviorRecipes)
            {
                if (behavior is MoveBehavior)
                {
                    foundMoveAction = true;
                    break;
                }
            }
            if (foundMoveAction) { moveAction = action; break; }
        }

        return moveAction;
    }

    Action.ExecutionContext GetMoveActionExecutionContextForSelectedEntity(Action a)
    {
        var context = new Action.ExecutionContext();

        // Step One: Try to find a direct path to the target
        var path = _gridManager.CalculatePath(_selectedEntity.Position, _aggroTarget.Position, range: AggroRange, alwaysIncludeTarget: true);

        List<Vector2Int> walkPath = null;
        int range = _selectedEntity.Range(a);
        if (path.Count > 1)
        {
            range = Mathf.Min(path.Count - 1, range + 1);
            walkPath = path.GetRange(0, range);
        }

        TileData? data = null;
        if (walkPath != null && walkPath.Count >= 1) { data = _gridManager.GetTileData(walkPath.Last()); }
        context.action = a;
        context.source = _selectedEntity;
        context.range = range;
        context.gridManager = _gridManager;
        context.target = data;

        return context;
    }

    Action.ExecutionContext GetGenericActionExecutionContext(Action a)
    {
        var context = new Action.ExecutionContext();
        TileData? data = null;
        if (_aggroTarget != null)
        {
            data = _gridManager.GetTileData(_aggroTarget.Position);
        }
        context.action = a;
        context.source = _selectedEntity;
        context.range = _selectedEntity.Range(a);
        context.gridManager = _gridManager;
        context.target = data;

        return context;
    }

    void TakeAction()
    {
        if (_selectedEntity == null) { return; }

        Action actionDecision = null;
        Action moveAction = GetMoveActionForSelectedEntity();
        Action waitAction = GetWaitActionForSelectedEntity();

        // If we don't have a target, wait
        // TODO: Fix for abilities that don't require targets
        if (_aggroTarget == null)  {  actionDecision = waitAction; }
        else if (_selectedEntity.CanAffordAction(moveAction)) { actionDecision = moveAction; }

        if (actionDecision == null)
        {
            if (waitAction == null) { Debug.LogError("Unable to make a decision for this entity!"); return; }
            else { actionDecision = waitAction; }
        }

        Action.ExecutionContext context;
        if (actionDecision == moveAction) { context = GetMoveActionExecutionContextForSelectedEntity(actionDecision); }
        else { context = GetGenericActionExecutionContext(actionDecision); }

        if (!actionDecision.Validate(context)) {
            Debug.LogError("Decided on an invalid action " + actionDecision.Name);
            return; 
        }

        Debug.Log($"AI Decision - {_selectedEntity} takes action {actionDecision.Name}");
        _turnManager.SubmitAction(actionDecision, context);

        _selectedEntity = null;
    }
}
