using System;
using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
public class StationDoorPanelLever : MonoBehaviour, IInteractable
{

    [SerializeField]
    InteractObjectData interactObjectData;
    Action interactAction = delegate { };
    new Rigidbody rigidbody;
    bool IInteractable.CanShow => !GameStates.Instance.MainDoorPanelLeverPulled;

    IInteractable.IObjectData IInteractable.ObjectData => interactObjectData;
    void Awake()
    {
        rigidbody = gameObject.GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        GameEvents.Instance.MainDoorPanelLeverPulled += OnMainDoorPanelLeverPulled;
    }

    private void OnDisable()
    {
        GameEvents.Instance.MainDoorPanelLeverPulled -= OnMainDoorPanelLeverPulled;
    }

    void IInteractable.Interact()
    {
        if (GameStates.Instance.MainDoorPanelLeverPulled)
        {
            return;
        }

        if (PullmanSequenceNetwork.TryGetInstance(out PullmanSequenceNetwork sequenceNetwork))
        {
            sequenceNetwork.RequestMainDoorPanelLeverPull();
        }
        else
        {
            GameEvents.Instance.RaiseMainDoorPanelLeverPulled();
        }
    }
    void IInteractable.Stop()
    {
    }

    private void OnMainDoorPanelLeverPulled()
    {
        interactAction();
        rigidbody.isKinematic = false;
        rigidbody.AddRelativeForce(new Vector3(0.5f, 0, -2), ForceMode.Impulse);
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
