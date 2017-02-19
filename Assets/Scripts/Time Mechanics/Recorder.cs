using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;

public class Recorder : NetworkBehaviour
{
    // Public references
    public Transform recordedTransform;                 // NOTE TO SELF: GET REFERENCE ON START

    // Private references
    private TimelineManager timelineManager;

    // Recording variables
    float recordingTimer;

    // Recording data collection
    List<Vector3> recordedPositions;
    List<Quaternion> recordedRotations;
    List<float> recordedTimes;

    // State variables
    bool recording = false;

    // Input variables
    [SyncVar]
    private bool pressedT;

    // Use this for networking initialization
    bool server = NetworkCustom.isServer;
    bool clientsHost;
    bool client;
    Recorder recorder;

    void Start()
    {
        // Setup recording data collection
        recordedPositions = new List<Vector3>();
        recordedTimes = new List<float>();
        recordedRotations = new List<Quaternion>();

        // Setup references
        timelineManager = FindObjectOfType<TimelineManager>();

        // Setup networking
        clientsHost = !isLocalPlayer && !server;
        client = !server && isLocalPlayer;
        if (client)
            recorder = GameObject.Find("clientsHost").GetComponent<Recorder>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!client && !clientsHost)
            pressedT = Input.GetButtonDown("Test Button");
        else if (pressedT && client)
        {
            recorder.pressedT = true;
            return;                         // HANI QUESTION: WHERE YOU TRYING TO BREAK FROM UPDATE HERE?
        }

        // DELETEME
        if (pressedT)
            if (!recording)
                StartRecording();
            else
            {
                CreateShadow();
                StopRecording();
            }

        if (clientsHost)
        {
            pressedT = false;               // HANI QUESTION: DO YOU MEAN recorder.pressedT = false ????
        }
    }

    private void LateUpdate()
    {
        if (recording)
        {
            RecordFrame();
        }
    }

    private void StartRecording()
    {
        // Wipe old data structures
        recordedPositions = new List<Vector3>();
        recordedTimes = new List<float>();
        recordedRotations = new List<Quaternion>();

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

        recordedPositions.Add(recordedTransform.position);
        recordedRotations.Add(recordedTransform.localRotation);
        recordedTimes.Add(recordingTimer);
    }

    private void CreateShadow()
    {
        timelineManager.CreateShadow(recordedPositions, recordedRotations, recordedTimes);
    }
}
