using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewindableLooper : Looper
{
    // Private state variables
    bool rewinding = false;

    protected override void LateUpdate()
    {
        if (!rewinding)
            Loop();
    }

    public void LockUnlockRewind(bool locked)
    {
        rewinding = locked;
    }

    public void Rewind(float rewindInterval)
    {
        try
        {
            if (rewindInterval <= 0)
                return;

            loopingTimer -= rewindInterval;

            if (currentLooperIndex == 0)
            {
                gameObject.transform.position = recordedPositions[currentLooperIndex];
                gameObject.transform.localRotation = recordedRotations[currentLooperIndex];
                return;
            }

            LerpState(currentLooperIndex - 1, currentLooperIndex);

            if (recordedTimes[currentLooperIndex] >= loopingTimer && currentLooperIndex > 0)
                currentLooperIndex--;
        }
        catch(System.ArgumentOutOfRangeException e)
        {
#if DEBUG_VERBOSE
            Debug.Log("Index out of bounds for clone " + gameObject + " during Rewind method of RewindableLooper.cs, current index at " + currentLooperIndex + " with a collection size of " + recordedPositions.Count);
#endif
        }
    }

    public override void Reloop()
    {
        //Debug.Log("Child reloop");

        if (!rewinding)
            base.Reloop();
        else
        {
#if DEBUG_VERBOSE
            Debug.Log("Can't reloop object " + this.name + " because it is rewinding.");
#endif
        }
    }

    public override void TimeParadox()
    {
        // Ignore time paradoxes when rewinding
        if (rewinding)
            return;

        base.TimeParadox();
    }
}
