using System;
using UnityEngine;
using UnityEngine.UI;

public class Chest : MonoBehaviour, IInteractable
{
    [SerializeField]
    AudioSource audioSource;
    [SerializeField]
    Animation animationOpen;
    [SerializeField]
    InteractObjectData interactObjectDataClosed, interactObjectDataOpened;
    Action onInteract = delegate { };
    InteractObjectData interactObjectCurrent = null;
    public bool IsOpened { get; private set; } = false;
    IInteractable.IObjectData IInteractable.ObjectData => interactObjectCurrent;
    bool IInteractable.CanShow => true;


    private void Awake()
    {
        animationOpen.clip.legacy = true;
        interactObjectCurrent = interactObjectDataClosed;
    }
    public void Interact()
    {
        animationOpen.ChangeDirection(!IsOpened);
        IsOpened = !IsOpened;
        interactObjectCurrent = IsOpened ? interactObjectDataOpened : interactObjectDataClosed;
        audioSource.Play();
        animationOpen.Play();
        onInteract();
    }
    public void Init(Action onInteract)
    {
        this.onInteract = onInteract;
    }
    void IInteractable.Stop()
    {
    }
}
