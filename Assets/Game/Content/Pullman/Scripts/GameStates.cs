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
    public bool CarriageStartsStopping { get; private set; } = false;
    public bool MainDoorStartsOpening { get; private set; } = false;
    public bool MainDoorOpened { get; private set; } = false;
    private GameStates()
    {
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
        GameEvents.Instance.MainDoorOpeningStarted += () =>
        {
            MainDoorStartsOpening = true;
        };
        GameEvents.Instance.MainDoorOpened += () =>
        {
            MainDoorOpened = true;
        };
    }
}
