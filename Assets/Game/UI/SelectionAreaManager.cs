using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionAreaManager : MonoBehaviour
{

    public UnitSelectionArea unitSelectionArea;

    // Start is called before the first frame update
    void Start()
    {
        _gridManager = FindObjectOfType<GridManager>();
        OnSelectableChanged();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private GridManager _gridManager;

    public void OnSelectableChanged()
    {
        var selectable = _gridManager.SelectedSelectable;

        if (selectable != null)
        {
            var entity = selectable.GetComponent<Entity>();
            if (entity != null)
            {
                unitSelectionArea.gameObject.SetActive(true);
                unitSelectionArea.SetEntity(entity);
            }
            else
            {
                unitSelectionArea.gameObject.SetActive(false);
            }
        }
        else
        {
            unitSelectionArea.gameObject.SetActive(false);
        }
    }
}
