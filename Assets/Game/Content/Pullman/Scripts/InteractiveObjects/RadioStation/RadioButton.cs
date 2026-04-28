using System;
using UnityEngine;
using UnityEngine.UI;

public class RadioButton : MonoBehaviour, IInteractable
{
    [SerializeField]
    Animation animationClick;
    [SerializeField]
    InteractObjectData interactObjectData;
    Action onInteract = delegate { };
    bool IInteractable.CanShow => true;
    IInteractable.IObjectData IInteractable.ObjectData => interactObjectData;

    private void Awake()
    {
        animationClick.clip.legacy = true;
    }
    public void Interact()
    {
        animationClick.Play();
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
