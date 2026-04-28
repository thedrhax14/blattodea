using UnityEngine;

public class StationDoor : MonoBehaviour
{
    [SerializeField]
    StationDoorActivator doorActivator;
    [SerializeField]
    ParticleSystem[] particlesMetalImpact;
    [SerializeField]
    Animator animator;
    [SerializeField]
    AudioClip audioClipValve, audioClipDoorOpen;
    AudioSource audioSource;
    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        doorActivator.Init(() =>
        {
            startRotateValve();
        });
    }
    void startRotateValve()
    {
        audioSource.clip = audioClipValve;
        audioSource.Play();
        animator.SetTrigger("Activate");
        foreach (var particle in particlesMetalImpact)
        {
            particle.Play();
        }
    }
    public void OpeningDoorEffects()
    {
        audioSource.clip = audioClipDoorOpen;
        audioSource.Play();
    }
    public void MarkDoorOpened()
    {
        GameEvents.Instance.RaiseMainDoorOpened();
    }
}
