using UnityEngine;

public class GameStates
{
    static GameStates instance;
    public static GameStates Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new GameStates();
            }
            return instance;
        }
    }
    public bool StopLeverActivated { get; private set; } = false;
    public bool CarriageStopped { get; private set; } = false;
    public bool CarriageLeaving { get; private set; } = false;

    public bool CarriageStartsStopping { get; private set; } = false;
    public bool MainDoorOpened { get; private set; } = false;
    public float CarriageSpeedPercentage { get; private set; } = 0;
    public void SyncCarriageSpeedPercentage(float val)
    {
        CarriageSpeedPercentage = val;
        CarriageSpeedPercentage = Mathf.Clamp(CarriageSpeedPercentage, 0.3f, 1);
    }
    private GameStates()
    {
        GameEvents.Instance.CarriageStartsLeaving += () =>
        {
            CarriageLeaving = true;
        };
        GameEvents.Instance.StopLeverActivated += () =>
        {
            StopLeverActivated = true;
        };
        GameEvents.Instance.CarriageStopped += () =>
        {
            CarriageStopped = true;
        };
        GameEvents.Instance.CarriageStartStopping += () =>
        {
            CarriageStartsStopping = true;
        };
        GameEvents.Instance.MainDoorOpened += () =>
        {
            MainDoorOpened = true;
        };
    }
}
