using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Recorder : MonoBehaviour {

    public Transform recordedTransform;

    // Recording variables
    float recordingTimer;

    // Recording data collection
    Stack<Vector3> recordedPositions;
    Stack<float> recordedTimes;

    // State variables
    bool recording = false;

	// Use this for initialization
	void Start ()
    {
        // Setup recording data collection
        recordedPositions = new Stack<Vector3>();
        recordedTimes = new Stack<float>();
	}
	
	// Update is called once per frame
	void Update () {

        // DELETEME
        if (Input.GetButtonDown("Test Button"))
            if (!recording)
                StartRecording();
        else
            {
               
            }
	}

    private void LateUpdate()
    {
        if(recording)
        {
            RecordFrame();
        }
    }

    private void StartRecording()
    {
        recordingTimer = 0;

        recording = true;
    }

    private void RecordFrame()
    {
        recordingTimer += Time.deltaTime;

        recordedPositions.Push(recordedTransform.position);
        recordedTimes.Push(recordingTimer);
    }
}
