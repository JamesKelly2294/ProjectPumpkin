using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Selectable))]
public class Entity : MonoBehaviour
{
    public EntityDefinition Definition;

    public GridManager GridManager;

    public int Health;
    public int Mana;
    public int ActionPoints;
    public int Movement;

    public Vector2Int Position { get; private set; }

    public int MaxHealth { get { return Definition.BaseMaxHealth; } }
    public int MaxMana { get { return Definition.BaseMaxMana; } }
    public int MaxMovement { get { return Definition.BaseMaxMovement; } }
    public int MaxActionPoints { get { return Definition.BaseMaxActionPoints; } }

    public List<Action> Actions { get; private set; } = new List<Action>();


    // Start is called before the first frame update
    void Start()
    {
        if (GridManager == null)
        {
            // PANIK, try to find one
            GridManager = GameObject.FindObjectOfType<GridManager>();
        }

        Health = MaxHealth;
        Mana = MaxMana;
        ActionPoints = MaxActionPoints;
        Movement = MaxMovement;

        foreach (var actionDefinition in Definition.BaseActions)
        {
            Actions.Add(new Action(actionDefinition, this));
        }

        GridManager.RegisterEntity(this, new Vector2Int(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y)));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
