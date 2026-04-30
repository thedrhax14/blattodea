using System;
using UnityEngine;

public class StationDoorActivator : MonoBehaviour, IInteractable
{
    [SerializeField]
    InteractObjectData interactObjectData;
    bool IInteractable.CanShow => !GameStates.Instance.MainDoorStartsOpening && !GameStates.Instance.MainDoorOpened;
    Action interact = delegate { };
    IInteractable.IObjectData IInteractable.ObjectData => interactObjectData;
    void IInteractable.Interact()
    {
        interact();
    }
    void IInteractable.Stop()
    {
    }
    public void Init(Action interact)
    {
        this.interact = interact;
    }
}
