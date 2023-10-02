using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class ActionBehavior : MonoBehaviour
{
    public virtual bool CanExecute(Action.ExecutionContext context)
    {
        throw new NotImplementedException("ActionBehavior.CanExecute must be implemented in a subclass!");
    }

    public virtual void Execute(Action.ExecutionContext context)
    {
        throw new NotImplementedException("ActionBehavior.Execute must be implemented in a subclass!");
    }
}
