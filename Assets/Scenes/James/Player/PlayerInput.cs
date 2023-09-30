using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    private GridManager _gridManager;
    private TurnManager _turnManager;
    private CameraControls _cameraControls;
    private Selectable _selectable;

    // Start is called before the first frame update
    void Start()
    {
        _gridManager = FindObjectOfType<GridManager>();
        _turnManager = FindObjectOfType<TurnManager>();
        _cameraControls = FindObjectOfType<CameraControls>();

        SelectableDidChange();
    }

    private Entity SelectedEntity()
    {
        if (_gridManager.SelectedSelectable == null) { return null; }

        return _gridManager.SelectedSelectable.GetComponent<Entity>();
    }

    public void SelectableDidChange()
    {
        _selectable = _gridManager.SelectedSelectable;
    }

    private bool CanTakePrimaryAction()
    {
        return false;
    }

    private void TakePrimaryAction()
    {
        // THIS IS A TEST
        //Debug.Log("Taking primary action!");

        //var entity = SelectedEntity();

        //if (entity != null)
        //{
        //    entity.ExecuteAction(entity.Actions.First(), ignoringCost: true);
        //}
    }

    private bool CanTakeSecondaryAction()
    {
        return SelectedEntity() != null && 
            !CameraControlsTakingPriority() &&
            _gridManager.HoveredTilePosition != null;
    }

    private bool CameraControlsTakingPriority()
    {
        if (_cameraControls == null) { return false; }
        return _cameraControls.CameraIsPanning;
    }

    private void TakeSecondaryAction()
    {
        Debug.Log("Taking secondary action!");

        var highlitedTilePosition = _gridManager.HoveredTilePosition.Value;

        var entity = SelectedEntity();

        if (entity != null)
        {
            var entityPos = entity.Position;
            var targetPos = highlitedTilePosition;

            var path = _gridManager.CalculatePath(entityPos, targetPos, debugVisuals: false);
            entity.Move(path);
        }
    }

    private bool SkipUpdate
    {
        get
        {
            return _selectable == null;
        }
    }

    private void Update()
    {
        if (SkipUpdate) { return; }

        if (CanTakePrimaryAction() && Input.GetMouseButtonUp(0))
        {
            TakePrimaryAction();
        }

        if (CanTakeSecondaryAction() && Input.GetMouseButtonUp(1))
        {
            TakeSecondaryAction();
        }
    }
}
