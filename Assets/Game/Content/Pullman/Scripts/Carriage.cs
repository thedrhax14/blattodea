using UnityEngine;

public class Carriage : MonoBehaviour
{
    [SerializeField]
    SimpleAnimation animationDoorsOpen;
    [SerializeField]
    AudioSource audioSource;
    [SerializeField]
    AudioClip audioClipDoorsOpen;
    private void OnEnable()
    {
        GameEvents.Instance.CarriageStopped += Instance_CarriageStopped;
    }
    private void OnDisable()
    {
        GameEvents.Instance.CarriageStopped -= Instance_CarriageStopped;
    }
    private void Instance_CarriageStopped()
    {
        animationDoorsOpen.Play();
        audioSource.clip = audioClipDoorsOpen;
        audioSource.volume = 1.0f;
        audioSource.Play();
    }
}
