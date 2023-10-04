using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using info.jacobingalls.jamkit;
using UnityEngine.EventSystems;
using TMPro;

[RequireComponent(typeof(PubSubSender))]
public class PlayerUnitPreview : MonoBehaviour, IPointerClickHandler
{
    public Entity Entity;

    public Image Icon;
    public Image IconBackground;
    public Image IconFrame;
    public Image RestIcon;
    public Sprite GoldFramePrefab;
    public Sprite SilverFramePrefab;
    public Color GoldColor;
    public Color SilverColor;
    public ProgressBar MovementBar;
    public ProgressBar ManaBar;
    public ProgressBar HealthBar;
    public GameObject PipsHolder;
    public Image PipsHolderFrame;
    public GameObject ActionPointPrefab;
    public GameObject UnavailableActionPointPrefab;
    public TextMeshProUGUI HotkeyLabel;

    private int availablePips = 0, unavailablePips;
    private bool isWaiting = false;

    // Start is called before the first frame update
    void Start()
    {
        SetEntity(Entity);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateProgress(Entity);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        GetComponent<PubSubSender>().Publish("entity.select.requested", Entity);
    }

    public void SetEntity(Entity entity) {
        Entity = entity;
        if (entity == null) { return; }
        Icon.sprite = Entity.Definition.Icon;
        IconBackground.color = Entity.Definition.Color;
        UpdateProgress(Entity);
    }

    public void UpdateProgress(Entity entity) {
        if (entity == null) { return; }
        MovementBar.SetProgress((float)entity.Movement / (float)entity.MaxMovement);
        ManaBar.SetProgress((float)entity.Mana / (float)entity.MaxMana);
        HealthBar.SetProgress((float)entity.Health / (float)entity.MaxHealth);
        RestIcon.enabled = entity.IsWaiting;
        // Update Pips
        int uPips = entity.MaxActionPoints - entity.ActionPoints;
        if (availablePips != entity.ActionPoints || unavailablePips != uPips || isWaiting != entity.IsWaiting) { 

            // Add actual pips
            availablePips = entity.ActionPoints;
            unavailablePips = uPips;
            isWaiting = entity.IsWaiting;
            foreach (Transform child in PipsHolder.transform) {
                Destroy(child.gameObject);
            }
            for (int i = 0; i < availablePips; i++) {
                GameObject.Instantiate(ActionPointPrefab, PipsHolder.transform);
            }
            for (int i = 0; i < uPips; i++) {
                GameObject.Instantiate(UnavailableActionPointPrefab, PipsHolder.transform);
            }

            // Update frame color
            if (availablePips > 0 && !entity.IsWaiting) {
                IconFrame.sprite = GoldFramePrefab;
                PipsHolderFrame.color = GoldColor;
            } else {
                IconFrame.sprite = SilverFramePrefab;
                PipsHolderFrame.color = SilverColor;
            }
        }
    }
}
