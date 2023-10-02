using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class TeamAIManager : MonoBehaviour
{
    struct AttackDecisionCandidate
    {
        public Action Action;
        public Vector2Int? TargetPosition;
        public Entity TargetEntity;
        public bool CanAfford;
        public bool InRange;
    }

    public Entity.OwnerKind ControlledTeam;
    [Range(0, 50)]
    public int AggroRange = 10;
    public bool TakingTurn { get; private set; }

    private TurnManager _turnManager;
    private Entity _selectedEntity;
    private GridManager _gridManager;
    private Entity _aggroTarget;

    // Start is called before the first frame update
    void Awake()
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

    [Range(0.0f, 1.0f)]
    public float CooldownDuration = 0.1f;
    float _cooldownTimer = 0.5f;

    void Update()
    {
        if (!TakingTurn || _turnManager.BlockingEventIsExecuting) { return; }

        if (_cooldownTimer > 0.0f) { _cooldownTimer -= Time.deltaTime; return; }

        var foundEntity = SelectEntity();
        if (!foundEntity) { 
            _turnManager.EndTeamTurn();
            _cooldownTimer = 0.0f;
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
        var randomIndex = UnityEngine.Random.Range(0, availableEntities.Count);
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
        int range = _selectedEntity.Range(a);
        List<Vector2Int> walkPath = null;

        // Step One: Try to find a direct path to the target
        var path = _gridManager.CalculatePath(_selectedEntity.Position, _aggroTarget.Position, maxRange: AggroRange, alwaysIncludeTarget: true);

        // Step Two: Try to find a path to the target assuming we can walk through entities.
        // If we can, walk as far along that path as we can.
        if (path.Count == 0) { 
            path = _gridManager.CalculatePath(_selectedEntity.Position, _aggroTarget.Position, range: AggroRange, ignoringObstacles:true);
            walkPath = new List<Vector2Int>();
            foreach (var pos in path)
            {
                var tileData = _gridManager.GetTileData(pos);
                if (tileData.Entity == null || tileData.Entity == _selectedEntity)
                {
                    walkPath.Add(pos);
                }
                else
                {
                    break;
                }
            }
            range = Mathf.Min(walkPath.Count, range + 1);
            walkPath = walkPath.GetRange(0, range);
        }
        else
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

    bool SelectedEntityCanReachAttackActionTarget(Action a)
    {
        // We have an attack action, where the target can be required to be within melee range
        // or could be further away for a ranged attack

        // Goal: BFS, ignoring entities, get a list 

        return false;
    }

    Entity FindTargetInRangeForSelectedEntity(int range)
    {
        var exploration = _gridManager.OrderedBFS((Vector3Int)_selectedEntity.Position, range, ignoringObstacles: true);
        var alignmentMapping = _selectedEntity.Owner.GetAlignmentMapping();
        Entity target = null;
        foreach (var tile in exploration)
        {
            var data = _gridManager.GetTileData(tile);
            var tileEntity = data.Entity;
            if (tileEntity == null) { continue; }
            var prospectiveTargetAligment = alignmentMapping[tileEntity.Owner];
            if (prospectiveTargetAligment == Entity.OwnerAlignment.Bad)
            {
                target = data.Entity;
                break;
            }
        }

        return target;
    }

    List<AttackDecisionCandidate> CandidateAttackActionsForSelectedEntity()
    {
        List<AttackDecisionCandidate> results = new List<AttackDecisionCandidate>();

        var attackActions = _selectedEntity.Actions.Where(a => a.Kind == Action.ActionKind.Attack);

        foreach (var attackAction in attackActions)
        {
            var target = FindTargetInRangeForSelectedEntity(attackAction.Range);

            var decisionCandidate = new AttackDecisionCandidate()
            {
                Action = attackAction,
                TargetEntity = target,
                TargetPosition = target == null ? null : target.Position,
                CanAfford = _selectedEntity.CanAffordAction(attackAction),
                InRange = target != null,
            };

            results.Add(decisionCandidate);
        }

        return results;
    }

    void TakeAction()
    {
        if (_selectedEntity == null) { return; }

        Action actionDecision = null;
        Action moveAction = GetMoveActionForSelectedEntity();
        Action waitAction = GetWaitActionForSelectedEntity();

        // If we don't have a target, wait
        // TODO: Fix for abilities that don't require targets
        if (_aggroTarget == null)  
        {  
            actionDecision = waitAction; 
        }
        else {
            var attackCadidatesInRange = CandidateAttackActionsForSelectedEntity().Where(a => a.InRange);
            var attackCandidatesInRangeAndAffordable = attackCadidatesInRange.Where(a => a.CanAfford).ToList();

            if (attackCandidatesInRangeAndAffordable.Count > 0)
            {
                var randomIndex = UnityEngine.Random.Range(0, attackCandidatesInRangeAndAffordable.Count);
                actionDecision = attackCandidatesInRangeAndAffordable[randomIndex].Action;
            }

            // If we can't find a valid attack, move towards target
            // unless we're in range for an attack already, even if we can't afford it
            var entityShouldMoveIntoRange = attackCadidatesInRange.Count() == 0 && actionDecision == null;
            if (entityShouldMoveIntoRange)
            {
                actionDecision = _selectedEntity.CanAffordAction(moveAction) ? moveAction : null;
            }
        }

        if (actionDecision == null)
        {
            if (waitAction == null) { Debug.LogError("Unable to make a decision for this entity!"); return; }
            else { actionDecision = waitAction; }
        }

        Action.ExecutionContext context;
        if (actionDecision == moveAction) 
        { 
            context = GetMoveActionExecutionContextForSelectedEntity(actionDecision);

            // If we have decided to but can't move, fall back to waiting
            if (context.range == 1) { 
                actionDecision = waitAction; 
                context = GetGenericActionExecutionContext(actionDecision);
            }
        }
        else 
        { 
            context = GetGenericActionExecutionContext(actionDecision);
        }

        if (!actionDecision.Validate(context)) {
            Debug.LogError("Decided on an invalid action " + actionDecision.Name);
            return; 
        }

        Debug.Log($"AI Decision - {_selectedEntity} takes action {actionDecision.Name}");
        _turnManager.SubmitAction(actionDecision, context);

        if (actionDecision != waitAction)
        {
            _cooldownTimer = CooldownDuration;
        }
    }
}
