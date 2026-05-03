using TMPro;
using UnityEngine;

public class StationDoorPanel : MonoBehaviour
{
    [SerializeField]
    TextMeshPro textMesh;
    [SerializeField]
    AudioClip audioClipLeverBreaking;
    [SerializeField]
    ParticleSystem particlesMetalImpact;
    [SerializeField]
    StationDoorActivator brokenLever;
    [SerializeField]
    StationDoorPanelLever lever;
    AudioSource audioSource;
    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        brokenLever.gameObject.SetActive(false);
        textMesh.gameObject.SetActive(false);
        brokenLever.SetupInteraction(() =>
        {
            textMesh.gameObject.SetActive(true);
            textMesh.text = "Success";
        }, true);
    }

    private void OnEnable()
    {
        GameEvents.Instance.MainDoorPanelLeverPulled += OnMainDoorPanelLeverPulled;
    }

    private void OnDisable()
    {
        GameEvents.Instance.MainDoorPanelLeverPulled -= OnMainDoorPanelLeverPulled;
    }

    private void OnMainDoorPanelLeverPulled()
    {
        brokenLever.gameObject.SetActive(true);
        particlesMetalImpact.Play();
        audioSource.clip = audioClipLeverBreaking;
        audioSource.Play();
    }
}
