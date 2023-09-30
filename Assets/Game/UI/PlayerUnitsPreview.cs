using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerUnitsPreview : MonoBehaviour
{

    private TurnManager turnManager;
    public GameObject Holder;
    public PlayerUnitPreview PlayerUnitPreviewPrefab;
    private List<Entity> entities;

    // Start is called before the first frame update
    void Start()
    {
        turnManager = FindObjectOfType<TurnManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShouldUpdate() {
        List<Entity> newEntities = turnManager.OwnedEntities(Entity.OwnerKind.Player);
        if(entities != null && entities.SequenceEqual(newEntities)) { return; }
        entities = newEntities;

        foreach(Transform child in Holder.transform) {
            GameObject.Destroy(child.gameObject);
        }
        foreach(Entity entity in turnManager.OwnedEntities(Entity.OwnerKind.Player)) {
            PlayerUnitPreview unit = GameObject.Instantiate(PlayerUnitPreviewPrefab, Holder.transform);
            unit.SetEntity(entity);
        }
    }
}
