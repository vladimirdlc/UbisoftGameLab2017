using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Recorder : MonoBehaviour {

    public Transform recordedTransform;
    public TrailRenderer recordedTrail;

    // Recording variables
    float recordingTimer;

    // Recording data collection
    Queue<Vector3> recordedPositions;
    Queue<Quaternion> recordedRotations;
    Queue<float> recordedTimes;

    // State variables
    bool recording = false;

	// Use this for initialization
	void Start ()
    {
        // Setup recording data collection
        recordedPositions = new Queue<Vector3>();
        recordedTimes = new Queue<float>();
        recordedRotations = new Queue<Quaternion>();
    }
	
	// Update is called once per frame
	void Update () {

        // DELETEME
        if (Input.GetButtonDown("Test Button"))
            if (!recording)
                StartRecording();
        else
            {
                recordedTrail.time = 100;                // FIX ME TO NOT HARDCODED
                GetComponent<Looper>().StartLooping(recordedPositions, recordedRotations, recordedTimes, recordedTrail);
                recordedTrail.time = 500000000000;      // FIX ME TO NOT HARDCODED
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

        recordedTrail.enabled = true;

        recording = true;
    }

    private void StopRecording()
    {
        recordedTrail.enabled = false;

        recording = false;
    }

    private void RecordFrame()
    {
        recordingTimer += Time.deltaTime;

        recordedPositions.Enqueue(recordedTransform.position);
        recordedRotations.Enqueue(recordedTransform.localRotation);
        recordedTimes.Enqueue(recordingTimer);
    }
}
