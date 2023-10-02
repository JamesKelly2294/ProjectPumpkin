using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using info.jacobingalls.jamkit;

public class LeafyGameManager : MonoBehaviour
{

    private TurnManager _turnManager;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (_turnManager == null) {
            _turnManager = FindObjectOfType<TurnManager>();
        }

        // We lost...
        if (_turnManager.OwnedEntities(Entity.OwnerKind.Player).Count == 0) {
            Debug.Log("Should show lose screen!");
            GetComponent<PubSubSender>().Publish("gameManager.showLose");
        }
    }
}
