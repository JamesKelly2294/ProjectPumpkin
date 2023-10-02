using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class MoveBehavior : ActionBehavior
{
    public override bool CanExecute(Action.ExecutionContext context)
    {
        return context.source != null && context.target == null;
    }

    public override void Execute(Action.ExecutionContext context)
    {
        Debug.Log("Executing " + context);

        context.source.SetBusy(true);
        var startPosition = context.source.Position;
        Vector2Int endPosition;
        var tileData = context.target;

        if (tileData != null)
        {
            endPosition = tileData.Value.Position;
        }
        else
        {
            context.source.SetBusy(false);
            Debug.LogError("Invalid configuration for MoveBehavior - missing target");
            return;
        }

        var path = context.gridManager.CalculatePath(startPosition, endPosition, context.range);
        if (path.Count <= 1) {
            context.source.SetBusy(false);
            return;
        }

        if (!context.ignoringCost && context.action.CostIsPerTile)
        {
            for (int i = 0; i < path.Count - 1; i++)
            {
                context.source.PayCostForAction(context.action);
            }
        }

        context.source.Move(path, MoveCompleted);
    }

    private void MoveCompleted(Entity e )
    {
        e.SetBusy(false);
    }
}

