using System.Collections.Generic;
using UnityEngine;

public class LandscapeManager : MonoBehaviour
{
    [Tooltip("—корость вагона")]
    [SerializeField]
    float vagonSpeed = 0.025f;
    [Tooltip(" ак часто будет включена лампа в туннеле")]
    [SerializeField]
    int lampEnableIndexMin, lampEnableIndexMax;
    [Tooltip("—колько частей туннел€ одновременно активны")]
    [SerializeField]
    int activePartsNum = 3;
    [Tooltip("“очка смены частей туннел€")]
    [SerializeField]
    Vector3 disablePoint;
    [Tooltip("¬се используемые части туннел€")]
    [SerializeField]
    List<Transform> partsSource;
    [Tooltip("Z размер части туннел€ ")]
    [SerializeField]
    float partSize;
    [SerializeField]
    Transform trainStation;
    [SerializeField]
    Transform stopPoint;
    [SerializeField]
    Transform carriage;
    [SerializeField]
    [Tooltip(" вадрат дистанции до конечной точки когда вагон должен начать тормозить")]
    float distanceForTargetStartStopping;
    [SerializeField]
    [Tooltip(" вадрат дистанции  до конечной точки погрешности, когда позици€ вагона приравниваетс€ конечной точке")]
    float distanceForTargetStop;
    [SerializeField]
    [Tooltip("—колько еще частей туннел€ проедет вагон перед тем как по€витс€ станци€")]
    int partsToStop = 4;
    List<Transform> activeParts = new List<Transform>();
    List<Transform> partsAvailable = new List<Transform>();
    int lampEnableIndexChoosed = 0, lampEnableIndexCurrent = 0;
    float brakingDistance = 0;
    Vector3 carriageVelocity;
    private void Awake()
    {
        foreach (Transform t in partsSource)
        {
            t.gameObject.SetActive(false);
            partsAvailable.Add(t);
        }
        for (int i = 0; i < activePartsNum; i++)
        {
            getPartAvailable(new Vector3(0, -1, i * partSize));
        }
        calcLampIndex();
        trainStation.gameObject.SetActive(false);
    }
    private void OnEnable()
    {
        GameEvents.Instance.StopLeverActivated += Instance_StopLeverActivated;
    }
    private void OnDisable()
    {
        GameEvents.Instance.StopLeverActivated -= Instance_StopLeverActivated;
    }
    private void Instance_StopLeverActivated()
    {
    }

    void calcLampIndex()
    {
        lampEnableIndexCurrent = 0;
        lampEnableIndexChoosed = Random.Range(lampEnableIndexMin, lampEnableIndexMax);
    }
    Transform getPartAvailable(Vector3 pos)
    {
        var part = partsAvailable[Random.Range(0, partsAvailable.Count)];
        partsAvailable.Remove(part);
        activeParts.Add(part);
        part.gameObject.SetActive(true);
        part.position = pos;
        return part;
    }
    void flushPart(Transform part)
    {
        part.gameObject.SetActive(false);
        activeParts.Remove(part);
        partsAvailable.Add(part);
    }
    private void Update()
    {
        if (GameStates.Instance.CarriageStopped)
        {
            return;
        }
        if (trainStation.gameObject.activeSelf)
        {
            carriage.transform.position = Vector3.SmoothDamp(carriage.transform.position, stopPoint.position, ref carriageVelocity, 4, vagonSpeed);
            var posDelta = (carriage.transform.position - stopPoint.position).sqrMagnitude;
            if (posDelta <= distanceForTargetStop)
            {
                carriage.transform.position = stopPoint.position;
                GameEvents.Instance.RaiseCarriageStopped();
            }
            else if (posDelta <= distanceForTargetStartStopping)
            {
                if (!GameStates.Instance.CarriageStartsStopping)
                {
                    GameEvents.Instance.RaiseCarriageStartStopping();
                }
            }
        }
        else
        {
            for (int i = 0; i < activeParts.Count; i++)
            {
                var part = activeParts[i];
                part.position = Vector3.MoveTowards(part.position, disablePoint, vagonSpeed * Time.deltaTime);
                if (i == 0)
                {
                    if (part.position == disablePoint)
                    {
                        flushPart(part);
                        getPartAvailable(new Vector3(0, -1, (activeParts.Count - 1) * partSize));
                        lampEnableIndexCurrent++;
                        if (lampEnableIndexCurrent == lampEnableIndexChoosed)
                        {
                            calcLampIndex();
                        }
                        if (GameStates.Instance.StopLeverActivated)
                        {
                            if (partsToStop > -1)
                            {
                                partsToStop--;
                            }
                            if (partsToStop == 0)
                            {
                                trainStation.gameObject.SetActive(true);
                                trainStation.position = new Vector3(0, -1, (activeParts.Count - 1) * partSize - partSize);
                                Vector3 direction = (stopPoint.position - transform.position).normalized;
                                direction.y = 0;
                                carriageVelocity = direction * vagonSpeed;
                            }
                        }
                    }
                }
            }
        }
    }
}
