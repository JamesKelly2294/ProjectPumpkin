using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroppedItem : MonoBehaviour
{
    public Item Item;
    public SpriteRenderer VisualsRenderer;

    [HideInInspector]
    public GridManager GridManager;

    // Start is called before the first frame update
    void Awake()
    {
    }

    private void Start()
    {
        if (GridManager == null)
        {
            GridManager = FindObjectOfType<GridManager>();
        }

        VisualsRenderer.sprite = Item.Icon;
        VisualsRenderer.color = Item.Color;

        var position = new Vector2Int(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y));
        GridManager.RegisterItem(Item, position);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
