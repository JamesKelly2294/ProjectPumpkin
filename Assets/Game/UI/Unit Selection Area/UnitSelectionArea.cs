using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSelectionArea : MonoBehaviour
{

    public Entity Entity;

    public ActionButtons ActionButtons;
    public UnitSelectionAreaStats UnitSelectionAreaStats;
    public UnitSelectionAreaFace unitSelectionAreaFace;

    private bool setOnce = true;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {   
        if (setOnce) {
            SetEntity(Entity);
            setOnce = false;
        }
    }

    public void SetEntity(Entity entity) {
        Entity = entity;
        if (Entity == null) { return; }

        ActionButtons.SetEntity(Entity);
        UnitSelectionAreaStats.SetEntity(Entity);
        unitSelectionAreaFace.SetEntity(Entity);
    }
}
