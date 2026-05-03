using System;
using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
public class StationDoorPanelLever : MonoBehaviour, IInteractable
{

    [SerializeField]
    InteractObjectData interactObjectData;
    Action interactAction = delegate { };
    new Rigidbody rigidbody;
    bool IInteractable.CanShow => true;

    IInteractable.IObjectData IInteractable.ObjectData => interactObjectData;
    void Awake()
    {
        rigidbody = gameObject.GetComponent<Rigidbody>();
    }

    void IInteractable.Interact()
    {
        interactAction();
        rigidbody.isKinematic = false;
        rigidbody.AddRelativeForce(new Vector3(0.5f, 0, -2), ForceMode.Impulse);
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
