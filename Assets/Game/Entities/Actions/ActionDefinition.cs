using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "ActionDefinition", menuName = "Entity/Action", order = 1)]
public class ActionDefinition : ScriptableObject
{

    public string Name = "Action";
    public string Description = "Description of the action.";
    public string FlavorText = "\"Flavor text for the action.\"";

    public Sprite Icon = null;
    public Color Color = Color.blue;

    public bool BlocksFromEndingTurn = true; // if the entity can take this action, don't let turn end
    public bool CanOnlyExecuteOnOwnersTurn = true;
    public bool CostIsPerTile = false;
    public bool CanExecuteWhileWaiting = false;
    public bool RangeIsDrivenByMovementCost = false;
    public bool DeferCostPayment = false;
    public Action.ActionKind Kind = Action.ActionKind.Other;
    public Action.ActionTarget Target = Action.ActionTarget.None;
    public List<ActionBehavior> Behaviors;

    [Range(0, 5)]
    public int BaseActionPointCost = 1;

    [Range(0, 50)]
    public int BaseMagicCost = 0;

    [Range(0, 10)]
    public int BaseMovementCost = 0;

    [Range(0, 10)]
    public int BaseHealthCost = 0;

    [Range(0, 10)]
    public int BaseRange = 1;
}
