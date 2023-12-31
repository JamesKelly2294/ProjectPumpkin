using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EntityDefinition", menuName = "Entity/Definition", order = 1)]
public class EntityDefinition : ScriptableObject
{
    public string Name = "Entity";
    public string ClassName = "";
    public string Description = "Description of the entity.";
    public string FlavorText = "\"Flavor text for the entity.\"";
    public Sprite Icon;
    public Color Color = Color.blue;

    [Range(1, 100)]
    public int BaseMaxHealth = 10;
    [Range(0, 20)]
    public int BaseMaxMana = 10;
    [Range(1, 10)]
    public int BaseMaxActionPoints = 2;
    [Range(0, 20)]
    public int BaseMaxMovement = 4;

    [Range(0.1f, 10.0f)]
    public float MoveSpeed = 5.0f;


    [Header("Loot")]
    public GameObject DroppedItemPrefab;
    public List<ItemDefinition> PotentialItemDrops;

    [Range(0, 10)]
    public int MinDrops;
    [Range(0, 10)]
    public int MaxDrops;

    public List<ActionDefinition> BaseActions = new List<ActionDefinition>();
}
