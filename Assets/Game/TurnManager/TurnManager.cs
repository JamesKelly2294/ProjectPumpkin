using info.jacobingalls.jamkit;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;
using static UnityEngine.GraphicsBuffer;

[RequireComponent(typeof(PubSubSender))]
public class TurnManager : MonoBehaviour
{
    public List<Entity.OwnerKind> TurnOrder = new List<Entity.OwnerKind> { Entity.OwnerKind.Player, Entity.OwnerKind.Enemy, Entity.OwnerKind.Neutral, };

    public int CurrentTurn
    {
        get { return _currentTurn; }
        private set
        {
            if (_currentTurn == value) { return; }
            _currentTurn = value;
            _pubSubSender.Publish("turnManager.currentTurn.changed", _currentTurn);
            _pubSubSender.Publish("turnManager.state.changed", _currentTurn);
        }
    }
    [SerializeField]
    private int _currentTurn = 0;

    public Entity.OwnerKind CurrentTeam { 
        get { return _currentTeam; } 
        private set
        {
            if (_currentTeam == value) { return; }
            _currentTeam = value;
            _pubSubSender.Publish("turnManager.currentTeam.changed", _currentTeam);
            _pubSubSender.Publish("turnManager.state.changed", _currentTurn);
        }
    }
    [SerializeField]
    private Entity.OwnerKind _currentTeam = Entity.OwnerKind.Player;

    public bool CurrentTeamCanTakeAction { 
        get { return _currentTeamCanTakeAction; } 
        private set
        {
            if (_currentTeamCanTakeAction == value) { return; }
            _currentTeamCanTakeAction = value;
            _pubSubSender.Publish("turnManager.currentTeamCanTakeAction.changed", _currentTeamCanTakeAction);
            _pubSubSender.Publish("turnManager.state.changed", _currentTurn);
        }
    }
    [SerializeField]
    private bool _currentTeamCanTakeAction = false;

    public List<Entity> CurrentTeamEntitiesThatCanTakeAction
    {
        get { return _currentTeamEntitiesThatCanTakeAction; }
        private set
        {
            if (_currentTeamEntitiesThatCanTakeAction == value) { return; }
            if (_currentTeamEntitiesThatCanTakeAction.Count == value.Count && _currentTeamEntitiesThatCanTakeAction.SequenceEqual(value)) { return; }

            _currentTeamEntitiesThatCanTakeAction = value;
            _pubSubSender.Publish("turnManager.currentTeamEntitiesThatCanTakeAction.changed", _currentTeamEntitiesThatCanTakeAction);
            _pubSubSender.Publish("turnManager.state.changed", _currentTurn);
        }
    }
    [SerializeField]
    private List<Entity> _currentTeamEntitiesThatCanTakeAction = new();

    public bool BlockingEventIsExecuting
    {
        get { return _blockingEventIsExecuting; }
        private set
        {
            if (_blockingEventIsExecuting == value) { return; }
            _blockingEventIsExecuting = value;
            _pubSubSender.Publish("turnManager.blockingEventIsExecuting.changed", BlockingEventIsExecuting);
            _pubSubSender.Publish("turnManager.state.changed", _currentTurn);
        }
    }
    [SerializeField]
    private bool _blockingEventIsExecuting = false;

    private PubSubSender _pubSubSender;
    private GridManager _gridManager;

    public List<Entity> OwnedEntities(Entity.OwnerKind owner)
    {
        return _gridManager.Entities
               .Where(e => e.Owner == owner)
               .ToList();
    }
    public bool EntityCanDoMoreThisTurn(Entity entity)
    {
        return entity.CanAffordAnyAction;
    }

    public List<Entity> EntitiesThatCanTakeAction(Entity.OwnerKind team)
    {
        var currentTeamEntities = OwnedEntities(team);

        return currentTeamEntities.Where(e => EntityCanDoMoreThisTurn(e)).ToList();
    }

    public void EntityBusynessDidChange()
    {
        var anyEntityIsBusy = _gridManager.Entities.Any(e => e.IsBusy);

        BlockingEventIsExecuting = anyEntityIsBusy;
    }

    public void SubmitAction(Action a, Action.ExecutionContext context)
    {
        if (a == null) { return; }

        if (BlockingEventIsExecuting)
        {
            return;
        }

        if (context.source.Owner != CurrentTeam && a.CanOnlyExecuteOnOwnersTurn)
        {
            Debug.Log($"Attempting to submit {a} for {context.source}, but it is not their turn.");
            return;
        }

        if (!context.ignoringCost && !context.source.CanAffordAction(a))
        {
            return;
        }

        ExecuteAction(a, context);
    }

    private void ExecuteAction(Action a, Action.ExecutionContext context)
    {
        Debug.Log($"Executing {a}...");

        var entity = context.source;

        if (!context.ignoringCost && !entity.CanAffordAction(a))
        {
            Debug.LogError($"Unable to execute {a} for {this} - cannot afford it.");
        }

        var recipes = a.BehaviorRecipes.GroupBy(r => r.gameObject).Select(y => y.First()).ToList();
        List<ActionBehavior> behaviors = new();

        foreach (var recipe in recipes)
        {
            var go = Instantiate(recipe);
            go.transform.parent = transform;
            go.transform.name = $"{a.Name} Behavior";
            foreach (var b in go.GetComponents<ActionBehavior>())
            {
                behaviors.Add(b);
            }
        }

        var canExecuteAllBehaviors = behaviors.All(b => b.CanExecute(context));

        if (!context.ignoringCost && !context.action.DeferCostPayment)
        {
            entity.PayCostForAction(a);
        }

        foreach (var behavior in behaviors)
        {
            behavior.Execute(context);
        }
    }

    public void EndTeamTurn()
    {
        var currentTeamIndex = TurnOrder.IndexOf(CurrentTeam);

        if (currentTeamIndex == -1) {
            Debug.LogError("Error - the current team is not part of the TurnOrder array. We don't know whose turn is next!");
        }

        if (currentTeamIndex == TurnOrder.Count - 1)
        {
            _currentTeam = TurnOrder[0];
            _currentTurn += 1;

            _pubSubSender.Publish("turnManager.currentTurn.changed", _currentTurn);
            _pubSubSender.Publish("turnManager.currentTeam.changed", _currentTurn);
            _pubSubSender.Publish("turnManager.state.changed", _currentTurn);
        }
        else
        {
            CurrentTeam = TurnOrder[currentTeamIndex + 1];
        }

        var currentTeamEntities = OwnedEntities(CurrentTeam);
        foreach ( var entity in currentTeamEntities )
        {
            entity.NewTurnBegan();
        }
    }

    // Start is called before the first frame update
    void Awake()
    {
        _pubSubSender = GetComponent<PubSubSender>();
        _gridManager = FindObjectOfType<GridManager>();
    }

    // Update is called once per frame
    void Update()
    {
        var currentTeamEntities = OwnedEntities(CurrentTeam);
        CurrentTeamEntitiesThatCanTakeAction = currentTeamEntities.Where(e => EntityCanDoMoreThisTurn(e)).ToList();
        CurrentTeamCanTakeAction = CurrentTeamEntitiesThatCanTakeAction.Count > 0;
    }
}
