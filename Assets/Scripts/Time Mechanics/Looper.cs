using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Looper : MonoBehaviour
{
    public bool hasTrail;

    // Private State variables
    protected bool looping = false;
    bool trailAttatched = true;            // Once the looping happens once, the trail needs to be unparented from the looper object

    // Numeric variables
    protected int currentLooperIndex;      // The index number of the current position in the recorded collections
    protected float loopingTimer;

    // Playback data collections
    protected List<Vector3> recordedPositions;
    protected List<Quaternion> recordedRotations;
    protected List<float> recordedTimes;

#if DEBUG_VERBOSE
    private bool debugFlag = false;
#endif

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    protected virtual void LateUpdate()
    {
        Loop();
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

    protected void Loop()
    {

#if DEBUG_VERBOSE
        if (debugFlag)
            Debug.Log(debugFlag);
#endif

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
                // Stop looping
                looping = false;
            }
        }
    }

    protected void NextFrameAction()
    {
        Vector3 tempVector;
        Quaternion tempQuartenion;

        float tempFloat = recordedTimes[currentLooperIndex];

        while (loopingTimer >= tempFloat && recordedPositions.Count > currentLooperIndex)
        {
            tempVector = recordedPositions[currentLooperIndex];
            tempQuartenion = recordedRotations[currentLooperIndex];
            tempFloat = recordedTimes[currentLooperIndex];

            gameObject.transform.position = tempVector;
            gameObject.transform.localRotation = tempQuartenion;
            //Debug.Log(originalRecordedTimes.Count);

            currentLooperIndex++;
        }
    }

    public virtual void Reloop()
    {
        if (!looping)
        {
            looping = true;

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

    public virtual void TimeParadox()
    {
        #region debug
#if DEBUG_VERBOSE
        Debug.Log("Before paradox");
        //CustomDebugTools.PrintList<float>(recordedTimes);
        Debug.Log("Current index at " + currentLooperIndex);
        Debug.Log("State collection count at " + recordedPositions.Count);
        Debug.Log("First element in collection " + recordedPositions[0]);
        Debug.Log("Last element in collection " + recordedPositions[recordedTimes.Count - 1]);
#endif
        #endregion

        recordedPositions = CustomCollectionManipulation.RemoveElementsAfterIndex(recordedPositions, currentLooperIndex);
        recordedRotations = CustomCollectionManipulation.RemoveElementsAfterIndex(recordedRotations, currentLooperIndex);
        recordedTimes = CustomCollectionManipulation.RemoveElementsAfterIndex(recordedTimes, currentLooperIndex);

        #region debug
#if DEBUG_VERBOSE
        Debug.Log("After paradox");
        //CustomDebugTools.PrintList<float>(recordedTimes);
        Debug.Log("Current index at " + currentLooperIndex);
        Debug.Log("State collection count at " + recordedPositions.Count);
        Debug.Log("First element in collection " + recordedPositions[0]);
        Debug.Log("Last element in collection " + recordedPositions[recordedTimes.Count - 1]);

        debugFlag = true;
#endif
        #endregion
    }
}