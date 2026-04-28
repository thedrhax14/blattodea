using UnityEngine;

public class TraderEffects : MonoBehaviour
{
    [SerializeField]
    ParticleSystem particleSystemSmoke;
    public void MakeSmoke()
    {
        particleSystemSmoke.Play();
    }
}
