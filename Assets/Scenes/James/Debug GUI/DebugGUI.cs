using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DebugGUI : MonoBehaviour
{
    public Image EntityIcon;
    public TextMeshProUGUI EntityLabel;
    
    private GridManager _gridManager;

    public void OnSelectableChanged()
    {
        var selectable = _gridManager.SelectedSelectable;

        if (selectable != null)
        {
            var entity = selectable.GetComponent<Entity>();
            if (entity != null)
            {
                EntityIcon.sprite = entity.Definition.Icon;
                EntityLabel.text = entity.Definition.Name + "(" + entity.gameObject.name + ")";

                EntityIcon.enabled = true;
                EntityLabel.enabled = true;
            }
            else
            {
                EntityIcon.enabled = false;
                EntityLabel.enabled = false;
            }
        }
        else
        {
            EntityIcon.enabled = false;
            EntityLabel.enabled = false;
        }
    }

    public void Start()
    {
        _gridManager = FindObjectOfType<GridManager>();
        OnSelectableChanged();
    }
}
