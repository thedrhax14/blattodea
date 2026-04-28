using UnityEngine;

public class StopLever : MonoBehaviour, IInteractable
{
    [SerializeField]
    AudioSource audioSource;
    [SerializeField]
    new SimpleAnimation animation;
    [SerializeField]
    InteractObjectData interactObjectData;
    IInteractable.IObjectData IInteractable.ObjectData => interactObjectData;
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
