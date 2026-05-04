using UnityEngine;
using System;
using System.Collections;
public interface IBarnacleAttackable
{
    Vector3 CapturePointPosition { get; }
    void Capture(Transform barnacle);
    void Release();
}
public class Barnacle : MonoBehaviour
{
    [SerializeField]
    Transform hideTargetPoint;
    [SerializeField]
    private float speedAttack, speedHide;
    [SerializeField]
    Transform tongue;
    [SerializeField]
    AudioSource audioSource;
    [SerializeField]
    string tagToAttack;
    [SerializeField]
    Trigger triggerAttack;
    [SerializeField]
    SimpleAnimation animationAttack;
    IBarnacleAttackable victimCurrent = null;
    private void Awake()
    {
        triggerAttack.OnTriggerEnterAction += TriggerAttack_OnTriggerEnterAction;
    }
    private void TriggerAttack_OnTriggerEnterAction(Collider obj)
    {
        if (obj.CompareTag(tagToAttack))
        {
            if (obj.TryGetComponent(out victimCurrent))
            {
                StartCoroutine(movingToAttack());
            }
            else
            {
                Debug.LogError("Object " + obj.name + " has tag " + obj.tag + " but does not implement IBarnacleAttackable", this);
            }
        }
        else
        {
            Debug.LogError("Object " + obj.name + " does not have the correct tag to be attacked by barnacle. Expected: " + tagToAttack + ", actual: " + obj.tag, this);
        }
    }
    IEnumerator movingToAttack()
    {
        yield return new WaitForSeconds(2);
        animationAttack.Play();
        //for now
        yield break;
        //
        while (true)
        {
            tongue.position = Vector3.MoveTowards(tongue.position, victimCurrent.CapturePointPosition, speedAttack);
            if (tongue.position == victimCurrent.CapturePointPosition)
            {
                captureVictim();
                yield return new WaitForSeconds(0.05f);
                StartCoroutine(movingToHide());
                yield break;
            }
            yield return null;
        }
    }
    IEnumerator movingToHide()
    {
        while (true)
        {
            tongue.position = Vector3.MoveTowards(tongue.position, hideTargetPoint.position, speedHide);
            if (tongue.position == hideTargetPoint.position)
            {
                yield break;
            }
            yield return null;
        }
    }
    void captureVictim()
    {
        //animationAttack.Play();
        victimCurrent.Capture(tongue);
    }
    void PlaySFX()
    {
        audioSource.Play();
    }
}
