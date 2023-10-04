using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ActionButtons : MonoBehaviour
{

    public ActionButton ActionButtonPrefab;
    private List<Action> actions;

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
        if (actions == null || !entity.Actions.SequenceEqual(actions)) { 
            actions = entity.Actions;

            foreach (Transform child in gameObject.transform) {
                Destroy(child.gameObject);
            }

            var i = 0;
            var hotkeyMappings = new List<string> { "Q", "W", "E", "R", "F"};
            foreach(Action action in actions) {
                ActionButton actionButton = GameObject.Instantiate(ActionButtonPrefab, gameObject.transform);
                actionButton.gameObject.SetActive(false);
                actionButton.SetAction(action);
                actionButton.gameObject.SetActive(true);
                actionButton.HotkeyLabel.text = $"{hotkeyMappings[i]}";

                i += 1;
            }
        }
    }
}
