using info.jacobingalls.jamkit;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(PubSubSender))]
public class TurnManager : MonoBehaviour
{
    public int CurrentTurn
    {
        get { return _currentTurn; }
        private set
        {
            if (_currentTurn == value) { return; }
            _currentTurn = value;
            _pubSubSender.Publish("turnManager.currentTurn.changed", _currentTurn);
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
            _currentTeamEntitiesThatCanTakeAction = value;
            _pubSubSender.Publish("turnManager.currentTeamEntitiesThatCanTakeAction.changed", _currentTeamEntitiesThatCanTakeAction);
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

    public void SubmitAction(Action a)
    {
        if (a == null) { return; }

        var entity = a.Entity;
        if (entity == null) { return; };


        if (entity.Owner != CurrentTeam && a.CanOnlyExecuteOnOwnersTurn)
        {
            Debug.Log($"Attempting to submit {a} for {entity}, but it is not their turn.");
            return;
        }

        entity.InitiateActionAttempt(a);
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
