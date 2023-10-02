using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Diagnostics.CodeAnalysis;
using info.jacobingalls.jamkit;

[RequireComponent(typeof(PubSubSender))]
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
    public TooltipAmount HealthCostPrefab;
    public TooltipAmount MovementCostPrefab;
    public TextMeshProUGUI TooltipCostText;
    public GameObject TooltipCostsHolder;
    public TextMeshProUGUI TooltipDealsText;
    public GameObject TooltipDealsHolder;
    public GameObject PipsHolder;
    public GameObject ActionPipPrefab;
    public GameObject ManaPipPrefab;
    public GameObject HealthPipPrefab;
    public GameObject MovementPipPrefab;
    private TurnManager turnManager;
    public Button Button;
    public CanvasGroup CanvasGroup;
    public CanvasGroup PipsCanvasGroup;
    public Outline Outline;

    private PlayerInput _playerInput;

    // Start is called before the first frame update
    void Start()
    {
        turnManager = FindObjectOfType<TurnManager>();

        _playerInput = FindObjectOfType<PlayerInput>();
        if (_playerInput != null )
        {
            Outline.enabled = _playerInput.SelectedAction == Action;
        }
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
        for (int i = 0; i < Action.HealthCost; i++)
        {
            GameObject.Instantiate(HealthPipPrefab, PipsHolder.transform);
        }
        for (int i = 0; i < Action.MovementCost; i++)
        {
            GameObject.Instantiate(MovementPipPrefab, PipsHolder.transform);
        }

        // Add costs to Tooltip
        foreach (Transform child in TooltipCostsHolder.transform) {
            Destroy(child.gameObject);
        }
        if(Action.ActionPointCost > 0 || Action.MagicCost > 0 || Action.HealthCost > 0 || Action.MovementCost > 0) {
            if (Action.CostIsPerTile)
            {
                TooltipCostText.text = "COST/TILE:";
            }
            else
            {
                TooltipCostText.text = "COST:";
            }
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
        if (Action.HealthCost > 0)
        {
            TooltipAmount tooltipAmount = GameObject.Instantiate(HealthCostPrefab, TooltipCostsHolder.transform);
            tooltipAmount.Amount.text = "" + Action.HealthCost;
        }
        if (Action.MovementCost > 0)
        {
            TooltipAmount tooltipAmount = GameObject.Instantiate(MovementCostPrefab, TooltipCostsHolder.transform);
            tooltipAmount.Amount.text = "" + Action.MovementCost;
        }

        // Add deals to Tooltip
        foreach (Transform child in TooltipDealsHolder.transform) {
            Destroy(child.gameObject);
        }
        if (Action.Damage > 0) {
            TooltipDealsHolder.SetActive(true);
            TooltipDealsText.gameObject.SetActive(true);
            TooltipAmount tooltipAmount = GameObject.Instantiate(HealthCostPrefab, TooltipDealsHolder.transform);
            tooltipAmount.Amount.text = "" + Action.Damage;
        } else {
            TooltipDealsHolder.SetActive(false);
            TooltipDealsText.gameObject.SetActive(false);
        }
    }

    public void DoIt() {
        var actionSelectionRequest = new ActionSelectionRequest()
        {
            Action = Action,
            Entity = Action.Entity,
        };

        GetComponent<PubSubSender>().Publish("entity.action.selection.requested", actionSelectionRequest);
    }

    public void UpdateSelectionOutline(PubSubListenerEvent e)
    {
        var a = e.value as Action;
        Outline.enabled = a == Action;
    }

    public void ConsiderDisabiling() {

        // Computer Science, amiright?
        if (turnManager == null) { turnManager = FindObjectOfType<TurnManager>(); }
        if (turnManager == null) { return; }
        if (Action == null) { return; }

        bool disabled = turnManager.BlockingEventIsExecuting;
        disabled |= (turnManager.CurrentTeam != Entity.OwnerKind.Player);
        disabled |= (Action.Entity.Owner != Entity.OwnerKind.Player);
        disabled |= !Action.Entity.CanAffordAction(Action);
        disabled |= (Action.Entity.IsWaiting && !Action.CanExecuteWhileWaiting);

        var buttonShouldBeInteractable = !disabled;
        if (Button.interactable != buttonShouldBeInteractable)
        {
            Button.interactable = buttonShouldBeInteractable;
        }
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
