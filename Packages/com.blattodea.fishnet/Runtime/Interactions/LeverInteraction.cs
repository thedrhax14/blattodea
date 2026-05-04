using FishNet.Object;
using KINEMATION.MotionWarping.Runtime.Core;
using KINEMATION.MotionWarping.Runtime.Utility;
using UnityEngine;

namespace Blattodea.FishNet.Interactions
{
    [RequireComponent(typeof(NetworkObject))]
    public class LeverInteraction : NetworkBehaviour, ICharacterInteraction
    {
        public Transform ikHandTarget;
        public Transform playerTarget, respawnPoint;
        [Range(0, 180f)] public float maxAllowedAngle = 40f;
        public MotionWarpingAsset warpingAsset;

        private Animator _animator;

        private void Start()
        {
            _animator = GetComponent<Animator>();
        }

        public Transform GetLeftHandTarget()
        {
            return ikHandTarget;
        }

        public Transform GetRightHandTarget()
        {
            return null;
        }

        public void StartInteraction(GameObject player)
        {
            if (!CanStartInteraction(player))
            {
                return;
            }

            PlayInteraction(player);

            NetworkObject playerNetworkObject = player.GetComponent<NetworkObject>();
            if (playerNetworkObject == null)
            {
                Debug.LogError("Lever interaction requires the player to have a NetworkObject.");
                return;
            }

            ReplicateInteraction(playerNetworkObject);
        }

        protected virtual bool CanStartInteraction(GameObject player)
        {
            if (player == null)
            {
                Debug.LogError("Lever interaction requires a valid player GameObject.");
                return false;
            }

            Vector3 targetDirection = playerTarget.forward;
            Vector3 playerDirection = player.transform.forward;

            if (Vector3.Angle(targetDirection, playerDirection) > maxAllowedAngle)
            {
                Debug.LogError("Player is not facing the lever within the allowed angle. Interaction aborted.");
                return false;
            }

            return true;
        }

        protected virtual void PlayInteraction(GameObject player)
        {
            MotionWarpingComponent warpingComponent = player.GetComponent<MotionWarpingComponent>();
            if (warpingComponent != null)
            {
                WarpPoint[] warpPoints = new[]
                {
                    new WarpPoint(playerTarget)
                };
                if(respawnPoint != null)
                {
                    warpPoints = new[]
                    {
                        new WarpPoint(playerTarget),
                        new WarpPoint(respawnPoint)
                    };
                }
                warpingComponent.Play(warpingAsset, warpPoints);
            }

            if (_animator != null)
            {
                _animator.Play("Lever", -1, 0f);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        protected virtual void ReplicateInteraction(NetworkObject playerNetworkObject)
        {
            BroadcastInteraction(playerNetworkObject);
        }

        [ObserversRpc(ExcludeOwner = true)]
        protected virtual void BroadcastInteraction(NetworkObject playerNetworkObject)
        {
            if (playerNetworkObject == null)
            {
                return;
            }

            PlayInteraction(playerNetworkObject.gameObject);
        }
    }
}