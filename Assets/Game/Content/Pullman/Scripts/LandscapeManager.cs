using System.Collections.Generic;
using UnityEngine;

public class LandscapeManager : MonoBehaviour
{
    [Tooltip("Скорость вагона")]
    [SerializeField]
    float vagonSpeed = 0.025f;
    [Tooltip("Как часто будет включена лампа в туннеле")]
    [SerializeField]
    int lampEnableIndexMin, lampEnableIndexMax;
    [Tooltip("Сколько частей туннеля одновременно активны")]
    [SerializeField]
    int activePartsNum = 3;
    [Tooltip("Точка смены частей туннеля")]
    [SerializeField]
    Vector3 disablePoint;
    [Tooltip("Все используемые части туннеля")]
    [SerializeField]
    List<Transform> partsSource;
    [Tooltip("Z размер части туннеля ")]
    [SerializeField]
    float partSize;
    List<Transform> activeParts = new List<Transform>();
    List<Transform> partsAvailable = new List<Transform>();
    int lampEnableIndexChoosed = 0, lampEnableIndexCurrent = 0;
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
        for (int i = 0; i < activeParts.Count; i++)
        {
            var part = activeParts[i];
            part.position = Vector3.MoveTowards(part.position, disablePoint, vagonSpeed);
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
                }
            }
        }
    }
}
