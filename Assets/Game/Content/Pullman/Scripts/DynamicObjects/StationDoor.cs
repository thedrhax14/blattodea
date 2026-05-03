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
    AudioSource audioSource;

    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        doorActivator.SetupInteraction(() =>
        {
            startRotateValve();
        }, true);
        triggerSmash.OnTriggerEnterAction += (val) =>
        {
            if (val.CompareTag(tagForActivate))
            {
                audioSource.clip = audioClipSmashed;
                audioSource.Play();
                //particlesSmashEffect.transform.position = triggerSmash.ClosestCollisionPoint;
                particlesSmashEffect.Play();
            }
        };
    }
    void startRotateValve()
    {
        particlesMetalImpactValve.Play();
        audioSource.clip = audioClipValve;
        audioSource.Play();
        animator.SetTrigger("Activate");

    }
    public void OpeningDoorEffects()
    {
        foreach (var particle in particlesMetalImpactDoor)
        {
            particle.Play();
        }
        audioSource.clip = audioClipDoorOpen;
        audioSource.Play();
    }
    public void MarkDoorOpened()
    {
        GameEvents.Instance.RaiseMainDoorOpened();
    }
}
