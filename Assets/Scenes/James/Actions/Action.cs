using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action
{
    public enum ActionKind
    {
        Movement,
        Attack,
        Other
    }

    public ActionDefinition Definition;

    public Entity Entity { get; private set; }

    public Action(ActionDefinition definition, Entity entity)
    {
        this.Definition = definition;
        this.Entity = entity;
    }

    public ActionKind Kind
    {
        get
        {
            return Definition.Kind;
        }
    }

    public bool DeferCostPayment
    {
        get
        {
            return Definition.DeferCostPayment;
        }
    }

    public bool CostIsPerTile
    {
        get
        {
            return Definition.CostIsPerTile;
        }
    }

    public bool CanExecuteWhileWaiting
    {
        get 
        { 
            return Definition.CanExecuteWhileWaiting; 
        }
    }

    public bool BlocksFromEndingTurn
    {
        get
        {
            return Definition.BlocksFromEndingTurn;
        }
    }

    public bool Targetable
    {
        get
        {
            return Definition.Targetable;
        }
    }

    public bool CanOnlyExecuteOnOwnersTurn
    {
        get
        {
            return Definition.CanOnlyExecuteOnOwnersTurn;
        }
    }

    public string Name {
        get {
            return Definition.Name;
        }
    }

    public string Description
    {
        get
        {
            return Definition.Description;
        }
    }

    public string FlavorText
    {
        get
        {
            return Definition.FlavorText;
        }
    }

    public int ActionPointCost 
    {
        get
        {
            return Definition.BaseActionPointCost;
        }
    }

    public int MagicCost
    {
        get
        {
            return Definition.BaseMagicCost;
        }
    }

    public int MovementCost
    {
        get
        {
            return Definition.BaseMovementCost;
        }
    }

    public int HealthCost
    {
        get
        {
            return Definition.BaseHealthCost;
        }
    }

    public int Range
    {
        get
        {
            return Definition.BaseRange;
        }
    }

    public bool RangeIsDrivenByMovementCost
    {
        get
        {
            return Definition.RangeIsDrivenByMovementCost;
        }
    }

    public List<ActionBehavior> BehaviorRecipes
    {
        get
        {
            return Definition.Behaviors;
        }
    }

    public struct ExecutionContext
    {
        public Action action;
        public Entity source;
        public int range;
        public GridManager gridManager;
        public TileData? target;
        public bool ignoringCost;

        public override string ToString() => $"<ExecutionContext: source={source}, target={target}>";
    }
}
