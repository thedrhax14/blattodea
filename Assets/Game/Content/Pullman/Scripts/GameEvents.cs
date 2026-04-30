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
    public event Action CarriageStartStopping = delegate { };
    public void RaiseCarriageStartStopping()
    {
        CarriageStartStopping();
    }
    public event Action TrainStopCutsceneStarted = delegate { };
    public void RaiseTrainStopCutsceneStarted()
    {
        TrainStopCutsceneStarted();
    }
    public event Action TrainStopCutsceneEnded = delegate { };
    public void RaiseTrainStopCutsceneEnded()
    {
        TrainStopCutsceneEnded();
    }
    public event Action MainDoorOpeningStarted = delegate { };
    public void RaiseMainDoorOpeningStarted()
    {
        MainDoorOpeningStarted();
    }
    public event Action MainDoorOpened = delegate { };
    public void RaiseMainDoorOpened()
    {
        MainDoorOpened();
    }
}
