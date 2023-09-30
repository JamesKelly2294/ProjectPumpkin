using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicAttackBehavior : ActionBehavior
{
    public override bool CanExecute(Action.ExecutionContext context)
    {
        return context.source != null && context.target != null && context.target.Value.Entity != null;
    }

    public override void Execute(Action.ExecutionContext context)
    {
        Debug.Log("Executing " + context);
    }
}
