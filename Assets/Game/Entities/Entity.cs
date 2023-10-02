using info.jacobingalls.jamkit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Entity;

public static class OwnerKindExtensions
{
    public static Dictionary<Entity.OwnerKind, Entity.OwnerAlignment> GetAlignmentMapping(this Entity.OwnerKind kind)
    {
        Dictionary<Entity.OwnerKind, Entity.OwnerAlignment> results = new();
        foreach (Entity.OwnerKind ownerKind in Enum.GetValues(typeof(Entity.OwnerKind)))
        {
            if (ownerKind == kind) { results[ownerKind] = OwnerAlignment.Good; }
            else { results[ownerKind] = OwnerAlignment.Bad; }
        }

        return results;
    }
}

[RequireComponent(typeof(Selectable))]
[RequireComponent(typeof(PubSubSender))]
public class Entity : MonoBehaviour, ISelectable
{
    public static int TheoreticalHealthMax = 30;
    public static int TheoreticalManaMax = 20;
    public static int TheoreticalActionPointMax = 5;
    public static int TheoreticalMovementMax = 8;

    public enum OwnerKind
    {
        Neutral,
        Player,
        Enemy,
        Ally,
        EnemyAlly
    }
    public enum OwnerAlignment
    {
        Good,
        Bad,
        Neutral
    }

    public struct ActionCostAnalysis
    {
        public bool CanAffordHealthCost;
        public bool CanAffordManaCost;
        public bool CanAffordActionPointCost;
        public bool CanAffordMovementCost;

        public bool CanBeExecuted {
            get {
                return CanAffordHealthCost &&
                CanAffordManaCost &&
                CanAffordActionPointCost &&
                CanAffordMovementCost;
            }
        }
    }

    public OwnerKind Owner = Entity.OwnerKind.Enemy; // cannot be changed after registration

    public EntityDefinition Definition;

    public GridManager GridManager;


    public string Name
    {
        get
        {
            return Definition.Name;
        }
    }

    public string ClassName
    {
        get
        {
            return Definition.ClassName;
        }
    }

    public int Health
    {
        get { return _health; }
        private set
        {
            if (_health == value) { return; }
            _health = value;
            _pubSubSender.Publish("entity.health.changed", _health);
            _pubSubSender.Publish("entity.resources.changed", _health);
        }
    }
    [SerializeField]
    private int _health;

    public int Mana
    {
        get { return _mana; }
        private set
        {
            if (_mana == value) { return; }
            _mana = value;
            _pubSubSender.Publish("entity.mana.changed", _mana);
            _pubSubSender.Publish("entity.resources.changed", _mana);
        }
    }
    [SerializeField]
    private int _mana;

    public int ActionPoints
    {
        get { return _actionPoints; }
        private set
        {
            if (_actionPoints == value) { return; }
            _actionPoints = value;
            _pubSubSender.Publish("entity.action_points.changed", _actionPoints);
            _pubSubSender.Publish("entity.resources.changed", _actionPoints);
        }
    }
    [SerializeField]
    private int _actionPoints;

    public int Movement
    {
        get { return _movement; }
        private set
        {
            if (_movement == value) { return; }
            _movement = value;
            _pubSubSender.Publish("entity.movement.changed", _movement);
            _pubSubSender.Publish("entity.resources.changed", _movement);
        }
    }
    [SerializeField]
    private int _movement;

    public bool IsWaiting // a unit can "wait", ending its turn
    {
        get { return _isWaiting; }
        private set
        {
            if (_isWaiting == value) { return; }
            _isWaiting = value;
            _pubSubSender.Publish("entity.is_waiting.changed", _isWaiting);
        }
    }
    [SerializeField]
    private bool _isWaiting = false;

    public void SetWaiting(bool waiting)
    {
        IsWaiting = waiting;
    }

    public bool IsBusy // if a unit is "busy", they are actively executing an action, and UI should be disabled
    {
        get { return _isBusy; }
        private set
        {
            if (_isBusy == value) { return; }
            _isBusy = value;
            _pubSubSender.Publish("entity.is_busy.changed", _isBusy);
        }
    }
    [SerializeField]
    private bool _isBusy = false;

    public void SetBusy(bool busy)
    {
        IsBusy = busy;
    }

    public ActionCostAnalysis CreateActionCostAnalysis(Action a)
    {
        var actionAttempt = new ActionCostAnalysis
        {
            CanAffordHealthCost = Health >= a.HealthCost,
            CanAffordManaCost = Mana >= a.MagicCost,
            CanAffordActionPointCost = ActionPoints >= a.ActionPointCost,
            CanAffordMovementCost = Movement >= a.MovementCost,
        };

        return actionAttempt;
    }

    public bool CanAffordAction(Action a)
    {
        return CreateActionCostAnalysis(a).CanBeExecuted;
    }

    public bool CanAffordAnyAction
    {
        get
        {
            var canAffordASpecificAction = Actions.Any(a => CanAffordAction(a) && a.BlocksFromEndingTurn);
            var canMove = Movement > 0;

            var canTakeAnyAction = canMove || canAffordASpecificAction;

            return !IsWaiting && canTakeAnyAction; 
        }
    }

    public void NewTurnBegan()
    {
        Movement = MaxMovement;
        IsWaiting = false;
    }

    public void ToggleWait()
    {
        IsWaiting = !IsWaiting;
    }

