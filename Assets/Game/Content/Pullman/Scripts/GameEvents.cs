using System;
using UnityEngine;

public class GameEvents
{
    static GameEvents instance;
    public static GameEvents Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new GameEvents();
            }
            return instance;
        }
    }
    public event Action StopLeverActivated = delegate { };
    public void ActivateStopLever()
    {
        StopLeverActivated();
    }
    public event Action CarriageStopped = delegate { };
    public void RaiseCarriageStopped()
    {
        CarriageStopped();
    }

}
