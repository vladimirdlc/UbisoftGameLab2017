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
    float loopingTimer;

    // Playback data collections
    Queue<Vector3> originalRecordedPositions;
    Queue<float> originalRecordedTimes;
    Queue<Quaternion> originalRecordedRotations;
    Queue<Vector3> recordedPositions;
    Queue<Quaternion> recordedRotations;
    Queue<float> recordedTimes;

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
            if (recordedPositions.Count > 0)
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

    public void StartLooping(Queue<Vector3> recordedPositions, Queue<Quaternion> recordedRotations, Queue<float> recordedTimes)
    {
        this.recordedPositions = recordedPositions;
        this.recordedTimes = recordedTimes;
        this.recordedRotations = recordedRotations;

        originalRecordedPositions = new Queue<Vector3>(recordedPositions);
        originalRecordedRotations = new Queue<Quaternion>(recordedRotations);
        originalRecordedTimes = new Queue<float>(recordedTimes);

        loopingTimer = 0;
        looping = true;
    }

    void NextFrameAction()
    {
        Vector3 tempVector;
        Quaternion tempQuartenion;
        float tempFloat;

        do
        {
            tempVector = recordedPositions.Dequeue();
            tempQuartenion = recordedRotations.Dequeue();
            tempFloat = recordedTimes.Dequeue();

            gameObject.transform.position = tempVector;
            gameObject.transform.localRotation = tempQuartenion;
            //Debug.Log(originalRecordedTimes.Count);
        }
        while (loopingTimer > tempFloat && recordedPositions.Count > 0);
    }

    private void Reloop()
    {
        recordedPositions = new Queue<Vector3>(originalRecordedPositions);
        recordedRotations = new Queue<Quaternion>(originalRecordedRotations);
        recordedTimes = new Queue<float>(originalRecordedTimes);

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