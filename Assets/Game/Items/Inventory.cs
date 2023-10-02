using info.jacobingalls.jamkit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PubSubSender))]
public class Inventory : MonoBehaviour
{
    [Range(0, 20)]
    public int Capacity = 10;

    public List<ItemDefinition> Items {  get { return _items; } }
    [SerializeField]
    private List<ItemDefinition> _items;

    private PubSubSender _pubSubSender;

    private void Awake()
    {
        _pubSubSender = GetComponent<PubSubSender>();
    }

    public bool PickupItem(Item item)
    {
        if (Items.Count >= Capacity)
        {
            return false;
        }

        _items.Add(item.Definition);

        _pubSubSender.Publish("inventory.items.changed", this);

        return true;
    }

    public bool RemoveAllItems()
    {
        _items.Clear();

        _pubSubSender.Publish("inventory.items.changed", this);

        return true;
    }
}
