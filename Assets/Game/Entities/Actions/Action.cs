using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Action
{
    public enum ActionKind
    {
        Movement,
        Attack,
        Other
    }

    public enum ActionTarget
    {
        None,
        Walkable,
        Entity,
        AllyEntity,
        EnemyEntity,
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

    public ActionTarget Target
    {
        get
        {
            return Definition.Target;
        }
    }

    public bool Targetable
    {
        get
        {
            return Definition.Target != ActionTarget.None;
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

    public bool Validate(Action.ExecutionContext context)
    {
        var entity = context.source;
        var canAfford = entity.CanAffordAction(this);

        var validTarget = false;
        if (this.Targetable && context.target != null)
        {
            var entityForTarget = context.target.Value.Entity;
            switch (this.Target)
            {
                case Action.ActionTarget.Walkable:
                    validTarget = context.target.Value.IsWalkable();
                    break;
                case Action.ActionTarget.Entity:
                    validTarget = entityForTarget != null;
                    break;
                case Action.ActionTarget.AllyEntity:
                    validTarget = entityForTarget != null && entity.Owner.GetAlignmentMapping()[entityForTarget.Owner] == Entity.OwnerAlignment.Good;
                    break;
                case Action.ActionTarget.EnemyEntity:
                    validTarget = entityForTarget != null && entity.Owner.GetAlignmentMapping()[entityForTarget.Owner] == Entity.OwnerAlignment.Bad;
                    break;
            }

            var targetPosition = context.target.Value.Position;
            var path = context.gridManager.CalculatePath(context.source.Position, targetPosition, context.range, ignoringObstacles: true);
            if (path.LastOrDefault() != targetPosition)
            {
                validTarget = false;
            }
        }
        else
        {
            validTarget = true;
        }

        bool validated = canAfford && validTarget;

        if (!validated)
        {
            Debug.Log($"Unable to validate, entity={entity.gameObject}, canAfford={canAfford}, validTarget={validTarget}");
        }

        return validated;
    }
}
