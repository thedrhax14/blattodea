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
            startRotateValve();
        }, true);
        triggerSmash.OnTriggerEnterAction += (val) =>
        {
            if (val.CompareTag(tagForActivate))
            {
                audioSourceSmashTrigger.clip = audioClipSmashed;
                audioSourceSmashTrigger.Play();
                //particlesSmashEffect.transform.position = triggerSmash.ClosestCollisionPoint;
                particlesSmashEffect.Play();
            }
        };
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
        foreach (var particle in particlesMetalImpactDoor)
        {
            particle.Play();
        }
        audioSourceMain.clip = audioClipDoorOpen;
        audioSourceMain.Play();
    }
    public void MarkDoorOpened()
    {
        GameEvents.Instance.RaiseMainDoorOpened();
    }
}
