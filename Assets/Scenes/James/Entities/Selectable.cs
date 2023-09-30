using info.jacobingalls.jamkit;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Events;

public interface ISelectable
{
    void OnSelected();
    void OnDeselected();
}

[RequireComponent(typeof(PubSubSender))]
public class Selectable : MonoBehaviour
{
    public UnityEvent OnSelected;
    public UnityEvent OnDeselected;

    public void Select()
    {
        ISelectable[] selectables = GetComponents<MonoBehaviour>().OfType<ISelectable>().ToArray();

        foreach (var selectable in selectables)
        {
            selectable.OnSelected();
        }

        OnSelected.Invoke();

        GetComponent<PubSubSender>().Publish("selectable.selected", this);
    }

    public void Deselect()
    {
        ISelectable[] selectables = GetComponents<MonoBehaviour>().OfType<ISelectable>().ToArray();

        foreach (var selectable in selectables)
        {
            selectable.OnDeselected();
        }

        OnDeselected.Invoke();

        GetComponent<PubSubSender>().Publish("selectable.deselected", this);
    }

    public Entity AsEntity()
    {
        return GetComponent<Entity>();
    }
}
