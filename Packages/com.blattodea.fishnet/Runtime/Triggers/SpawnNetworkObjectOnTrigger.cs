using FishNet.Object;
using UnityEngine;

namespace Blattodea.FishNet.Triggers
{
    [RequireComponent(typeof(Collider))]
    public class SpawnNetworkObjectOnTrigger : NetworkBehaviour
    {
        [SerializeField]
        private NetworkObject prefab;

        [SerializeField]
        private Transform spawnPoint;

        [SerializeField]
        private bool disableAfterTrigger = true;

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

            if (prefab == null)
            {
                Debug.LogError("Spawn trigger requires a NetworkObject prefab.", this);
                return;
            }

            Vector3 spawnPosition = spawnPoint != null ? spawnPoint.position : transform.position;
            Quaternion spawnRotation = spawnPoint != null ? spawnPoint.rotation : transform.rotation;

            NetworkObject instance = Instantiate(prefab, spawnPosition, spawnRotation);
            Spawn(instance);

            if (disableAfterTrigger)
            {
                enabled = false;
            }
        }
    }
}