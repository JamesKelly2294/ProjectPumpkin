using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action
{
    public ActionDefinition Definition;

    private Entity _entity;

    public Action(ActionDefinition definition, Entity entity)
    {
        this.Definition = definition;
        this._entity = entity;
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
            return Definition.BaseMagicCost;
        }
    }

    public int HealthCost
    {
        get
        {
            return Definition.BaseHealthCost;
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
        public Entity source;
        public TileData? target;

        public override string ToString() => $"<ExecutionContext: source={source}, target={target}>";
    }
}
