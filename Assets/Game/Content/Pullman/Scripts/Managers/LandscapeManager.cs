using System.Collections.Generic;
using UnityEngine;

public class LandscapeManager : MonoBehaviour
{
    [Tooltip("�������� ������")]
    [SerializeField]
    float vagonSpeed = 0.025f;
    [Tooltip("��� ����� ����� �������� ����� � �������")]
    [SerializeField]
    int lampEnableIndexMin, lampEnableIndexMax;
    [Tooltip("������� ������ ������� ������������ �������")]
    [SerializeField]
    int activePartsNum = 3;
    [Tooltip("����� ����� ������ �������")]
    [SerializeField]
    Vector3 disablePoint;
    [Tooltip("��� ������������ ����� �������")]
    [SerializeField]
    List<Transform> partsSource;
    [Tooltip("Z ������ ����� ������� ")]
    [SerializeField]
    float partSize;
    [SerializeField]
    Transform trainStation;
    [SerializeField]
    Transform stopPoint;
    [SerializeField]
    Transform carriage;
    [SerializeField]
    [Tooltip("������� ��������� �� �������� ����� ����� ����� ������ ������ ���������")]
    float distanceForTargetStartStopping;
    [SerializeField]
    [Tooltip("������� ���������  �� �������� ����� �����������, ����� ������� ������ �������������� �������� �����")]
    float distanceForTargetStop;
    [SerializeField]
    [Tooltip("������� ��� ������ ������� ������� ����� ����� ��� ��� �������� �������")]
    int partsToStop = 4;
    List<Transform> activeParts = new List<Transform>();
    List<Transform> partsAvailable = new List<Transform>();
    int lampEnableIndexChoosed = 0, lampEnableIndexCurrent = 0;
    float brakingDistance = 0;
    float vagonSpeedCurrent;
    const float vagonSpeedMin = 0.35f;

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
        vagonSpeedCurrent = vagonSpeed;
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
        trainStation.position = new Vector3(0, -1, partsToStop * partSize);
        brakingDistance = Vector3.Distance(carriage.transform.position, stopPoint.position);
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

        for (int i = 0; i < activeParts.Count; i++)
        {
            var part = activeParts[i];
            part.position = Vector3.MoveTowards(part.position, disablePoint, vagonSpeedCurrent * Time.deltaTime);
            if (i == 0 && part.position == disablePoint)
            {
                flushPart(part);
                if (GameStates.Instance.StopLeverActivated)
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
                    getPartAvailable(new Vector3(0, -1, (activeParts.Count - 1) * partSize));
                    lampEnableIndexCurrent++;
                    if (lampEnableIndexCurrent == lampEnableIndexChoosed)
                    {
                        calcLampIndex();
                    }
                }
            }
        }

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

            if (brakingDistance > 0)
            {
                float distance = Vector3.Distance(carriage.position, stopPoint.position);
                vagonSpeedCurrent = vagonSpeed * (distance / brakingDistance);
                if (vagonSpeedCurrent < vagonSpeedMin)
                {
                    vagonSpeedCurrent = vagonSpeedMin;
                }
            }
        }
    }
}
