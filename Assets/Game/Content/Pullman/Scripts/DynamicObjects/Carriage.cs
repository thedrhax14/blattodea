using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class Carriage : MonoBehaviour
{
    [SerializeField]
    float leavingDelay = 5;
    [SerializeField]
    AudioMixer audioMixer;
    [SerializeField]
    Trigger doorClosingTrigger;
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
        GameEvents.Instance.MainDoorOpened += Instance_MainDoorOpened;
        doorClosingTrigger.gameObject.SetActive(false);
        doorClosingTrigger.OnTriggerEnterAction += DoorClosingTrigger_OnTriggerEnterAction;
    }

    private void DoorClosingTrigger_OnTriggerEnterAction(Collider obj)
    {
        doorsChangeState(false);
        doorClosingTrigger.gameObject.SetActive(false);
        StartCoroutine(startLeaving());
    }

    IEnumerator startLeaving()
    {
        yield return new WaitForSeconds(leavingDelay);
        audioSourceBackground.Play();
        GameEvents.Instance.RaiseCarriageStartsLeaving();
    }
    private void Instance_MainDoorOpened()
    {
        doorClosingTrigger.gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        GameEvents.Instance.CarriageStopped -= Instance_CarriageStopped;
        GameEvents.Instance.CarriageStartStopping -= Instance_CarriageStartStopping;
        GameEvents.Instance.MainDoorOpened -= Instance_MainDoorOpened;
    }
    private void Instance_CarriageStartStopping()
    {
        StartCoroutine(stopping());
    }

    private void Instance_CarriageStopped()
    {
        doorsChangeState(true);
    }
    void doorsChangeState(bool open)
    {
        animationDoorsOpen.Animation.ChangeDirection(open);
        animationDoorsOpen.Play();
        audioSourceEffects.clip = audioClipDoorsOpen;
        audioSourceEffects.Play();
    }
    IEnumerator stopping()
    {
        audioSourceEffects.clip = audioClipStartStopping;
        audioSourceEffects.Play();
        yield return new WaitForSeconds(0.3f);
        audioSourceBackground.Stop();
    }
    private void Update()
    {
        audioSourceBackground.pitch = GameStates.Instance.CarriageSpeedPercentage;
        float pitchCorrection = 1f / audioSourceBackground.pitch;
        audioMixer.SetFloat("PitchShift", pitchCorrection);
    }
}
