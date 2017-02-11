using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewindableLooper : Looper {

    // Private state variables
    bool rewinding = false;

    protected override void FixedUpdate()
    {
        if (!rewinding)
            Loop();
        else
            ;
    }

    void Rewind(float rewindInterval)
    {
        loopingTimer -= rewindInterval;

        Vector3 tempVector;
        Quaternion tempQuartenion;
        float tempFloat;

        while (recordedTimes[currentLooperIndex] >= loopingTimer && currentLooperIndex >= 0)
        {
            currentLooperIndex--;

            tempVector = recordedPositions[currentLooperIndex];
            tempQuartenion = recordedRotations[currentLooperIndex];
            tempFloat = recordedTimes[currentLooperIndex];

            gameObject.transform.position = tempVector;
            gameObject.transform.localRotation = tempQuartenion;
            //Debug.Log(originalRecordedTimes.Count);
        }
    }
}
