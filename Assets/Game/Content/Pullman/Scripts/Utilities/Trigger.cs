using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger : MonoBehaviour
{
    [SerializeField]
    bool ProvideClosestPoint;
    [SerializeField]
    private new Collider collider;
    public event Action<Collider> OnTriggerEnterAction = delegate { };
    public event Action<Collider> OnTriggerExitAction = delegate { };
    public Vector3 ClosestCollisionPoint { get; private set; } = Vector3.zero;
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
        if (ProvideClosestPoint)
        {
            ClosestCollisionPoint = other.ClosestPoint(transform.position);
        }
        OnTriggerEnterAction(other);
    }
    private void OnTriggerExit(Collider other)
    {
        OnTriggerExitAction(other);
    }
}
