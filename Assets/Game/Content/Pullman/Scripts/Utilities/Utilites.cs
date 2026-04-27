using UnityEngine;

public static class Utilites
{
    
    public static void ChangeDirection(this Animation animation, bool forward)
    {
        foreach (AnimationState state in animation)
        {
            if (forward)
            {
                state.speed = 1;
                state.normalizedTime = 0;
            }
            else
            {
                state.speed = -1;
                state.normalizedTime = 1;
            }
        }
    }
}
