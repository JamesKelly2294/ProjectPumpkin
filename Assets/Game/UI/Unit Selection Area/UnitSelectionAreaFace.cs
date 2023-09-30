using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitSelectionAreaFace : MonoBehaviour
{

    public GameObject PipsHolder;
    public GameObject ActionPointPrefab;
    public GameObject UnavailableActionPointPrefab;
    public Image Icon;
    public Image IconBackground;

    public TextMeshProUGUI TooltipName;
    public TextMeshProUGUI TooltipDescription;
    public TextMeshProUGUI TooltipFlavorText;
    public TooltipAmount TooltipActionPointsAmount;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetEntity(Entity entity) {
        if (entity == null) { return; }
        Icon.sprite = entity.Definition.Icon;
        IconBackground.color = entity.Definition.Color;

        // Update Pips
        foreach (Transform child in PipsHolder.transform) {
            Destroy(child.gameObject);
        }
        for (int i = 0; i < (entity.ActionPoints); i++) {
            GameObject.Instantiate(ActionPointPrefab, PipsHolder.transform);
        }
        for (int i = 0; i < (entity.MaxActionPoints - entity.ActionPoints); i++) {
            GameObject.Instantiate(UnavailableActionPointPrefab, PipsHolder.transform);
        }

        // Update Tooltips
        TooltipName.text = entity.Definition.Name;
        TooltipDescription.text = entity.Definition.Description;
        TooltipFlavorText.text = entity.Definition.FlavorText;
        TooltipActionPointsAmount.Amount.text = "" + entity.MaxActionPoints;
    }
}
