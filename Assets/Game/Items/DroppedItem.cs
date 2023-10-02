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
        if (GridManager == null)
        {
            GridManager = FindObjectOfType<GridManager>();
        }
        var position = new Vector2Int(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y));
        GridManager.RegisterItem(Item, position);
    }

    private void Start()
    {
        VisualsRenderer.sprite = Item.Icon;
        VisualsRenderer.color = Item.Color;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
