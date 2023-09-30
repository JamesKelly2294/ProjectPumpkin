using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ActionButton : MonoBehaviour
{

    public Action Action;

    public Image Icon;
    public Image IconBackground;

    public TextMeshProUGUI TooltipTitle;
    public TextMeshProUGUI TooltipDescription;
    public TextMeshProUGUI TooltipFlavorText;
    public TooltipAmount ActionCostPrefab;
    public TooltipAmount ManaCostPrefab;
    public GameObject TooltipCostsHolder;

    public GameObject PipsHolder;
    public GameObject ActionPipPrefab;
    public GameObject ManaPipPrefab;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetAction(Action action) {
        Action = action;
        if (Action == null) { return; }
        Icon.sprite = Action.Definition.Icon;
        IconBackground.color = Action.Definition.Color;
        TooltipTitle.text = Action.Definition.Name;
        TooltipDescription.text = Action.Definition.Description;
        TooltipFlavorText.text = Action.Definition.FlavorText;

        // Rebuild Pips
        foreach (Transform child in PipsHolder.transform) {
            Destroy(child.gameObject);
        }
        for (int i = 0; i < Action.ActionPointCost; i++) {
            GameObject.Instantiate(ActionPipPrefab, PipsHolder.transform);
        }
        for (int i = 0; i < Action.MagicCost; i++) {
            GameObject.Instantiate(ManaPipPrefab, PipsHolder.transform);
        }

        // Add costs to Tooltip
        foreach (Transform child in TooltipCostsHolder.transform) {
            Destroy(child.gameObject);
        }
        if (Action.ActionPointCost > 0) {
            TooltipAmount tooltipAmount = GameObject.Instantiate(ActionCostPrefab, TooltipCostsHolder.transform);
            tooltipAmount.Amount.text = "" + Action.ActionPointCost;
        }
        if (Action.MagicCost > 0) {
            TooltipAmount tooltipAmount = GameObject.Instantiate(ManaCostPrefab, TooltipCostsHolder.transform);
            tooltipAmount.Amount.text = "" + Action.MagicCost;
        }

    }

    public void DoIt() {
        Action.Entity.InitiateActionAttempt(Action);
    }
}
