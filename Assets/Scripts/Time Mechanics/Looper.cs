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
            NextFrameAction();
            loopingTimer += Time.deltaTime;
        }
    }

    public void StartLooping(Queue<Vector3> recordedPositions, Queue<float> recordedTimes)
    {
        this.recordedPositions = recordedPositions;
        this.recordedTimes = recordedTimes;

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
        }
        while (loopingTimer > tempFloat && recordedPositions.Count > 0);
    }
}
