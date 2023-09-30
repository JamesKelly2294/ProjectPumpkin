using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using info.jacobingalls.jamkit;

public class PlayerUnitPreview : MonoBehaviour
{
    public Entity Entity;

    public Image Icon;
    public Image IconBackground;
    public ProgressBar MovementBar;
    public ProgressBar ManaBar;
    public ProgressBar HealthBar;
    public GameObject PipsHolder;
    public GameObject ActionPointPrefab;
    public GameObject UnavailableActionPointPrefab;

    private int availablePips = 0, unavailablePips;

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

        // Update Pips
        int uPips = entity.MaxActionPoints - entity.ActionPoints;
        if (availablePips != entity.ActionPoints || unavailablePips != uPips) { 
            availablePips = entity.ActionPoints;
            unavailablePips = uPips;
            foreach (Transform child in PipsHolder.transform) {
                Destroy(child.gameObject);
            }
            for (int i = 0; i < availablePips; i++) {
                GameObject.Instantiate(ActionPointPrefab, PipsHolder.transform);
            }
            for (int i = 0; i < uPips; i++) {
                GameObject.Instantiate(UnavailableActionPointPrefab, PipsHolder.transform);
            }
        }
    }
}
