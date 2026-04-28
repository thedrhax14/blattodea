using System.Collections;
using UnityEngine;

public class Carriage : MonoBehaviour
{
    [SerializeField]
    ParticleSystem[] particlesSteam;
    [SerializeField]
    SimpleAnimation animationDoorsOpen;
    [SerializeField]
    AudioSource audioSourceEffects, audioSourceBackground;
    [SerializeField]
    AudioClip audioClipDoorsOpen, audioClipStartStopping;
    private void OnEnable()
    {
        GameEvents.Instance.CarriageStopped += Instance_CarriageStopped;
        GameEvents.Instance.CarriageStartStopping += Instance_CarriageStartStopping;
    }

    private void OnDisable()
    {
        GameEvents.Instance.CarriageStopped -= Instance_CarriageStopped;
        GameEvents.Instance.CarriageStartStopping -= Instance_CarriageStartStopping;
    }
    private void Instance_CarriageStartStopping()
    {
        StartCoroutine(stopping());
    }

    private void Instance_CarriageStopped()
    {
        animationDoorsOpen.Play();
        audioSourceEffects.clip = audioClipDoorsOpen;
        audioSourceEffects.Play();
        foreach (var particle in particlesSteam)
        {
            particle.Play();
        }
    }
    IEnumerator stopping()
    {
        audioSourceEffects.clip = audioClipStartStopping;
        audioSourceEffects.Play();
        yield return new WaitForSeconds(0.3f);
        audioSourceBackground.Stop();
    }
}
