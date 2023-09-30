using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ActionDefinition", menuName = "Entity/Action", order = 1)]
public class ActionDefinition : ScriptableObject
{
    public string Name = "Action";
    public string Description = "Description of the action.";
    public string FlavorText = "\"Flavor text for the action.\"";

    public Sprite Icon = null;
    public Color Color = Color.blue;

    [Range(1, 5)]
    public int BaseActionPointCost = 1;

    [Range(1, 50)]
    public int BaseMagicCost = 2;
}
