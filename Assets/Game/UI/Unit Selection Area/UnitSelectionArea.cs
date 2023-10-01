using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UnitSelectionArea : MonoBehaviour
{

    public Entity Entity;
    public TextMeshProUGUI Name;
    public ActionButtons ActionButtons;
    public UnitSelectionAreaStats UnitSelectionAreaStats;
    public UnitSelectionAreaFace unitSelectionAreaFace;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {   
        SetEntity(Entity);
    }

    public void SetEntity(Entity entity) {
        Entity = entity;
        if (Entity == null) { return; }

        Name.text = Entity.ClassName.Length == 0 ? Entity.Name : $"{Entity.Name}, {Entity.ClassName}";
        ActionButtons.SetEntity(Entity);
        UnitSelectionAreaStats.SetEntity(Entity);
        unitSelectionAreaFace.SetEntity(Entity);
    }
}
