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

    private void OnEnable()
    {
        GameEvents.Instance.StopLeverActivated += OnStopLeverActivated;
    }

    private void OnDisable()
    {
        GameEvents.Instance.StopLeverActivated -= OnStopLeverActivated;
    }

    void IInteractable.Interact()
    {
        if (!GameStates.Instance.StopLeverActivated)
        {
            if (PullmanSequenceNetwork.TryGetInstance(out PullmanSequenceNetwork sequenceNetwork))
            {
                sequenceNetwork.RequestStopLeverActivation();
            }
            else
            {
                GameEvents.Instance.ActivateStopLever();
            }
        }
    }

    void IInteractable.Stop()
    {
    }

    private void OnStopLeverActivated()
    {
        audioSource.Play();
        if (animation != null)
        {
            animation.Animation.ChangeDirection(true);
            animation.Play();
        }
    }
}
