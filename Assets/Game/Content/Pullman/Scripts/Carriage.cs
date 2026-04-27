using UnityEngine;

public class Carriage : MonoBehaviour
{
    [SerializeField]
    SimpleAnimation animationDoorsOpen;
    [SerializeField]
    AudioSource audioSource;
    [SerializeField]
    AudioClip audioClipDoorsOpen;
    private void Awake()
    {
        GameEvents.Instance.CarriageStopped += Instance_CarriageStopped;
    }

    private void Instance_CarriageStopped()
    {
        //animationDoorsOpen.Play();
        audioSource.clip = audioClipDoorsOpen;
        audioSource.Play();
    }
}
