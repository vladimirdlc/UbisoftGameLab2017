using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Looper : MonoBehaviour
{
    public bool hasTrail;

    // Private State variables
    bool looping = false;
    bool trailAttatched = true;            // Once the looping happens once, the trail needs to be unparented from the looper object

    // Numeric variables
    int currentLooperIndex;                // The index number of the current position in the recorded collections
    float loopingTimer;

    // Playback data collections
    List<Vector3> recordedPositions;
    List<Quaternion> recordedRotations;
    List<float> recordedTimes;

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void FixedUpdate()
    {
        if (looping)
        {
            // If the timer isn't past the last indexed time, or if the index isn't out of bounds
            if (loopingTimer <= recordedTimes[recordedTimes.Count - 1] && recordedPositions.Count > currentLooperIndex)
            {
                NextFrameAction();
                loopingTimer += Time.deltaTime;
            }
            else
            {
                Reloop();
            }
        }
    }

    public void StartLooping(List<Vector3> recordedPositions, List<Quaternion> recordedRotations, List<float> recordedTimes)
    {
        this.recordedPositions = recordedPositions;
        this.recordedTimes = recordedTimes;
        this.recordedRotations = recordedRotations;

        loopingTimer = 0;
        currentLooperIndex = 0;
        looping = true;
    }

    void NextFrameAction()
    {
        Vector3 tempVector;
        Quaternion tempQuartenion;
        float tempFloat;

        do
        {
            tempVector = recordedPositions[currentLooperIndex];
            tempQuartenion = recordedRotations[currentLooperIndex];
            tempFloat = recordedTimes[currentLooperIndex];

            gameObject.transform.position = tempVector;
            gameObject.transform.localRotation = tempQuartenion;
            //Debug.Log(originalRecordedTimes.Count);

            currentLooperIndex++;
        }
        while (loopingTimer > tempFloat && recordedPositions.Count > currentLooperIndex);
    }

    private void Reloop()
    {
        currentLooperIndex = 0;

        if (hasTrail)
            if (trailAttatched)
            {
                // Unparent trail and toggle flag
                GameObject trail = transform.GetComponentInChildren<TrailRenderer>().gameObject;
                trail.transform.parent = null;

                trailAttatched = false;
            }

        loopingTimer = 0;
    }
}