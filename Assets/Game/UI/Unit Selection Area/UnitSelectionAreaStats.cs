using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using info.jacobingalls.jamkit;

public class UnitSelectionAreaStats : MonoBehaviour
{

    public TextMeshProUGUI MovementText;
    public TextMeshProUGUI ManaText;
    public TextMeshProUGUI HealthText;

    public ProgressBar MovementBar;
    public ProgressBar ManaBar;
    public ProgressBar HealthBar;

    public LayoutElement MovementPane;
    public LayoutElement ManaPane;
    public LayoutElement HealthPane;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetEntity(Entity entity) {
        MovementPane.preferredWidth = Mathf.Min(entity.MaxMovement, Entity.TheoreticalMovementMax) * 20f;
        MovementBar.SetProgress((float)entity.Movement / (float)entity.MaxMovement);
        MovementText.text = "" + entity.Movement + " / " + entity.MaxMovement;
        
        if (entity.MaxMana > 0) {
            ManaPane.gameObject.SetActive(true);
            ManaPane.preferredWidth = Mathf.Min(entity.MaxMana, Entity.TheoreticalManaMax) * 20f;
            ManaBar.SetProgress((float)entity.Mana / (float)entity.MaxMana);
            ManaText.text = "" + entity.Mana + " / " + entity.MaxMana;
        } else {
            ManaPane.gameObject.SetActive(false);
            ManaText.text = "N/A";
        }

        HealthPane.preferredWidth = Mathf.Min(entity.MaxHealth, Entity.TheoreticalHealthMax) * 20f;
        HealthBar.SetProgress((float)entity.Health / (float)entity.MaxHealth);
        HealthText.text = "" + entity.Health + " / " + entity.MaxHealth;
    }
}
