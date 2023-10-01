using info.jacobingalls.jamkit;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PubSubSender))]
public class EndTurnButton : MonoBehaviour
{
    public StandardButton Button;
    public Image EndTurnButtonImage;
    public TextMeshProUGUI EndTurnButtonLabel;

    public Color PlayerTurnStillWorkToDoColor = Color.yellow;
    public Color PlayerTurnDoneColor = Color.green;
    public Color NonPlayerTurnColor = Color.gray;

    private TurnManager _turnManager;

    private bool _canEndTurn = true;
    private PubSubSender _sender;
    private PlayerInput _playerInput;

    private Entity _entityWithAvailableActions;

    // Start is called before the first frame update
    void Start()
    {
        _turnManager = FindObjectOfType<TurnManager>();
        _sender = GetComponent<PubSubSender>();
        _playerInput = FindObjectOfType<PlayerInput>();

        TurnStateDidChange();
    }

    public void TurnStateDidChange()
    {
        _canEndTurn = false;
        _entityWithAvailableActions = null;

        if (_turnManager.CurrentTeam == Entity.OwnerKind.Player)
        {
            Button.interactable = true;
            if (_turnManager.CurrentTeamEntitiesThatCanTakeAction.Count > 0)
            {
                _entityWithAvailableActions = _turnManager.CurrentTeamEntitiesThatCanTakeAction.First();
                EndTurnButtonImage.color = PlayerTurnStillWorkToDoColor;
                EndTurnButtonLabel.text = $"{_entityWithAvailableActions.Name} ready!";
            }
            else
            {
                EndTurnButtonImage.color = PlayerTurnDoneColor;
                EndTurnButtonLabel.text = $"End Turn";
                _canEndTurn = true;
            }
        }
        else
        {
            Button.interactable = false;
            EndTurnButtonImage.color = NonPlayerTurnColor;
            EndTurnButtonLabel.text = $"{_turnManager.CurrentTeam}'s Turn...";
        }
    }

    public void EndTurnButtonOnClick()
    {
        if (_canEndTurn)
        {
            _sender.Publish("end_turn_button.pressed");
        }
        else
        {
            if (_entityWithAvailableActions != null)
            {
                var selectable = _entityWithAvailableActions.GetComponent<Selectable>();
                if (selectable != null)
                {
                    _playerInput.Select(selectable);
                }
            }
        }
    }
}
