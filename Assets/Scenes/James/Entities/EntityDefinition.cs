using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EntityDefinition", menuName = "Entity/Definition", order = 1)]
public class EntityDefinition : ScriptableObject
{
    public string Name = "Entity";
    public string Description = "Description of the entity.";
    public string FlavorText = "\"Flavor text for the entity.\"";

    [Range(1, 100)]
    public int BaseMaxHealth = 10;
    [Range(1, 20)]
    public int BaseMaxMana = 10;
    [Range(1, 10)]
    public int BaseMaxActionPoints = 2;
    [Range(1, 20)]
    public int BaseMaxMovement = 4;
}
