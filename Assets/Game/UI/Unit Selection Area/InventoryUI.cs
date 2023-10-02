using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public GameObject InventoryContainer;
    public GameObject InventorySlotContainer;

    public GameObject InventorySlotPrefab;

    public TextMeshProUGUI CountLabel;

    private PlayerInput _playerInput;

    // Start is called before the first frame update
    void Start()
    {
        _playerInput = FindObjectOfType<PlayerInput>();
    }

    List<GameObject> _cachedInventorySlots = new();

    public void UpdateVisuals()
    {
        var selectedSelectable = _playerInput.SelectedSelectable;

        if (selectedSelectable == null)
        {
            InventoryContainer.SetActive(false);
            return;
        }
        var inventoryForSelected = selectedSelectable.GetComponent<Inventory>();
        if (inventoryForSelected == null)
        {
            InventoryContainer.SetActive(false);
            return;
        }

        InventoryContainer.SetActive(true);

        if (inventoryForSelected.Capacity != _cachedInventorySlots.Capacity)
        {

            for (var i = 0; i < InventorySlotContainer.transform.childCount; i++)
            {
                Destroy(InventorySlotContainer.transform.GetChild(i).gameObject);
            }

            _cachedInventorySlots.Clear();

            for (var i = 0; i < inventoryForSelected.Capacity; i++)
            {
                var go = Instantiate(InventorySlotPrefab, InventorySlotContainer.transform);
                go.transform.name = "Inventory Slot";
                _cachedInventorySlots.Add(go);
            }
        }

        for (var i = 0; i < inventoryForSelected.Items.Count; i++)
        {
            var slotImage = _cachedInventorySlots[i].transform.Find("Image").GetComponent<Image>();
            slotImage.sprite = inventoryForSelected.Items[i].Icon;
            slotImage.color = inventoryForSelected.Items[i].Color;
            slotImage.gameObject.SetActive(true);
        }

        for (var i = inventoryForSelected.Items.Count; i < inventoryForSelected.Capacity; i++)
        {
            var slotImage = _cachedInventorySlots[i].transform.Find("Image").GetComponent<Image>();
            slotImage.gameObject.SetActive(false);
        }

        CountLabel.text = $"{inventoryForSelected.Items.Count} of {inventoryForSelected.Capacity}";
    }
}
