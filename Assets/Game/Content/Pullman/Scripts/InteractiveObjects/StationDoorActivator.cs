using System;
using UnityEngine;

public class StationDoorActivator : MonoBehaviour, IInteractable
{
    [SerializeField]
    InteractObjectData interactObjectData;
    bool IInteractable.CanShow => !GameStates.Instance.MainDoorStartsOpening && !GameStates.Instance.MainDoorOpened;
    Action interactAction = delegate { };
    IInteractable.IObjectData IInteractable.ObjectData => interactObjectData;
    void IInteractable.Interact()
    {
        interactAction();
    }
    void IInteractable.Stop()
    {
    }

    public void SetupInteraction(Action interactAction, bool append = false)
    {
        if (append)
        {
            this.interactAction += interactAction;
        }
        else
        {
            this.interactAction = interactAction;
        }
    }
}
