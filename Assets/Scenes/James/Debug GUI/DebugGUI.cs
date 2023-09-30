using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DebugGUI : MonoBehaviour
{
    [Header("Entity Selection")]
    public Image EntityIcon;
    public TextMeshProUGUI EntityLabel;

    [Header("Turn Info")]
    public TextMeshProUGUI CurrentTurnLabel;
    public TextMeshProUGUI CurrentTeamLabel;
    public TextMeshProUGUI CurrentTeamEntityCountLabel;
    public TextMeshProUGUI CurrentTeamIsDoneLabel;


    private GridManager _gridManager;
    private TurnManager _turnManager;

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

        _turnManager = FindObjectOfType<TurnManager>();

        OnTurnChanged();
        OnTurnTeamChanged();
        OnTeamCanTakeActionChanged();
        OnTeamEntitiesThatCanTakeActionChanged();
    }
    
    public void OnTurnChanged()
    {
        CurrentTurnLabel.text = "Turn " + _turnManager.CurrentTurn;
    }

    public void OnTurnTeamChanged()
    {
        CurrentTurnLabel.text = _turnManager.CurrentTeam + "'s Turn";
    }

    public void OnTeamCanTakeActionChanged()
    {
        CurrentTeamIsDoneLabel.gameObject.SetActive(!_turnManager.CurrentTeamCanTakeAction);
    }

    public void OnTeamEntitiesThatCanTakeActionChanged()
    {
        var freeEntities = _turnManager.CurrentTeamEntitiesThatCanTakeAction.Count;
        var totalEntities = _turnManager.OwnedEntities(_turnManager.CurrentTeam).Count;

        CurrentTeamIsDoneLabel.gameObject.SetActive(freeEntities > 0);
        CurrentTeamEntityCountLabel.text = "[" + freeEntities + "/" + totalEntities + "]";
    }
}
