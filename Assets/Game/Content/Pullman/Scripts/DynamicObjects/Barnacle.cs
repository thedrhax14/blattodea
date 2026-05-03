using UnityEngine;

public class Barnacle : MonoBehaviour
{
    [SerializeField]
    AudioSource audioSource;
    [SerializeField]
    string tagToAttack;
    [SerializeField]
    Trigger triggerAttack;
    [SerializeField]
    SimpleAnimation animationAttack;
    private void Awake()
    {
        triggerAttack.OnTriggerEnterAction += TriggerAttack_OnTriggerEnterAction;
    }

    private void TriggerAttack_OnTriggerEnterAction(Collider obj)
    {
        if (obj.CompareTag(tagToAttack))
        {
            attack();
        }
    }
    void attack()
    {
        audioSource.Play();
        animationAttack.Play();
    }
}
