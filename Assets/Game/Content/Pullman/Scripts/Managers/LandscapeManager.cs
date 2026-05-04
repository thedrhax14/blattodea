using System.Collections.Generic;
using UnityEngine;

public class LandscapeManager : MonoBehaviour
{
    [SerializeField]
    bool debug;
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
    float tonnelPartSize, stationSize;
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
    float breakingDistance = 0;
    float vagonSpeedCurrent;
    float vagonSpeedMin = 0.35f;
    private void Awake()
    {
        foreach (Transform t in partsSource)
        {
            t.gameObject.SetActive(false);
            partsAvailable.Add(t);
        }
        for (int i = 0; i < activePartsNum; i++)
        {
            getPartAvailable(new Vector3(0, -1, i * tonnelPartSize));
        }
        calcLampIndex();
        trainStation.gameObject.SetActive(false);
        vagonSpeedCurrent = vagonSpeed;
        if (debug)
        {
            vagonSpeedMin = vagonSpeedMin * 10;
        }
    }

    private void Instance_CarriageStartsLeaving()
    {
        partsAvailable.Clear();
        foreach (Transform t in partsSource)
        {
            flushPart(t);
        }
        disablePoint = new Vector3(disablePoint.x, disablePoint.y, (int)trainStation.transform.position.z + stationSize - tonnelPartSize);
        for (int i = 0; i < activePartsNum; i++)
        {
            getPartAvailable(new Vector3(0, -1, (int)trainStation.transform.position.z + stationSize + tonnelPartSize + i * tonnelPartSize));
        }
        partsToStop = 1;
    }

    private void OnEnable()
    {
        GameEvents.Instance.CarriageStartsLeaving += Instance_CarriageStartsLeaving;
        GameEvents.Instance.StopLeverActivated += Instance_StopLeverActivated;
    }
    private void OnDisable()
    {
        GameEvents.Instance.StopLeverActivated -= Instance_StopLeverActivated;
        GameEvents.Instance.CarriageStartsLeaving -= Instance_CarriageStartsLeaving;
    }
    private void Instance_StopLeverActivated()
    {
        trainStation.position = new Vector3(0, -1, partsToStop * tonnelPartSize);
        breakingDistance = Vector3.Distance(carriage.transform.position, stopPoint.position);
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
        if (GameStates.Instance.CarriageStopped && !GameStates.Instance.CarriageLeaving)
        {
            return;
        }
        GameStates.Instance.SyncCarriageSpeedPercentage(vagonSpeedCurrent / vagonSpeed + 0.5f);
        for (int i = 0; i < activeParts.Count; i++)
        {
            var part = activeParts[i];
            part.position = Vector3.MoveTowards(part.position, disablePoint, vagonSpeedCurrent * Time.deltaTime);
            if (i == 0)
            {
                if (part.position == disablePoint)
                {
                    flushPart(part);
                    if (GameStates.Instance.StopLeverActivated && !GameStates.Instance.CarriageLeaving)
                    {
                        if (partsToStop > -1)
                        {
                            partsToStop--;
                        }
                        if (partsToStop == 0)
                        {
                            trainStation.gameObject.SetActive(true);
                        }
                    }
                    if (partsToStop > 0)
                    {
                        getPartAvailable(new Vector3(0, -1, disablePoint.z + activePartsNum * tonnelPartSize));
                        lampEnableIndexCurrent++;
                        if (lampEnableIndexCurrent == lampEnableIndexChoosed)
                        {
                            calcLampIndex();
                        }
                    }
                }
            }
        }
        if (GameStates.Instance.CarriageLeaving)
        {
            if (vagonSpeedCurrent < vagonSpeed)
            {
                vagonSpeedCurrent = Mathf.MoveTowards(vagonSpeedCurrent, vagonSpeed, 0.002f);
            }
            trainStation.position = Vector3.MoveTowards(trainStation.position, trainStation.position + trainStation.forward, vagonSpeedCurrent * Time.deltaTime);
        }
        else
        {
            if (GameStates.Instance.StopLeverActivated)
            {
                trainStation.position = Vector3.MoveTowards(trainStation.position, trainStation.position + trainStation.forward, vagonSpeedCurrent * Time.deltaTime);
                var posDelta = (stopPoint.position - carriage.transform.position).sqrMagnitude;
                if (posDelta <= distanceForTargetStop)
                {
                    carriage.transform.position = stopPoint.position;
                    GameEvents.Instance.RaiseCarriageStopped();
                    vagonSpeedCurrent = 0;
                }
                else if (posDelta <= distanceForTargetStartStopping)
                {
                    if (!GameStates.Instance.CarriageStartsStopping)
                    {
                        GameEvents.Instance.RaiseCarriageStartStopping();
                    }
                }
                float distance = Vector3.Distance(carriage.position, stopPoint.position);
                vagonSpeedCurrent = vagonSpeed * (distance / breakingDistance);
                if (vagonSpeedCurrent < vagonSpeedMin)
                {
                    vagonSpeedCurrent = vagonSpeedMin;
                }
            }
        }
    }
}
