using UnityEngine;

public class StopLever : MonoBehaviour, IInteractable
{
    [SerializeField]
    AudioSource audioSource;
    [SerializeField]
    new SimpleAnimation animation;
    [SerializeField]
    InteractObject interactObject;
    string IInteractable.InteractText => interactObject.InteractText;

    KeyCode IInteractable.InteractKey => interactObject.InteractKey;

    bool IInteractable.LockCamera => false;

    Sprite IInteractable.Icon => interactObject.Icon;
    bool IInteractable.CanShow => !GameStates.Instance.StopLeverActivated;


    void IInteractable.Interact()
    {
        audioSource.Play();
        //animation.Animation.ChangeDirection(isActivated);
        //animation.Play();
        if (!GameStates.Instance.StopLeverActivated)
        {
            GameEvents.Instance.ActivateStopLever();
        }
    }

    void IInteractable.Stop()
    {
    }
}
