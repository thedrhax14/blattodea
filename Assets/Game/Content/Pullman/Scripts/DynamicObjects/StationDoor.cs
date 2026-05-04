using UnityEngine;

public class StationDoor : MonoBehaviour
{
    [SerializeField]
    string tagForActivate;
    [SerializeField]
    ParticleSystem particlesSmashEffect;
    [SerializeField]
    Trigger triggerSmash;
    [SerializeField]
    StationDoorActivator doorActivator;
    [SerializeField]
    ParticleSystem[] particlesMetalImpactDoor;
    [SerializeField]
    ParticleSystem particlesMetalImpactValve;
    [SerializeField]
    Animator animator;
    [SerializeField]
    AudioClip audioClipValve, audioClipDoorOpen, audioClipSmashed;
    [SerializeField]
    AudioSource audioSourceMain, audioSourceSmashTrigger;

    private void Awake()
    {
        doorActivator.SetupInteraction(() =>
        {
            if (PullmanSequenceNetwork.TryGetInstance(out PullmanSequenceNetwork sequenceNetwork))
            {
                sequenceNetwork.RequestMainDoorOpening();
            }
            else
            {
                GameEvents.Instance.RaiseMainDoorOpeningStarted();
            }
        }, true);

        if (triggerSmash != null)
        {
            triggerSmash.OnTriggerEnterAction += TriggerSmash_OnTriggerEnterAction;
        }
    }

    private void OnEnable()
    {
        GameEvents.Instance.MainDoorOpeningStarted += startRotateValve;
        GameEvents.Instance.MainDoorOpeningEffectsTriggered += PlayOpeningDoorEffects;
        GameEvents.Instance.MainDoorSmashEffectsTriggered += PlaySmashEffects;
    }

    private void OnDisable()
    {
        GameEvents.Instance.MainDoorOpeningStarted -= startRotateValve;
        GameEvents.Instance.MainDoorOpeningEffectsTriggered -= PlayOpeningDoorEffects;
        GameEvents.Instance.MainDoorSmashEffectsTriggered -= PlaySmashEffects;
        if (triggerSmash != null)
        {
            triggerSmash.OnTriggerEnterAction -= TriggerSmash_OnTriggerEnterAction;
        }
    }

    private void TriggerSmash_OnTriggerEnterAction(Collider val)
    {
        if (!val.CompareTag(tagForActivate))
        {
            return;
        }

        if (PullmanSequenceNetwork.TryGetInstance(out PullmanSequenceNetwork sequenceNetwork))
        {
            sequenceNetwork.ReportMainDoorSmashEffects();
        }
        else
        {
            GameEvents.Instance.RaiseMainDoorSmashEffectsTriggered();
        }
    }

    void startRotateValve()
    {
        particlesMetalImpactValve.Play();
        audioSourceMain.clip = audioClipValve;
        audioSourceMain.Play();
        animator.SetTrigger("Activate");
    }

    public void OpeningDoorEffects()
    {
        if (PullmanSequenceNetwork.TryGetInstance(out PullmanSequenceNetwork sequenceNetwork))
        {
            sequenceNetwork.ReportMainDoorOpeningEffects();
            return;
        }

        GameEvents.Instance.RaiseMainDoorOpeningEffectsTriggered();
    }

    private void PlayOpeningDoorEffects()
    {
        foreach (var particle in particlesMetalImpactDoor)
        {
            particle.Play();
        }
        audioSourceMain.clip = audioClipDoorOpen;
        audioSourceMain.Play();
    }

    private void PlaySmashEffects()
    {
        audioSourceSmashTrigger.clip = audioClipSmashed;
        audioSourceSmashTrigger.Play();
        particlesSmashEffect.Play();
    }

    public void MarkDoorOpened()
    {
        if (PullmanSequenceNetwork.TryGetInstance(out PullmanSequenceNetwork sequenceNetwork))
        {
            sequenceNetwork.ReportMainDoorOpened();
        }
        else
        {
            GameEvents.Instance.RaiseMainDoorOpened();
        }
    }
}
