using UnityEngine;

public class StationDoor : MonoBehaviour
{
    [SerializeField]
    StationDoorActivator doorActivator;
    [SerializeField]
    ParticleSystem[] particlesMetalImpactDoor;
    [SerializeField]
    ParticleSystem particlesMetalImpactValve;
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
