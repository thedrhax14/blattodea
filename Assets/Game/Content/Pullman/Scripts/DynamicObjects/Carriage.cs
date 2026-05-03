using System.Collections;
using UnityEngine;

public class Carriage : MonoBehaviour
{
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
        if (doorClosingTrigger != null)
        {
            doorClosingTrigger.gameObject.SetActive(false);
            doorClosingTrigger.OnTriggerEnterAction += DoorClosingTrigger_OnTriggerEnterAction;
        }
    }

    private void OnDisable()
    {
        GameEvents.Instance.CarriageStopped -= Instance_CarriageStopped;
        GameEvents.Instance.CarriageStartStopping -= Instance_CarriageStartStopping;
        GameEvents.Instance.MainDoorOpened -= Instance_MainDoorOpened;
        if (doorClosingTrigger != null)
        {
            doorClosingTrigger.OnTriggerEnterAction -= DoorClosingTrigger_OnTriggerEnterAction;
        }
    }

    private void DoorClosingTrigger_OnTriggerEnterAction(Collider obj)
    {
        doorsChangeState(false);
        if (doorClosingTrigger != null)
        {
            doorClosingTrigger.gameObject.SetActive(false);
        }
    }

    private void Instance_MainDoorOpened()
    {
        if (doorClosingTrigger != null)
        {
            doorClosingTrigger.gameObject.SetActive(true);
        }
    }

    private void Instance_CarriageStartStopping()
    {
        StartCoroutine(stopping());
    }

    private void Instance_CarriageStopped()
    {
        doorsChangeState(true);
        audioSourceEffects.clip = audioClipDoorsOpen;
        audioSourceEffects.Play();
    }

    void doorsChangeState(bool open)
    {
        animationDoorsOpen.Animation.ChangeDirection(open);
        animationDoorsOpen.Play();
    }

    IEnumerator stopping()
    {
        audioSourceEffects.clip = audioClipStartStopping;
        audioSourceEffects.Play();
        yield return new WaitForSeconds(0.3f);
        audioSourceBackground.Stop();
    }
}
