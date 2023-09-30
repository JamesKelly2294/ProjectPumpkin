using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionButtons : MonoBehaviour
{

    public ActionButton ActionButtonPrefab;

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

        foreach (Transform child in gameObject.transform) {
            Destroy(child.gameObject);
        }

        foreach(Action action in entity.Actions) {
            ActionButton actionButton = GameObject.Instantiate(ActionButtonPrefab, gameObject.transform);
            actionButton.SetAction(action);
        }
    }
}
