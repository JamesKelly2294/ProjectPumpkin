using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
