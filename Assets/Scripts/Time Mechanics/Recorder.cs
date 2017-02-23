using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;

#if NETWORKING
public class Recorder : NetworkingCharacterAttachment
#else
class Recorder : MonoBehaviour
#endif
{
    // Public references
    public Transform recordedTransform;                 // NOTE TO SELF: GET REFERENCE ON START

    // Private references
    private TimelineManager timelineManager;

    // Recording variables
    float recordingTimer;
    float samplingRate;
    float samplingTimer;

    // Recording data collection
    List<Vector3> recordedPositions;
    List<Quaternion> recordedRotations;
    List<float> recordedTimes;

    // State variables
    bool recording = false;

    // Input variables
#if NETWORKING
    [SyncVar]
#endif
    private bool pressedT = false;

    Recorder clientsHostRecorder;

#if NETWORKING
    protected override void Start()
#else
    void Start()
#endif
    {
        // Setup recording data collection
        recordedPositions = new List<Vector3>();
        recordedTimes = new List<float>();
        recordedRotations = new List<Quaternion>();

        // Setup references
        timelineManager = FindObjectOfType<TimelineManager>();
        
        // Setup variables
        samplingRate = timelineManager.samplingRate;
        samplingTimer = 0;

#if NETWORKING
        // Setup networking
        base.Start();

        if (client)
            clientsHostRecorder = GameObject.Find("clientsHost").GetComponent<Recorder>();
#endif

        pressedT = false;
    }

    // Update is called once per frame
    void Update()
    {
#if NETWORKING
        pressedT = ProcessButtonInput(ButtonEventType.GetButtonDown, "Test Button", pressedT);

         if (CheckIfBreak(pressedT, ref clientsHostRecorder.pressedT))
            return;
#else
        pressedT = Input.GetButtonDown("Test Button");
#endif

        /*
        if (!client && !clientsHost)
            pressedT = Input.GetButtonDown("Test Button");
        else if (pressedT && client)
        {
            clientsHostRecorder.pressedT = true;
            return;                         // HANI QUESTION: WHERE YOU TRYING TO BREAK FROM UPDATE HERE?
        }
        */

        // DELETEME
        if (pressedT)
            if (!recording)
                StartRecording();
            else
            {
                CreateShadow();
                StopRecording();
            }

#if NETWORKING
        ProcessButtonCleanup(ref pressedT);
#endif
        
        /*
        if (clientsHost)
        {
            pressedT = false;               // HANI QUESTION: DO YOU MEAN recorder.pressedT = false ????
        }
        */
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
        samplingTimer = 0;

        recording = true;
    }

    private void StopRecording()
    {
        recording = false;
    }

    private void RecordFrame()
    {
        recordingTimer += Time.deltaTime;
        samplingTimer += Time.deltaTime;

        if (samplingTimer >= samplingRate)
        {
            recordedPositions.Add(recordedTransform.position);
            recordedRotations.Add(recordedTransform.localRotation);
            recordedTimes.Add(recordingTimer);

            // Reset timer to 0 + remainder
            samplingTimer = samplingTimer % samplingRate;
        }
    }

    private void CreateShadow()
    {
        timelineManager.CreateShadow(recordedPositions, recordedRotations, recordedTimes);
    }
}
