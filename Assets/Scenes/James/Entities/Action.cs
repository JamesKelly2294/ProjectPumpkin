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

    public int ActionPointCost {
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
}
