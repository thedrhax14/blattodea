using FishNet.Object;
using UnityEngine;

namespace Blattodea.FishNet.Triggers
{
    [RequireComponent(typeof(Collider))]
    public class DestroyNetworkObjectOnTrigger : NetworkBehaviour
    {
        [SerializeField]
        private bool destroyRootNetworkObject = true;

        [SerializeField]
        private bool disableAfterTrigger;

        private void OnTriggerEnter(Collider other)
        {
            if(enabled == false)
            {
                Debug.LogWarning("Trigger is disabled, ignoring trigger event.", this);
                return;
            }
            if (!IsServerStarted)
            {
                return;
            }

            NetworkObject networkObject = destroyRootNetworkObject
                ? other.GetComponentInParent<NetworkObject>()
                : other.GetComponent<NetworkObject>();

            if (networkObject == null)
            {
                return;
            }

            if (networkObject == NetworkObject)
            {
                return;
            }

            Despawn(networkObject, DespawnType.Destroy);

            if (disableAfterTrigger)
            {
                enabled = false;
            }
        }
    }
}