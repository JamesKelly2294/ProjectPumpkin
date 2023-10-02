using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDefinition", menuName = "Item/Definition", order = 1)]
public class ItemDefinition : ScriptableObject
{
    public string Name = "Item";
    public string Description = "Description of the item.";
    public string FlavorText = "\"Flavor text for the item.\"";
    public Sprite Icon;
    public Color Color = Color.white;

    [Range(0, 100)]
    public int Value = 0;
}
