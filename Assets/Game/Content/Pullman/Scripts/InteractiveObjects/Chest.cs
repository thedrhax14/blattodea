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
    InteractObject interactObjectClosed, interactObjectOpened;
    Action onInteract = delegate { };
    InteractObject interactObjectCurrent = null;
    public bool IsOpened { get; private set; } = false;
    string IInteractable.InteractText => interactObjectCurrent.InteractText;

    KeyCode IInteractable.InteractKey => interactObjectCurrent.InteractKey;

    bool IInteractable.LockCamera => interactObjectCurrent.LockCamera;

    Sprite IInteractable.Icon => interactObjectCurrent.Icon;

    private void Awake()
    {
        animationOpen.clip.legacy = true;
        interactObjectCurrent = interactObjectClosed;
    }
    public void Interact()
    {
        foreach (AnimationState state in animationOpen)
        {
            if (!IsOpened)
            {
                state.speed = 1;
                state.normalizedTime = 0;
            }
            else
            {
                state.speed = -1;
                state.normalizedTime = 1;
            }

        }
        IsOpened = !IsOpened;
        interactObjectCurrent = IsOpened ? interactObjectOpened : interactObjectClosed;
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
