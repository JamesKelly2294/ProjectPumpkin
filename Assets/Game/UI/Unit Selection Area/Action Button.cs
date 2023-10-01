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
    public TextMeshProUGUI TooltipCostText;
    public GameObject TooltipCostsHolder;
    public GameObject PipsHolder;
    public GameObject ActionPipPrefab;
    public GameObject ManaPipPrefab;
    private TurnManager turnManager;
    public Button Button;
    public CanvasGroup CanvasGroup;
    public CanvasGroup PipsCanvasGroup;

    // Start is called before the first frame update
    void Start()
    {
        turnManager = FindObjectOfType<TurnManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetAction(Action action) {
        Action = action;
        if (Action == null) { return; }
        ConsiderDisabiling();

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
        if(Action.ActionPointCost > 0 || Action.MagicCost > 0) {
            TooltipCostText.text = "COST:";
        } else {
            TooltipCostText.text = "FREE!";
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
        var tm = FindObjectOfType<TurnManager>();

        if (tm != null)
        {
            tm.SubmitAction(Action);
        }
        else
        {
            Debug.LogError("Attempting to submit action for execution, but unable to find turn manager.");
        }
    }

    public void ConsiderDisabiling() {

        // Computer Science, amiright?
        if (turnManager == null) { turnManager = FindObjectOfType<TurnManager>(); }
        if (turnManager == null) { return; }

        bool disabled = turnManager.BlockingEventIsExecuting;
        disabled |= (turnManager.CurrentTeam != Entity.OwnerKind.Player);
        disabled |= (Action.Entity.Owner != Entity.OwnerKind.Player);
        disabled |= !Action.Entity.CanAffordAction(Action);

        Button.interactable = !disabled;
        CanvasGroup.alpha = disabled ? 0.5f : 1f;
        PipsCanvasGroup.alpha = disabled ? 0.5f : 1f;
    }

    Color AdjustBrightness(Color color, float factor)
    {
        float h, s, v;
        Color.RGBToHSV(color, out h, out s, out v);
        v = Mathf.Clamp01(v * factor);
        return Color.HSVToRGB(h, s, v);
    }

}