    IEnumerator test()
    {
        IsBusy = true;
        Debug.Log($"{this} is busy.");
        var t = 0.0f;

        while (t < 2.0f)
        {
            t += Time.deltaTime;
            yield return null;
        }
        IsBusy = false;
        Debug.Log($"{this} is no longer busy.");
    }

    public void PayCostForAction(Action a)
    {
        Health -= a.HealthCost;
        Mana -= a.MagicCost;
        ActionPoints -= a.ActionPointCost;
        Movement -= a.MovementCost;
    }

    public Vector2Int Position { get { return GridManager.PositionForEntity(this); } }

    public int MaxHealth { get { return Definition.BaseMaxHealth; } }
    public int MaxMana { get { return Definition.BaseMaxMana; } }
    public int MaxMovement { get { return Definition.BaseMaxMovement; } }
    public int MaxActionPoints { get { return Definition.BaseMaxActionPoints; } }

    public List<Action> Actions { get; private set; } = new List<Action>();

    private PubSubSender _pubSubSender;

    // Start is called before the first frame update
    void Start()
    {
        _pubSubSender = GetComponent<PubSubSender>();
        if (GridManager == null)
        {
            // PANIK, try to find one
            GridManager = GameObject.FindObjectOfType<GridManager>();
        }

        Health = MaxHealth;
        Mana = MaxMana;
        ActionPoints = MaxActionPoints;
        Movement = MaxMovement;

        foreach (var actionDefinition in Definition.BaseActions)
        {
            Actions.Add(new Action(actionDefinition, this));
        }

        GridManager.RegisterEntity(this, new Vector2Int(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y)));
    }

    private void OnDestroy()
    {
        GridManager.UnregisterEntity(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnSelected()
    {
    }

    public void OnDeselected()
    {

    }
    public int Range(Action a)
    {
        int range;
        if (a.RangeIsDrivenByMovementCost)
        {
            range = Movement / a.MovementCost;
        }
        else
        {
            range = a.Range;
        }
        return range;
    }

    public delegate void MoveCompleted(Entity e);

    public void Move(List<Vector2Int> path, MoveCompleted completionHandler)
    {
        if (moving) { return; }
        if (path == null) { return; }
        if (path.Count == 0) { return; }

        moving = true;
        StartCoroutine(MoveAlongPath(path, completionHandler));
    }

    bool moving = false;

    IEnumerator MoveAlongPath(List<Vector2Int> path, MoveCompleted completionHandler)
    {
        Vector2Int destinationNode = path.Last();
        Vector2Int currentNode = path.First();
        path.RemoveAt(0);


        var t = 0.0f;
        var speed = 7.5f; // meters per second
        var timeToWalkAcrossTile = 1 / speed;

        while (Position != destinationNode)
        {
            t += Time.deltaTime;
            float progress = t / timeToWalkAcrossTile;

            var direction = new Vector3(currentNode.x - Position.x, currentNode.y - Position.y, 0.0f).normalized;
            transform.position = (Vector3Int)Position + (direction * progress) + new Vector3(0.5f, 0.5f, 0.0f); // ew, becky. ew.

            if (t > timeToWalkAcrossTile) {
                GridManager.SetEntityPosition(this, currentNode);

                if (path.Count > 0)
                {
                    t = 0.0f;
                    currentNode = path.First();
                    path.RemoveAt(0);
                }
            }

            yield return null;
        }
        GridManager.SetEntityPosition(this, destinationNode);

        moving = false;
        completionHandler(this);
    }





    public delegate void MeleeAttackZenithReached(Entity e);
    public delegate void MeleeAttackCompleted(Entity e);

    public void PlayMeleeAttackAnimation(Vector2Int target, float speedMPS, MeleeAttackZenithReached zenithHandler, MeleeAttackCompleted completionHandler)
    {
        StartCoroutine(MeleeAttackAnimationCoroutine(target, speedMPS, zenithHandler, completionHandler));
    }

    IEnumerator MeleeAttackAnimationCoroutine(Vector2Int target, float speedMPS, MeleeAttackZenithReached zenithHandler, MeleeAttackCompleted completionHandler)
    {
        var t = 0.0f;
        var speed = speedMPS; // meters per second
        var timeToWalkAcrossTile = 1 / speed;
        var targetDirection = (new Vector3(target.x - Position.x, target.y - Position.y, 0.0f).normalized).normalized;
        float animationTime = targetDirection.magnitude * timeToWalkAcrossTile;

        var curve = AnimationCurve.EaseInOut(0.0f, 0.0f, animationTime, 1.0f);
        curve.postWrapMode = WrapMode.PingPong;

        bool calledZenithHandler = false;
        while (t < animationTime * 2)
        {
            t += Time.deltaTime;
            var direction = new Vector3(target.x - Position.x, target.y - Position.y, 0.0f).normalized;
            var progress = curve.Evaluate(t);
            transform.position = (Vector3Int)Position + (direction * progress) + new Vector3(0.5f, 0.5f, 0.0f); // ew, becky. ew.

            if (t > animationTime && !calledZenithHandler)
            {
                zenithHandler(this);
                calledZenithHandler = true;
            }

            yield return null;
        }

        completionHandler(this);
    }

    public void ApplyDamage(int damageAmount)
    {
        if (damageAmount <= 0) { Debug.LogError("Cannot apply non-positive damage."); return; }

        Health = Mathf.Max(Health - damageAmount, 0);

        if (Health <= 0)
        {
            Debug.Log("Deadge!");
            Destroy(gameObject);
        }
    }
}
