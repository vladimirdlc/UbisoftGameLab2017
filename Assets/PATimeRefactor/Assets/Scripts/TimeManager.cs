using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{


    // The player, the clones and the doors should all have their own layers
    // 8 - Clone
    // 9 - Player
    // 10 - Door

    // Apparently, toggling the Physics collision off sends an OnTriggerEnter/Exit event, 
    // paradoxes have to be disabled in order to prevent misfire

    public enum RewindType
    {
        SCRUB,
        HOLD_AND_RELEASE,
        TO_ZERO
    };

    public enum GameState
    {
        PARADOX,
        REVERT,
        NORMAL,
        REWIND,
        FORWARD,
        TIME_STOPPED,
        BLOCK
    };

    public enum ParadoxType
    {
        NONE,
        PROXIMITY,
        BLOCKING
    }

    public ParadoxType m_ParadoxType { get; private set; }

    public RewindType m_RewindMode;

    public bool m_SnapCameraToClone;

    public Camera m_PlayerCamera;

    public GameObject m_WarpInPrefab;
    public int m_WarpBubbleLife;

    public GameState m_GameState { get; private set; }

    public string m_PlayerLayer;
    public string m_CloneLayer;
    public string m_DoorLayer;
    public string m_AILayer;
    public string m_PlateLayer;
    public string m_AEOCircleLayer;

    public bool m_WaitingForPlayer { get; set; }

    public Color[] m_CloneColorCodes;             // Currently being applied to the trails at runtime

    public int sampleRate;

    public GameObject m_Player;
    private GameObject m_ClonePrefab;
    public GameObject m_Puppy;

    public UnityEngine.UI.Text m_Text;

    public class State
    {
        public Vector3 m_DogPosition { get; set; }
        public Quaternion m_DogRotation { get; set; }
        public Vector3 m_PuppyPosition { get; set; }
        public Quaternion m_PuppyRotation { get; set; }
        public Vector3 m_PuppyTarget { get; set; }
        public Transform m_PuppyTargetTransform { get; set; }
        public PuppyCharacterController.PuppySate m_PuppyState { get; set; }
        public bool m_PuppyAware { get; set; }
        public bool m_Bark { get; set; }

        public State(
            Vector3 dogPos,
            Quaternion dogRot,
            Vector3 pupPos,
            Quaternion pupRot,
            Vector3 targetPos,
            Transform targetTrans,
            PuppyCharacterController.PuppySate pupState,
            bool pupIsAware,
            bool bark)
        {
            m_DogPosition = dogPos;
            m_DogRotation = dogRot;
            m_PuppyPosition = pupPos;
            m_PuppyRotation = pupRot;
            m_PuppyTarget = targetPos;
            m_PuppyTargetTransform = targetTrans;
            m_PuppyState = pupState;
            m_PuppyAware = pupIsAware;
            m_Bark = bark;
        }
    }

    static public bool disableCameraForOverseer = false;

    private class Timeline
    {
        public int m_TimelineIndex { get; private set; }
        private float m_LerpOffset;
        public int m_TimelineID { get; set; }

        public int m_Start { get; set; }
        public int m_End { get; set; }

        GameObject m_ClonePrefab;
        GameObject m_WarpInPrefab;
        GameObject m_WarpOutPrefab;
        TimeManager m_TimeManager;
        List<State> m_MasterArrayRef;
        int m_WarpBubbleLife;

        Color m_colorCode;

        GameObject m_WarpInInstance;

        // This is set at runtime whenever clones pop in or out of a timeline
        GameObject m_CloneInstance;
        Camera m_CloneCam;

        // I think these need to be set at runtime since instances are going to pop in and out
        CloneTimeAttachment m_CloneTimeAttachment;
        CloneCharacterController m_CloneController;
        Transform m_CloneTransform;

        public Timeline(int start, GameObject clonePrefab, int id, List<State> masterArray, TimeManager timeManager, GameObject warpIn = null, int warpBubbleLife = 0, GameObject warpOut = null)
        {
            m_Start = start;
            m_End = -1;
            m_TimelineIndex = start;
            m_ClonePrefab = clonePrefab;
            m_TimelineID = id;
            m_MasterArrayRef = masterArray;
            m_TimeManager = timeManager;
            m_WarpInPrefab = warpIn;
            m_WarpOutPrefab = warpOut;
            m_colorCode = timeManager.m_CloneColorCodes[id % timeManager.m_CloneColorCodes.Length];
            m_WarpBubbleLife = warpBubbleLife;
        }

        // Mini helper methods for creating and cleaning up instances
        // Creates a new dog instance at the postition specified by the index, or deletes the current associated dog
        // and null points all the things
        private void create(bool rewinding = false)
        {
            State state = m_MasterArrayRef[m_TimelineIndex];
            m_CloneInstance = Instantiate(m_ClonePrefab, state.m_DogPosition, state.m_DogRotation);

            m_CloneTimeAttachment = m_CloneInstance.GetComponent<CloneTimeAttachment>();
            m_CloneTimeAttachment.m_TimelineID = m_TimelineID;
            m_CloneTimeAttachment.manager = m_TimeManager;

            m_CloneController = m_CloneInstance.GetComponent<CloneCharacterController>();
            m_CloneTransform = m_CloneInstance.GetComponent<Transform>();
            m_CloneCam = m_CloneInstance.GetComponentInChildren<Camera>();

            // Texture the trail
            m_CloneController.ColorCode(m_colorCode);

            if (rewinding)
            {
                haltClones();
            }
        }
        public void trashClone(bool onOpen = false)
        {
            if (!onOpen && m_CloneTimeAttachment != null)
            {
                m_CloneTimeAttachment.pressureOff();
            }
            Destroy(m_CloneInstance);
            m_CloneTimeAttachment = null;
            m_CloneController = null;
            m_CloneTransform = null;
            m_CloneCam = null;
        }

        public void trashBubble()
        {
            Destroy(m_WarpInInstance);
        }

        // Called when scrubbing to close a timeline or when a paradox occurs
        public void close(int end)
        {
            m_End = end;
            create();
        }

        // Called when no rewind occured after scrubbing - maybe also when reverting from a paradox
        public void open(int index)
        {
            m_End = -1;
            trashClone(true);
            trashBubble();
            m_TimelineIndex = index;
        }

        public void inc()
        {
            m_TimelineIndex++;
            if (m_WarpInInstance != null)
            {
                m_WarpInInstance.GetComponent<WarpBubble>().scrub(1);
            }
        }

        public void haltClones()
        {
            if (m_CloneTimeAttachment != null)
            {
                m_CloneTimeAttachment.m_Agent.Stop();
            }

        }

        public void resumeClones()
        {
            if (m_CloneTimeAttachment != null)
            {
                m_CloneTimeAttachment.m_Agent.ResetPath();
                m_CloneTimeAttachment.m_Agent.Resume();
            }
        }

        public void timelineLerpScrub(float lerpOffset, bool setTo = false)
        {
            if (setTo)
            {
                m_LerpOffset = lerpOffset;
                return;
            }

            if (m_LerpOffset >= 1.0f)
            {
                m_LerpOffset -= 1.0f;
                timelineScrub(1);
            }
            if (m_LerpOffset <= -1.0f)
            {
                m_LerpOffset += 1.0f;
                timelineScrub(-1);
            }

        }
        public void timelineScrub(int amount, int flipOffset = 0)
        {
            m_TimelineIndex += amount + flipOffset;
            if (m_WarpInInstance != null)
            {
                m_WarpInInstance.GetComponent<WarpBubble>().scrub(amount);
            }
        }

        public void activateCamera(bool active = true)
        {
#if NETWORKING
            if (disableCameraForOverseer)
                active = false;
#endif
            if (m_CloneCam != null)
                m_CloneCam.enabled = active;
        }

        public void runClones(bool rewinding = false)
        {
            // This method calls the Nav Mesh agent that will try to path towards the next position in the timeline
            // It also takes care of Instantiating or Destroying clones instances when they pop in or out
            // This is also where the warpIn and warpOut animations/particle effects should go
            // For now things just get instantiated or destroyed
            // On update, the TimeManager calls this method for each recorded timeline

            // When rewinding, behaviour is different. Players will most likely nudge forward and backward the timelines
            // when figuring out where to land. If we use a Nav Mash agent here, its movement will look awkward.
            // I think we should have a different look for the dogs when rewinding, maybe something ghosty where their legs
            // are blurred. I spend a while trying to figure out how to make the animation look good while rewinding and I don't think it can
            // be done if we allow the players to scrub through the timelines


            // Deal with clone instantiation
            if (m_TimelineIndex < m_Start || m_TimelineIndex > m_End)
            {
                if (m_CloneInstance != null)
                {
                    trashClone();
                }
            }
            else if (m_TimelineIndex >= m_Start && m_TimelineIndex <= m_End)
            {
                if (m_CloneInstance == null)
                {
                    // Create instance and assign references
                    create(rewinding);
                }

                // Then send the Nav Agent on its way, or hard code the Transform when RW or FF
                if (!rewinding)
                {
                    m_CloneController.setTarget(m_MasterArrayRef[m_TimelineIndex + 1]);

                    // Have the clones bark if needed
                    if (m_MasterArrayRef[m_TimelineIndex].m_Bark)
                    {
                        m_TimeManager.handleBark(m_TimelineID);
                    }
                }
                else
                {
                    State lerpFrom;
                    State lerpTo;

                    if (m_LerpOffset < 0)
                    {
                        lerpFrom = m_MasterArrayRef[m_TimelineIndex];
                        lerpTo = m_MasterArrayRef[m_TimelineIndex - 1];
                    }
                    else if (m_LerpOffset > 0)
                    {
                        lerpFrom = m_MasterArrayRef[m_TimelineIndex];
                        lerpTo = m_MasterArrayRef[m_TimelineIndex + 1];
                    }
                    else
                    {
                        lerpFrom = m_MasterArrayRef[m_TimelineIndex];
                        lerpTo = m_MasterArrayRef[m_TimelineIndex];
                    }

                    m_CloneTransform.position = Vector3.Lerp(lerpFrom.m_DogPosition, lerpTo.m_DogPosition, Mathf.Abs(m_LerpOffset));
                    m_CloneTransform.rotation = Quaternion.Slerp(lerpFrom.m_DogRotation, lerpTo.m_DogRotation, Mathf.Abs(m_LerpOffset));
                }
            }

            runWarpBubbles(rewinding);

        }

        public void runWarpBubbles(bool rewinding = false)
        {
            // Deal with warp in bubble instantiation
            if (m_TimelineIndex >= m_Start && m_TimelineIndex <= m_Start + m_WarpBubbleLife && m_TimelineID != 0)
            {
                if (m_WarpInInstance == null)
                {
                    m_WarpInInstance = Instantiate(m_WarpInPrefab, m_MasterArrayRef[m_Start].m_DogPosition + new Vector3(0.0f, 1.0f, 0.0f), m_MasterArrayRef[m_Start].m_DogRotation);
                }
                m_WarpInInstance.GetComponent<WarpBubble>().m_CurrentIndex = m_TimelineIndex - m_Start;
            }
            else if (!(rewinding && m_TimelineID == m_TimeManager.m_ActiveTimeline - 1) && m_End != -1 && m_TimelineIndex >= m_End && m_TimelineIndex <= m_End + m_WarpBubbleLife)
            {
                if (m_WarpInInstance == null)
                {
                    m_WarpInInstance = Instantiate(m_WarpInPrefab, m_MasterArrayRef[m_End].m_DogPosition + new Vector3(0.0f, 1.0f, 0.0f), m_MasterArrayRef[m_End].m_DogRotation);
                }
                m_WarpInInstance.GetComponent<WarpBubble>().m_CurrentIndex = m_TimelineIndex - m_End;
            }
            else
            {
                if (m_WarpInInstance != null)
                {
                    trashBubble();
                }
            }
        }
    }

    private List<State> m_MasterArray;
    private int m_MasterPointer;
    private int m_PuppyPointer;

    private List<Timeline> m_Timelines;
    private int m_ActiveTimeline;

    private int m_Frameticker;

    private PlayerUserController m_UserController;
    private PuppyCharacterController m_PuppyController;
    private int m_RevertTimeline;
    private int m_RevertIndex;
    private Transform m_PlayerTransform;
    private Transform m_PuppyTransform;

    private bool m_RestoreControlOnNextFrame;

    // duct tape - will remove soon
    private int m_CurrentCamera;

    private int m_TimeStopCD;

    private float m_LerpOffset;

    private float m_ScrubSpeed;
    private int m_ParadoxDistance;
    private int m_ParadoxPos;

    public GameObject[] clonePrefabs;

    private void Awake()
    {
#if NETWORKING
        enabled = false;
#endif

    }
    void Start()
    {
        m_Player = GameObject.FindGameObjectWithTag("Player");
        m_Puppy = GameObject.FindGameObjectWithTag("Puppy");
#if NETWORKING
        m_ClonePrefab = clonePrefabs[1];
#else
        m_ClonePrefab = clonePrefabs[0];
#endif
        m_MasterArray = new List<State>();
        m_Timelines = new List<Timeline>();
        m_MasterPointer = 0;
        m_PuppyPointer = 0;
        m_ActiveTimeline = 0;
        m_Timelines.Add(new Timeline(0, m_ClonePrefab, 0, m_MasterArray, this, m_WarpInPrefab, m_WarpBubbleLife));
        m_Frameticker = sampleRate;
        m_UserController = m_Player.GetComponent<PlayerUserController>();
        m_PuppyController = m_Puppy.GetComponent<PuppyCharacterController>();
        m_RevertTimeline = 0;
        m_RevertIndex = 0;
        m_PlayerTransform = m_Player.GetComponent<Transform>();
        m_PuppyTransform = m_Puppy.GetComponent<Transform>();
        m_GameState = GameState.NORMAL;
        m_RestoreControlOnNextFrame = false;
        m_WaitingForPlayer = false;
        m_ParadoxType = ParadoxType.NONE;
        m_CurrentCamera = 0;

        m_TimeStopCD = 0;

        // Disable collisions between clones and between the ground AEO circles
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer(m_CloneLayer), LayerMask.NameToLayer(m_CloneLayer), true);
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer(m_AEOCircleLayer), LayerMask.NameToLayer(m_AEOCircleLayer), true);
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer(m_AEOCircleLayer), LayerMask.NameToLayer(m_CloneLayer), true);
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer(m_AEOCircleLayer), LayerMask.NameToLayer(m_AILayer), true);
    }

    // Not sure if this should go in FixedUpdate or Update, Fixed seemed safer and more stable (constant frame rate)
    private void FixedUpdate()
    {

        switch (m_GameState)
        {
            case GameState.NORMAL:
                if (m_Frameticker == sampleRate)
                {
                    requestPush();
                    incrementPointers();

                    m_Frameticker = 0;
                }
                m_Frameticker++;
                break;

            case GameState.REWIND:
            case GameState.FORWARD:
                // This is where we revert to normal timestop. Set the int in the if statement to whatever feels best
                m_TimeStopCD++;
                if (m_TimeStopCD >= 6)
                {
                    m_TimeStopCD = 0;
                    m_GameState = GameState.TIME_STOPPED;
                }
                break;

            case GameState.PARADOX:
                // Compute scrub speed (values are approx)
                if (m_ScrubSpeed >= -0.95)
                {
                    m_ScrubSpeed -= 0.0125f / 30;
                }
                if (m_ScrubSpeed <= -0.1 && m_MasterPointer <= m_RevertIndex + 30)
                {
                    m_ScrubSpeed += 0.070f / 30;
                }
                break;


            case GameState.REVERT:
                if (m_ScrubSpeed < 0)
                {
                    m_ScrubSpeed *= -1;
                }
                if (m_ScrubSpeed <= 0.025)
                {
                    m_ScrubSpeed += 0.0125f / 30;
                }
                break;
        }

    }

    void Update()
    {
        if (m_RestoreControlOnNextFrame)
        {
            // Reset lerpOffset
            lerpScrub(0.0f, true);

            Physics.IgnoreLayerCollision(LayerMask.NameToLayer(m_CloneLayer), LayerMask.NameToLayer(m_PlayerLayer), false);
            Physics.IgnoreLayerCollision(LayerMask.NameToLayer(m_CloneLayer), LayerMask.NameToLayer(m_DoorLayer), false);
            Physics.IgnoreLayerCollision(LayerMask.NameToLayer(m_CloneLayer), LayerMask.NameToLayer(m_PlateLayer), false);
            Physics.IgnoreLayerCollision(LayerMask.NameToLayer(m_CloneLayer), LayerMask.NameToLayer(m_AILayer), false);

            Physics.IgnoreLayerCollision(LayerMask.NameToLayer(m_PlayerLayer), LayerMask.NameToLayer(m_DoorLayer), false);

            m_GameState = GameState.NORMAL;
            m_ParadoxType = ParadoxType.NONE;
            m_RestoreControlOnNextFrame = false;
        }
        if (m_Text != null)
        {
            m_Text.text = (m_Timelines[0].m_TimelineIndex).ToString();
        }

        #region Rewind
        if (m_GameState == GameState.REWIND || m_GameState == GameState.FORWARD || m_GameState == GameState.TIME_STOPPED)
        {
            if (m_RewindMode == RewindType.TO_ZERO)
            {
                if (m_Timelines[0].m_TimelineIndex == 0)
                {
                    m_WaitingForPlayer = true;
                }
                else
                {
                    masterScrub(-1);
                }
            }
            if (m_RewindMode == RewindType.HOLD_AND_RELEASE)
            {
                masterScrub(-1);
            }
        }
        #endregion

        #region Paradox
        // When doing the paradox/reverting from the paradox, everything gets bypassed and this happens
        if (m_GameState == GameState.PARADOX)
        {


            // The forced rewind will try to reach a few seconds before the paradox actually happened
            // in order to show the player the sequence of events that lead to the paradox
            if (m_MasterPointer > ((m_RevertIndex > 20) ? (m_RevertIndex - 20) : 0))
            {
                // During a forced rewind, timelines need to be snapped up in order to display their proper position
                // This is what the flipOffset is for
                if (m_MasterPointer == m_Timelines[m_ActiveTimeline].m_Start)
                {
                    int flipOffset = 0;
                    // flipOffset value is determined by the rewind amount that lead to the active timeline's position
                    // Current activeTimeline position is the current value of the m_MasterPointer
                    flipOffset = m_MasterPointer - m_Timelines[m_ActiveTimeline - 1].m_TimelineIndex;

                    // Bring down the active timeline and scrub
                    m_ActiveTimeline--;
                    lerpScrub(m_ScrubSpeed + (-1 * (int)m_ScrubSpeed));
                    masterScrub((int)m_ScrubSpeed, flipOffset);
                }

                // This happens during the force rewind when no "snapping" is required, just a straight
                // call to the masterScrub()
                else
                {
                    lerpScrub(m_ScrubSpeed + (-1 * (int)m_ScrubSpeed));
                    masterScrub((int)m_ScrubSpeed);
                }
            }
            // Once the forced rewind has reached the point where we want to display the playback of what
            // happened, we start reverting and on next update the first portion will be skipped
            else
            {
                m_GameState = GameState.REVERT;
            }
        }
        #endregion

        #region Revert
        if (m_GameState == GameState.REVERT)
        {

            // Start forward playback
            if (m_MasterPointer < m_RevertIndex)
            {
                lerpScrub(m_ScrubSpeed + (-1 * (int)m_ScrubSpeed));
                masterScrub((int)m_ScrubSpeed);
            }

            // Paradox time/location reached, scrap everything and restore control
            else
            {
                m_MasterArray.RemoveRange(m_MasterPointer + 1, m_MasterArray.Count - m_MasterPointer - 1);
                for (int i = m_RevertTimeline + 1; i < m_Timelines.Count; i++)
                {
                    m_Timelines[i].trashClone();
                    m_Timelines[i].trashBubble();
                }
                m_ActiveTimeline = m_RevertTimeline;
                m_Timelines[m_ActiveTimeline].open(m_MasterPointer);
                m_Timelines.RemoveRange(m_RevertTimeline + 1, m_Timelines.Count - m_RevertTimeline - 1);

                // Restore control
                m_RestoreControlOnNextFrame = true;

                // Restore Puppy State
                m_PuppyController.restoreState(m_MasterArray[m_MasterPointer]);

                // Nudge pointers up
                // Place pointers to next position
                m_MasterPointer++;
                for (int i = 0; i < m_Timelines.Count; i++)
                {
                    m_Timelines[i].inc();
                }

                // Snap puppy pointer to master
                m_PuppyPointer = m_MasterPointer;
            }
        }
        #endregion

        #region Normal
        // Update clones normally only when time is not stopped
        // The run() method is called at each rewind call - rewind is called on Update() by the
        // player who controls the time FF and RW mechanisms 
        else if (m_GameState == GameState.NORMAL)
        {
            // For each timeline that is not the active one, call run
            for (int i = 0; i < m_ActiveTimeline; i++)
            {
                m_Timelines[i].runClones();
            }

            // Then check for warpBubble instantiation - note that those are done automatically when calling runClones()
            m_Timelines[m_ActiveTimeline].runWarpBubbles();
        }
        #endregion
    }

    private void requestPush()
    {
        State state = new global::TimeManager.State(
            m_PlayerTransform.position,
            m_PlayerTransform.rotation,
            m_PuppyTransform.position,
            m_PuppyTransform.rotation,
            m_PuppyController.m_Target,
            m_PuppyController.m_FollowDogTransform,
            m_PuppyController.m_PuppyState,
            m_PuppyController.m_IsAware,
            m_UserController.barkTestAndSet());

        m_MasterArray.Add(state);
    }

    private void incrementPointers()
    {
        // Called on Update() on "normal" play
        m_MasterPointer++;
        m_PuppyPointer++;
        for (int i = 0; i < m_Timelines.Count; i++)
        {
            m_Timelines[i].inc();
        }
    }

    public void handleBark(int timeLineId = -1)
    {
        Vector3 soundSource;
        soundSource = m_MasterArray[(timeLineId == -1) ? m_MasterPointer - 1 : m_Timelines[timeLineId].m_TimelineIndex - 1].m_DogPosition;
        m_PuppyController.hearNoise(soundSource);
    }


    public void timeStopToggle(bool stopTime)
    {
        #region TurnTimeStopOff
        if (!stopTime)
        {
            if (m_GameState == GameState.TIME_STOPPED || m_GameState == GameState.REWIND || m_GameState == GameState.FORWARD)
            {
                // Switch camera 
                if (m_SnapCameraToClone)
                {
                    m_Timelines[m_CurrentCamera].activateCamera(false);
#if NETWORKING
                    if (disableCameraForOverseer)
                        m_PlayerCamera.enabled = false;
                    else
                        m_PlayerCamera.enabled = true;
#else
                        m_PlayerCamera.enabled = true;
#endif
                }

                m_WaitingForPlayer = false;
                bool warped = true;
                // If the time stop did not result in a rewind, clean-up
                if (m_Timelines[m_ActiveTimeline - 1].m_TimelineIndex == m_MasterPointer)
                {
                    m_Timelines[m_ActiveTimeline - 1].open(m_MasterPointer);
                    m_Timelines.RemoveAt(m_ActiveTimeline);
                    m_ActiveTimeline--;
                    warped = false;
                }

                if (warped)
                {
                    m_PuppyController.warpOutTarget();
                }
                // Resume AIs
                for (int i = 0; i < m_ActiveTimeline; i++)
                {
                    m_Timelines[i].resumeClones();
                }
                m_PuppyController.resumeAI();

                // Place pointers to next position
                m_MasterPointer++;

                // Snap back puppy pointer to master
                m_PuppyPointer = m_MasterPointer;

                for (int i = 0; i < m_Timelines.Count; i++)
                {
                    m_Timelines[i].inc();
                }

                // Enable collisions and control
                m_RestoreControlOnNextFrame = true;

                if (m_SnapCameraToClone)
                {
                    m_CurrentCamera = m_ActiveTimeline;
                }

            }
        }
        #endregion

        #region TurnTimeStopOn
        else if (m_GameState == GameState.NORMAL && stopTime == true)
        {
            // Set current state
            m_GameState = GameState.TIME_STOPPED;

            // Disable collisions
            Physics.IgnoreLayerCollision(LayerMask.NameToLayer(m_CloneLayer), LayerMask.NameToLayer(m_PlayerLayer), true);
            Physics.IgnoreLayerCollision(LayerMask.NameToLayer(m_CloneLayer), LayerMask.NameToLayer(m_DoorLayer), true);
            Physics.IgnoreLayerCollision(LayerMask.NameToLayer(m_CloneLayer), LayerMask.NameToLayer(m_PlateLayer), true);
            Physics.IgnoreLayerCollision(LayerMask.NameToLayer(m_CloneLayer), LayerMask.NameToLayer(m_AILayer), true);

            // Halt AIs
            for (int i = 0; i < m_ActiveTimeline; i++)
            {
                m_Timelines[i].haltClones();
            }
            m_PuppyController.haltAI();

            // Nudge pointers to next reading position
            m_MasterPointer--;
            m_PuppyPointer--;
            for (int i = 0; i < m_Timelines.Count; i++)
            {
                m_Timelines[i].timelineScrub(-1);
            }

            // Close the current timeline
            m_Timelines[m_ActiveTimeline].close(m_MasterPointer);
            m_ActiveTimeline++;
            m_Timelines.Add(new Timeline(m_MasterPointer, m_ClonePrefab, m_ActiveTimeline, m_MasterArray, this, m_WarpInPrefab, m_WarpBubbleLife));


            // Switch camera 
            if (m_SnapCameraToClone)
            {
                m_PlayerCamera.enabled = false;
                m_CurrentCamera = m_ActiveTimeline - 1;
                m_Timelines[m_CurrentCamera].activateCamera(true);
            }
        }
        #endregion

    }

    public void lerpScrub(float lerpOffset, bool setTo = false)
    {
        // Prevent lerping beyond array limits
        if ((m_Timelines[0].m_TimelineIndex == 0 && lerpOffset < 0) ||
            (m_ActiveTimeline != 0 && (m_Timelines[m_ActiveTimeline - 1].m_TimelineIndex == m_MasterPointer && lerpOffset > 0)))
        {
            m_LerpOffset = 0;
            return;
        }

        if (setTo)
        {
            m_LerpOffset = lerpOffset;
            for (int i = 0; i < m_Timelines.Count; i++)
            {
                m_Timelines[i].timelineLerpScrub(lerpOffset, true);
            }
            return;
        }



        m_LerpOffset += lerpOffset;
        for (int i = 0; i < m_Timelines.Count; i++)
        {
            m_Timelines[i].timelineLerpScrub(lerpOffset);
        }

        if (m_LerpOffset >= 1.0f)
        {
            m_LerpOffset -= 1.0f;
            masterScrub(1);
        }
        if (m_LerpOffset <= -1.0f)
        {
            m_LerpOffset += 1.0f;
            masterScrub(-1);
        }
    }
    public void masterScrub(int amount, int flipOffset = 0)
    {
        #region ValidScrubCheck
        // Check to make sure the rewind is legal, otherwise apply max value (positive or negative)
        // We only check this if there is not in a paradox or reverting mode, in those modes all
        // scrubs are managed by the system and should be valid
        if (m_GameState == GameState.TIME_STOPPED || m_GameState == GameState.REWIND || m_GameState == GameState.FORWARD)
        {
            int maxRewind = -1 * m_Timelines[0].m_TimelineIndex;
            int maxForward = m_MasterPointer - m_Timelines[m_ActiveTimeline - 1].m_TimelineIndex;

            if (amount < maxRewind)
                amount = maxRewind;
            if (amount > maxForward)
                amount = maxForward;
        }
        #endregion

        //Set Game state and reset CD
        if (amount < 0 || m_LerpOffset < 0)
        {
            if (m_GameState != GameState.PARADOX && m_GameState != GameState.REVERT)
                m_GameState = GameState.REWIND;
        }

        else if (amount > 0 || m_LerpOffset > 0)
        {
            if (m_GameState != GameState.PARADOX && m_GameState != GameState.REVERT)
                m_GameState = GameState.FORWARD;
        }


        m_TimeStopCD = 0;

        #region MoveClones
        // Move clones
        for (int i = 0; i < m_Timelines.Count; i++)
        {
            if (m_GameState == GameState.PARADOX || i != m_ActiveTimeline)
            {
                m_Timelines[i].timelineScrub(amount, flipOffset);
            }

            if (m_ActiveTimeline != i)
                m_Timelines[i].runClones(true);
            else
                m_Timelines[i].trashClone(true);
        }
        #endregion

        #region MovePuppy
        if (m_GameState != GameState.NORMAL)
        {
            m_PuppyPointer += amount;
            if (m_PuppyPointer >= m_MasterArray.Count)
                m_PuppyPointer = m_MasterArray.Count - 1;
            else if (m_PuppyPointer < 0)
                m_PuppyPointer = 0;
            m_PuppyController.restoreState(m_MasterArray[m_PuppyPointer]);
        }
        #endregion

        #region MovePlayer (if needed)
        // Also move the main timeline while paradoxing and the puppy
        if (m_GameState == GameState.PARADOX || m_GameState == GameState.REVERT)
        {
            m_MasterPointer += amount;

            State lerpFrom;
            State lerpTo;

            if (m_LerpOffset < 0 && m_MasterPointer != 0)
            {
                lerpFrom = m_MasterArray[m_MasterPointer];
                lerpTo = m_MasterArray[m_MasterPointer - 1];
            }
            else if (m_LerpOffset > 0)
            {
                lerpFrom = m_MasterArray[m_MasterPointer];
                lerpTo = m_MasterArray[m_MasterPointer + 1];
            }
            else
            {
                lerpTo = m_MasterArray[m_MasterPointer];
                lerpFrom = m_MasterArray[m_MasterPointer];
            }

            m_PlayerTransform.position = Vector3.Lerp(lerpFrom.m_DogPosition, lerpTo.m_DogPosition, Mathf.Abs(m_LerpOffset));
            m_PlayerTransform.rotation = Quaternion.Slerp(lerpFrom.m_DogRotation, lerpTo.m_DogRotation, Mathf.Abs(m_LerpOffset));
        }
        #endregion

        // Special case when rewinding (duct tape used here, more robust solution will follow)
        if (m_SnapCameraToClone && (m_GameState == GameState.REWIND || m_GameState == GameState.FORWARD || m_GameState == GameState.TIME_STOPPED))
        {
            // Find the "latest" running timeline
            for (int i = m_ActiveTimeline - 1; i >= 0; i--)
            {
                if (
                    ((m_Timelines[i].m_TimelineIndex <= m_Timelines[i].m_End - 1))
                    &&
                    ((m_Timelines[i].m_TimelineIndex >= m_Timelines[i].m_Start + 1))
                )
                {
                    m_Timelines[m_CurrentCamera].activateCamera(false);
                    m_CurrentCamera = i;
                    m_Timelines[m_CurrentCamera].activateCamera(true);
                    break;
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="idToRevert"></param>
    /// <param name="lastPos">If this parameter is set, the method will ensure that the paradox corresponds to the position given in this transform.</param>
    public void handleParadox(int idToRevert, Transform lastPos = null)
    {

        // Paradoxes can only occur on NORMAL game state
        if (m_GameState != GameState.NORMAL) return;

        // Otherwise deal with the paradox...
        m_GameState = GameState.PARADOX;

        // Set paradox type to proximity, will be overriden by the lastPost null check if needed
        m_ParadoxType = ParadoxType.PROXIMITY;

        // Disable collisions
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer(m_CloneLayer), LayerMask.NameToLayer(m_PlayerLayer), true);
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer(m_CloneLayer), LayerMask.NameToLayer(m_DoorLayer), true);
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer(m_CloneLayer), LayerMask.NameToLayer(m_PlateLayer), true);
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer(m_CloneLayer), LayerMask.NameToLayer(m_AILayer), true);

        Physics.IgnoreLayerCollision(LayerMask.NameToLayer(m_PlayerLayer), LayerMask.NameToLayer(m_DoorLayer), true);


        // Fetch revert targert
        m_RevertTimeline = idToRevert;

        // Blocking paradox require an override in order to revert to the clone's
        // last valid position and not spawn through a door
        // Find the closest position and revert to that
        m_RevertIndex = m_Timelines[m_RevertTimeline].m_TimelineIndex;

        if (lastPos != null)
        {
            m_ParadoxType = ParadoxType.BLOCKING;

            float currentDistance = (lastPos.position - m_MasterArray[m_RevertIndex].m_DogPosition).magnitude;
            m_RevertIndex--;
            float nextDistance = (lastPos.position - m_MasterArray[m_RevertIndex].m_DogPosition).magnitude;

            while (currentDistance > nextDistance)
            {
                currentDistance = nextDistance;
                m_RevertIndex--;
                nextDistance = (lastPos.position - m_MasterArray[m_RevertIndex].m_DogPosition).magnitude;
            }
        }


        // Nudge back pointers to reading position
        m_MasterPointer--;
        m_PuppyPointer--;
        for (int i = 0; i < m_Timelines.Count; i++)
        {
            m_Timelines[i].timelineScrub(-1);
        }
        m_Timelines[m_ActiveTimeline].close(m_MasterPointer);

        // Init parameters for varying paradox and revert speed
        m_ScrubSpeed = 0;
        m_ParadoxPos = 0;
        m_ParadoxDistance = m_MasterPointer - m_RevertIndex;

        // On update, will execute portion of code for paradox state
    }
}