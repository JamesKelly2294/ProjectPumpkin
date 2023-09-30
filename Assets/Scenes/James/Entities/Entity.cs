using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static UnityEditor.Timeline.TimelinePlaybackControls;

[RequireComponent(typeof(Selectable))]
public class Entity : MonoBehaviour, ISelectable
{
    public enum OwnerKind
    {
        Neutral,
        Player,
        Enemy,
        Ally,
        EnemyAlly
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

    public int Health;
    public int Mana;
    public int ActionPoints;
    public int Movement;

    public bool IsWaiting; // a unit can "wait", ending its turn

    public bool CanAffordAnyAction
    {
        get
        {
            var canAffordASpecificAction = Actions.Any(a => CanAffordAction(a));
            var canMove = Movement > 0;

            var canTakeAnyAction = canMove || canAffordASpecificAction;

            return !IsWaiting && canTakeAnyAction; 
        }
    }

    public ActionCostAnalysis CreateActionCostAnalysis(Action a)
    {
        var actionAttempt = new ActionCostAnalysis
        {
            CanAffordHealthCost = a.HealthCost >= Health,
            CanAffordManaCost = a.MagicCost >= Mana,
            CanAffordActionPointCost = a.ActionPointCost >= ActionPoints,
            CanAffordMovementCost = a.MovementCost >= Movement,
        };

        return actionAttempt;
    }

    public bool CanAffordAction(Action a)
    {
        return CreateActionCostAnalysis(a).CanBeExecuted;
    }

    public void ExecuteAction(Action a, Vector2Int? target = null, bool ignoringCost = false)
    {
        Debug.Log($"Executing {a}...");

        if (!ignoringCost && !CanAffordAction(a)) {
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

        var context = new Action.ExecutionContext();
        if (target != null)
        {
            var data = GridManager.GetTileData(target.Value);
        }
        context.source = this;
        context.target = null;

        var canExecuteAllBehaviors = behaviors.All(b => b.CanExecute(context));

        if (!ignoringCost)
        {
            PayCostForAction(a);
        }

        foreach (var behavior in behaviors)
        {
            behavior.Execute(context);
        }
    }

    public void InitiateActionAttempt(Action a) {
        // TODO
        ExecuteAction(a);
    }

    private void PayCostForAction(Action a)
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

    // Start is called before the first frame update
    void Start()
    {
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

    public void Move(List<Vector2Int> path)
    {
        if (moving) { return; }
        if (path == null) { return; }
        if (path.Count == 0) { return; }

        moving = true;
        StartCoroutine(MoveAlongPath(path));
    }

    bool moving = false;

    IEnumerator MoveAlongPath(List<Vector2Int> path)
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
            transform.position = (Vector3Int)Position + (direction * progress) + new Vector3(0.5f, 0.5f, 0.0f);

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
    }
}
