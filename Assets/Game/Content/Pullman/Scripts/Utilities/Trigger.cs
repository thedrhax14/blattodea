using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger : MonoBehaviour
{
    [SerializeField]
    private new Collider collider;
    public event Action<Collider> OnTriggerEnterAction = delegate { };
    public event Action<Collider> OnTriggerExitAction = delegate { };
    private void Awake()
    {
        collider.isTrigger = true;
    }
    public void Flush()
    {
        OnTriggerEnterAction = delegate { };
        OnTriggerExitAction = delegate { };
    }
    private void OnTriggerEnter(Collider other)
    {
        OnTriggerEnterAction(other);
    }
    private void OnTriggerExit(Collider other)
    {
        OnTriggerExitAction(other);
    }
}
