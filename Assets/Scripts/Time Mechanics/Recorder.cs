using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Recorder : MonoBehaviour {

    public Transform recordedTransform;

    // Recording variables
    float recordingTimer;

    // Recording data collection
    Queue<Vector3> recordedPositions;
    Queue<float> recordedTimes;

    // State variables
    bool recording = false;

	// Use this for initialization
	void Start ()
    {
        // Setup recording data collection
        recordedPositions = new Queue<Vector3>();
        recordedTimes = new Queue<float>();
	}
	
	// Update is called once per frame
	void Update () {

        // DELETEME
        if (Input.GetButtonDown("Test Button"))
            if (!recording)
                StartRecording();
        else
            {
                GetComponent<Looper>().StartLooping(recordedPositions, recordedTimes);
                StopRecording();
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

    private void StopRecording()
    {
        recording = false;
    }

    private void RecordFrame()
    {
        recordingTimer += Time.deltaTime;

        recordedPositions.Enqueue(recordedTransform.position);
        recordedTimes.Enqueue(recordingTimer);
    }
}
