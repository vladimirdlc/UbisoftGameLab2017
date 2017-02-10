﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Recorder : MonoBehaviour {

    // Public references
    public Transform recordedTransform;                 // NOTE TO SELF: GET REFERENCE ON START
    public TrailRenderer recordedTrail;

    // Private references
    private TimelineManager timelineManager;

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

        // Setup references
        timelineManager = FindObjectOfType<TimelineManager>();
    }
	
	// Update is called once per frame
	void Update () {

        // DELETEME
        if (Input.GetButtonDown("Test Button"))
            if (!recording)
                StartRecording();
        else
            {
                CreateShadow();
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

    private void CreateShadow()
    {

        recordedTrail.time = 100;                // FIX ME TO NOT HARDCODED
        timelineManager.CreateShadow(recordedPositions, recordedRotations, recordedTimes, recordedTrail);
        recordedTrail.time = 500000000000;      // FIX ME TO NOT HARDCODED
    }
}
