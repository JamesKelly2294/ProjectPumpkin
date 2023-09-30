using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    private GridManager _gridManager;
    private CameraControls _cameraControls;
    private Selectable _selectable;

    // Start is called before the first frame update
    void Start()
    {
        _gridManager = FindObjectOfType<GridManager>();
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
        throw new NotImplementedException();
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

            var path = _gridManager.CalculatePath(entityPos, targetPos, debugVisuals: true);
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
