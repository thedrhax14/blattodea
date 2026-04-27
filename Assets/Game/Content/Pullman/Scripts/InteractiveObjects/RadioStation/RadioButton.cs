using System;
using UnityEngine;
using UnityEngine.UI;

public class RadioButton : MonoBehaviour, IInteractable
{
    [SerializeField]
    Animation animationClick;
    [SerializeField]
    InteractObject interactObject;
    Action onInteract = delegate { };

    string IInteractable.InteractText => interactObject.InteractText;

    KeyCode IInteractable.InteractKey => interactObject.InteractKey;

    bool IInteractable.LockCamera => interactObject.LockCamera;

    Sprite IInteractable.Icon => interactObject.Icon;

    bool IInteractable.CanShow => true;

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
