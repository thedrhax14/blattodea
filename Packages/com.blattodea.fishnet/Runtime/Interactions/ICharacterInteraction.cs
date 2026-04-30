using UnityEngine;

namespace Blattodea.FishNet.Interactions
{
    public interface ICharacterInteraction
    {
        Transform GetLeftHandTarget();
        Transform GetRightHandTarget();
        void StartInteraction(GameObject player);
    }
}