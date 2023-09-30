using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

[RequireComponent(typeof(Selectable))]
public class Entity : MonoBehaviour, ISelectable
{
    public enum OwnerKind
    {
        Neutral,
        Player,
        Enemy,
        Ally,
        EnemyAlly
    }

    public OwnerKind Owner = Entity.OwnerKind.Enemy; // cannot be changed after registration

    public EntityDefinition Definition;

    public GridManager GridManager;

    public int Health;
    public int Mana;
    public int ActionPoints;
    public int Movement;

    public Vector2Int Position { get { return GridManager.PositionForEntity(this); } }

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

    private void OnDestroy()
    {
        GridManager.UnregisterEntity(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnSelected()
    {
    }

    public void OnDeselected()
    {

    }

    public void Move(List<Vector2Int> path)
    {
        if (moving) { return; }
        if (path == null) { return; }
        if (path.Count == 0) { return; }

        moving = true;
        StartCoroutine(MoveAlongPath(path));
    }

    bool moving = false;

    IEnumerator MoveAlongPath(List<Vector2Int> path)
    {
        Vector2Int destinationNode = path.Last();
        Vector2Int currentNode = path.First();
        path.RemoveAt(0);


        var t = 0.0f;
        var speed = 7.5f; // meters per second
        var timeToWalkAcrossTile = 1 / speed;

        while (Position != destinationNode)
        {
            t += Time.deltaTime;
            float progress = t / timeToWalkAcrossTile;

            var direction = new Vector3(currentNode.x - Position.x, currentNode.y - Position.y, 0.0f).normalized;
            transform.position = (Vector3Int)Position + (direction * progress) + new Vector3(0.5f, 0.5f, 0.0f);

            if (t > timeToWalkAcrossTile) {
                GridManager.SetEntityPosition(this, currentNode);

                if (path.Count > 0)
                {
                    t = 0.0f;
                    currentNode = path.First();
                    path.RemoveAt(0);
                }
            }

            yield return null;
        }
        GridManager.SetEntityPosition(this, destinationNode);

        moving = false;
    }
}
