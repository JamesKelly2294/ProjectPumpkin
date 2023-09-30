using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveBehavior : ActionBehavior
{
    public override bool CanExecute(Action.ExecutionContext context)
    {
        return context.source != null && context.target == null;
    }

    public override void Execute(Action.ExecutionContext context)
    {
        Debug.Log("Executing " + context);
    }
}

