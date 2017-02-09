using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Looper : MonoBehaviour
{
    public GameObject objectToInstantiate;

    // Object variables
    GameObject loopingObject;

    // State variables
    bool looping = false;

    // Numeric variables
    float loopingTimer;

    // Playback data collections
    Queue<Vector3> originalRecordedPositions;
    Queue<float> originalRecordedTimes;
    Queue<Vector3> recordedPositions;
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
        if(looping)
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

    public void StartLooping(Queue<Vector3> recordedPositions, Queue<float> recordedTimes)
    {
        this.recordedPositions = recordedPositions;
        this.recordedTimes = recordedTimes;

        originalRecordedPositions = new Queue<Vector3>(recordedPositions);
        originalRecordedTimes = new Queue<float>(recordedTimes);

        // NOTE TO SELF: CAST CONCERNS?
        loopingObject = (GameObject) Instantiate(objectToInstantiate);

        loopingTimer = 0;
        looping = true;
    }

    void NextFrameAction()
    {
        Vector3 tempVector;
        float tempFloat;

        do
        {
            tempVector = recordedPositions.Dequeue();
            tempFloat = recordedTimes.Dequeue();

            loopingObject.transform.position = tempVector;
            //Debug.Log(originalRecordedTimes.Count);
        }
        while (loopingTimer > tempFloat && recordedPositions.Count > 0);
    }

    private void Reloop()
    {
        recordedPositions = new Queue<Vector3>(originalRecordedPositions);
        recordedTimes = new Queue<float>(originalRecordedTimes);

        loopingTimer = 0;
    }
}
