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
    }
}
