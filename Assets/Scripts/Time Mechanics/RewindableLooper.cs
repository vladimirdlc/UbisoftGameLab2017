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
        if (rewindInterval <= 0)
            return;

        loopingTimer -= rewindInterval;

        Vector3 tempVector;
        Quaternion tempQuartenion;
        float tempFloat;

        //Debug.Log(currentLooperIndex);

        while (recordedTimes[currentLooperIndex] >= loopingTimer && currentLooperIndex > 0)
        {
            currentLooperIndex--;

            tempVector = recordedPositions[currentLooperIndex];
            tempQuartenion = recordedRotations[currentLooperIndex];
            tempFloat = recordedTimes[currentLooperIndex];

            gameObject.transform.position = tempVector;
            gameObject.transform.localRotation = tempQuartenion;
        }
    }

    public override void Reloop()
    {
        //Debug.Log("Child reloop");

        if (!rewinding)
            base.Reloop();
        else
        {
#if CUSTOM_DEBUG_VERBOSE
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
