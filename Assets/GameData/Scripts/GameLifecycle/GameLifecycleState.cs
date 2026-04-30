using Blattodea.Core.Models;

public enum GameLifecycleState : byte
{
    Offline = 0,
    InLobby = 1,
    InProgress = 2,
    DoorOpens = 3,
    Concluding = 4,
    GameOver = 5
}

public static class GameLifecycle
{
    public static ModelSubject<GameLifecycleState> State { get; } = new(GameLifecycleState.Offline);

    public static GameLifecycleState Current => State.Model;

    public static bool TrySetState(GameLifecycleState nextState)
    {
        return State.TrySetModel(nextState);
    }

    public static void SetOffline()
    {
        State.SetModel(GameLifecycleState.Offline);
    }
}