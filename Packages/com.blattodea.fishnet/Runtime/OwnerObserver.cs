using FishNet.Connection;
using FishNet.Object;
using UnityEngine.Events;

namespace Blattodea.FishNet.Interactions
{
    public class OwnerObserver : NetworkBehaviour
    {
        public UnityEvent OnGainedOwnership;
        public UnityEvent OnLostOwnership;

        public override void OnOwnershipClient(NetworkConnection prevOwner)
        {
            base.OnOwnershipClient(prevOwner);
            if (IsOwner)
            {
                OnGainedOwnership.Invoke();
            }
            else
            {
                OnLostOwnership.Invoke();
            }
        }
    }
}