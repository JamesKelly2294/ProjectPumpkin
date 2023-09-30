using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetEntity(Entity entity) {
        MovementBar.SetProgress((float)entity.Movement / (float)entity.MaxMovement);
        MovementText.text = "" + entity.Movement + " / " + entity.MaxMovement;

        ManaBar.SetProgress((float)entity.Mana / (float)entity.MaxMana);
        ManaText.text = "" + entity.Mana + " / " + entity.MaxMana;

        HealthBar.SetProgress((float)entity.Health / (float)entity.MaxHealth);
        HealthText.text = "" + entity.Health + " / " + entity.MaxHealth;
    }
}
