using info.jacobingalls.jamkit;
using System;
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

    private List<Entity> _entitiesWithAvailableActions;
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
            Button.interactable = !_turnManager.BlockingEventIsExecuting;

            _entitiesWithAvailableActions = _turnManager.CurrentTeamEntitiesThatCanTakeAction.Where(e => !e.ActedThisTurn).ToList();

            if (_entitiesWithAvailableActions.Count > 0)
            {
                _entityWithAvailableActions = _entitiesWithAvailableActions.First();
                EndTurnButtonImage.color = PlayerTurnStillWorkToDoColor;
                var name = _entityWithAvailableActions.ClassName.Length == 0 ? _entityWithAvailableActions.Name : _entityWithAvailableActions.ClassName;
                EndTurnButtonLabel.text = $"{name} ready!";
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
                var currentlySelectedSelectable = _playerInput.SelectedSelectable;
                var selectable = _entityWithAvailableActions.GetComponent<Selectable>();
                var remainingSelectables = _entitiesWithAvailableActions.Select(e => e.GetComponent<Selectable>()).Where(s => s != null).ToList();

                if (remainingSelectables.Count > 0)
                {
                    remainingSelectables.RemoveAt(0);
                }

                if (selectable != null)
                {
                    if (currentlySelectedSelectable == selectable)
                    {
                        // The player is clicking the button on a unit already selected
                        // Auto wait for convenience
                        foreach (var action in _entityWithAvailableActions.Actions)
                        {
                            if (action.Name.ToLower() == "wait")
                            {
                                var actionSelectionRequest = new ActionSelectionRequest()
                                {
                                    Action = action,
                                    Entity = _entityWithAvailableActions,
                                };

                                _playerInput.RequestActionSelection(actionSelectionRequest);

                                if (remainingSelectables.Count > 0)
                                {
                                    _playerInput.Select(remainingSelectables.First());
                                }

                                break;
                            }
                        }
                    }
                    else
                    {
                        _playerInput.Select(selectable);
                    }
                }
            }
        }
    }
}
