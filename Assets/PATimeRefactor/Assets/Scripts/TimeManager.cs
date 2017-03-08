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

    public string playerLayer;
    public string cloneLayer;
    public string doorLayer;
    public string aiLayer;

    public bool m_EnableRewindToZero;
    private bool m_AutoRewinding;

    public Color[] cloneColorCodes;             // Currently being applied to the trails at runtime
    private int cloneColorCodesIndex = 0;       // NOTE FOR PIERRE: maybe this index is uncessesary/dirty depending on how you keep track of clones, let me know

    public int sampleRate;

    public GameObject m_Player;
    public GameObject m_ClonePrefab;
    public GameObject m_Puppy;

    public UnityEngine.UI.Text m_Text;

    public class State
    {
        public Vector3 m_DogPosition { get; set; }
        public Quaternion m_DogRotation { get; set; }
        public Vector3 m_PuppyPosition { get; set; }
        public Quaternion m_PuppyRotation { get; set; }
        public bool m_PuppyIsHome { get; set; }
        public bool m_PuppyIsLatched { get; set; }
        public Vector3 m_PuppyTargetSound { get; set; }
        public Transform m_PuppyTargetTransform { get; set; }

        public State(
            Vector3 dogPos,
            Quaternion dogRot,
            Vector3 pupPos,
            Quaternion pupRot,
            bool isHome,
            bool isLatched,
            Vector3 targetPos,
            Transform targetTrans)
        {
            m_DogPosition = dogPos;
            m_DogRotation = dogRot;
            m_PuppyPosition = pupPos;
            m_PuppyRotation = pupRot;
            m_PuppyIsHome = isHome;
            m_PuppyIsLatched = isLatched;
            m_PuppyTargetSound = targetPos;
            m_PuppyTargetTransform = targetTrans;
        }
    }

    private class Timeline
    {
        public int m_TimelineIndex { get; private set; }
        public int m_TimelineID { get; set; }

        public int m_Start { get; set; }
        public int m_End { get; set; }

        GameObject m_ClonePrefab;
        GameObject m_WarpInPrefab;
        GameObject m_WarpOutPrefab;
        TimeManager m_TimeManager;
        List<State> m_MasterArrayRef;
        Color m_colorCode;

        // This is set at runtime whenever clones pop in or out of a timeline
        GameObject m_CloneInstance;

        // I think these need to be set at runtime since instances are going to pop in and out
        CloneTimeAttachment m_CloneTimeAttachment;
        CloneCharacterController m_CloneController;
        Transform m_CloneTransform;

        public Timeline(int start, GameObject clonePrefab, int id, List<State> masterArray, TimeManager timeManager, Color colorCode, GameObject warpIn = null, GameObject warpOut = null)
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
            m_colorCode = colorCode;
        }

        // Mini helper methods for creating and cleaning up instances
        // Creates a new dog instance at the postition specified by the index, or deletes the current associated dog
        // and null points all the things
        private void create(bool rewinding = false)
        {
            State state = m_MasterArrayRef[m_TimelineIndex];
            m_CloneInstance = Instantiate(m_ClonePrefab, state.m_DogPosition, state.m_DogRotation);

            m_CloneTimeAttachment = m_CloneInstance.GetComponent<CloneTimeAttachment>();
            m_CloneTimeAttachment.timelineID = m_TimelineID;
            m_CloneTimeAttachment.manager = m_TimeManager;

            m_CloneController = m_CloneInstance.GetComponent<CloneCharacterController>();
            m_CloneTransform = m_CloneInstance.GetComponent<Transform>();

            // Texture the trail
            m_CloneController.ColorCode(m_colorCode);

            if (rewinding)
            {
                haltClones();
            }
        }
        public void trash()
        {
            Destroy(m_CloneInstance);
            m_CloneTimeAttachment = null;
            m_CloneController = null;
            m_CloneTransform = null;
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
            trash();
            m_TimelineIndex = index;
        }

        public void inc()
        {
            m_TimelineIndex++;
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

        public void timelineScrub(int amount, int flipOffset = 0)
        {
            m_TimelineIndex += amount + flipOffset;
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

            if (m_TimelineIndex < m_Start || m_TimelineIndex > m_End)
            {
                if (m_CloneInstance != null)
                {
                    // NOTE TO SELF JESUS: WARP IN/OUT EFFECT HERE(?)
                    trash();
                }
            }
            else
            {
                State currentState = m_MasterArrayRef[m_TimelineIndex];
                if (m_CloneInstance == null)
                {
                    // Create instance and assign references
                    // NOTE TO SELF JESUS: WARP IN/OUT EFFECT HERE(?)
                    create(rewinding);
                }

                // Then send the Nav Agent on its way, or hard code the Transform when rewinding
                if (!rewinding) m_CloneController.setTarget(m_MasterArrayRef[m_TimelineIndex + 1]);
                else
                {
                    m_CloneTransform.position = currentState.m_DogPosition;
                    m_CloneTransform.rotation = currentState.m_DogRotation;
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
    private bool m_TimeStopped;
    private bool m_DisableParadoxes;

    private PlayerUserController m_UserController;
    private PuppyCharacterController m_PuppyController;
    private bool m_Paradoxing;
    private int m_RevertTimeline;
    private int m_RevertIndex;
    private Transform m_PlayerTransform;
    private Transform m_PuppyTransform;
    private bool m_Reverting;

    void Start()
    {
        m_MasterArray = new List<State>();
        m_Timelines = new List<Timeline>();
        m_MasterPointer = 0;
        m_PuppyPointer = 0;
        m_ActiveTimeline = 0;
        m_Timelines.Add(new Timeline(0, m_ClonePrefab, 0, m_MasterArray, this, GetNextColorCode()));
        m_Frameticker = sampleRate;
        m_TimeStopped = false;
        m_UserController = m_Player.GetComponent<PlayerUserController>();
        m_PuppyController = m_Puppy.GetComponent<PuppyCharacterController>();
        m_DisableParadoxes = false;

        m_Paradoxing = false;
        m_RevertTimeline = 0;
        m_RevertIndex = 0;
        m_PlayerTransform = m_Player.GetComponent<Transform>();
        m_PuppyTransform = m_Puppy.GetComponent<Transform>();
        m_Reverting = false;

        // Disable collisions between clones
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer(cloneLayer), LayerMask.NameToLayer(cloneLayer), true);
    }

    // Not sure if this should go in FixedUpdate or Update, Fixed seemed safer and more stable (constant frame rate)
    private void FixedUpdate()
    {
        if (!m_TimeStopped)
        {
            if (m_Frameticker == sampleRate)
            {
                requestPush();
                incrementPointers();

                m_Frameticker = 0;
            }
            m_Frameticker++;
        }
    }

    void Update()
    {

        if (m_Text != null)
        {
            m_Text.text = (m_Timelines[0].m_TimelineIndex).ToString();
        }

        if (m_AutoRewinding)
        {
            if (m_Timelines[0].m_TimelineIndex == 0)
            {
                m_AutoRewinding = false;
            }
            else
            {
                masterScrub(-1);
            }
        }
        #region ParadoxForcedRewind
        // When doing the paradox/reverting from the paradox, everything gets bypassed and this happens
        if (m_Paradoxing && !m_Reverting)
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
                    masterScrub(-1, flipOffset);
                }

                // This happens during the force rewind when no "snapping" is required, just a straight
                // call to the masterScrub()
                else
                {
                    masterScrub(-1);
                }
            }
            // Once the forced rewind has reached the point where we want to display the playback of what
            // happened, we start reverting and on next update the first portion will be skipped
            else
            {
                if (!m_Reverting) m_Reverting = true;
            }
        }
        #endregion

        #region ParadoxRevert
        else if (m_Reverting)
        {
            // Start forward playback
            if (m_MasterPointer < m_RevertIndex)
            {
                masterScrub(1);
            }

            // Paradox time/location reached, scrap everything and restore control
            else
            {
                m_MasterArray.RemoveRange(m_MasterPointer + 1, m_MasterArray.Count - m_MasterPointer - 1);
                for (int i = m_RevertTimeline + 1; i < m_Timelines.Count; i++)
                {
                    m_Timelines[i].trash();
                }
                m_ActiveTimeline = m_RevertTimeline;
                m_Timelines[m_ActiveTimeline].open(m_MasterPointer);
                m_Timelines.RemoveRange(m_RevertTimeline + 1, m_Timelines.Count - m_RevertTimeline - 1);

                // Restore control
                m_Paradoxing = false;
                m_TimeStopped = false;
                m_UserController.m_Paradoxing = false;
                m_Reverting = false;

                // Enable collisions
                Physics.IgnoreLayerCollision(LayerMask.NameToLayer(cloneLayer), LayerMask.NameToLayer(playerLayer), false);
                Physics.IgnoreLayerCollision(LayerMask.NameToLayer(cloneLayer), LayerMask.NameToLayer(doorLayer), false);

                // Enable paradoxes
                m_DisableParadoxes = false;

                // Snap puppy pointer to master
                m_PuppyPointer = m_MasterPointer;
            }
        }
        #endregion

        #region NormalPlayBack
        // Update clones normally only when time is not stopped
        // The run() method is called at each rewind call - rewind is called on Update() by the
        // player who controls the time FF and RW mechanisms 
        else if (!m_TimeStopped)
        {
            // For each timeline that is not the active one, call run
            for (int i = 0; i < m_ActiveTimeline; i++)
            {
                m_Timelines[i].runClones();
            }
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
            m_PuppyController.m_IsHome,
            m_PuppyController.m_IsLatched,
            m_PuppyController.m_Target,
            m_PuppyController.m_FollowTargetTransform);

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

    public void timeStopToggle()
    {
        #region TurnTimeStopOff
        if (m_TimeStopped)
        {
            // Enable collisions
            Physics.IgnoreLayerCollision(LayerMask.NameToLayer(cloneLayer), LayerMask.NameToLayer(playerLayer), false);
            Physics.IgnoreLayerCollision(LayerMask.NameToLayer(cloneLayer), LayerMask.NameToLayer(doorLayer), false);

            // Re-enable paradoxes
            m_DisableParadoxes = false;

            // Resume AIs
            for (int i = 0; i < m_ActiveTimeline; i++)
            {
                m_Timelines[i].resumeClones();
            }

            m_PuppyController.resumeAI();

            bool bangThePuppy = true;
            // If the time stop did not result in a rewind, clean-up
            // The timelines do the actual deleting and cleaning up, we just call it here with the open()
            if (m_Timelines[m_ActiveTimeline - 1].m_TimelineIndex == m_MasterPointer)
            {
                bangThePuppy = false;
                m_Timelines[m_ActiveTimeline - 1].open(m_MasterPointer);
                m_Timelines.RemoveAt(m_ActiveTimeline);
                m_ActiveTimeline--;

                // Also replace the pointers (for reasons)
                m_MasterPointer++;
                m_PuppyPointer++;
                for (int i = 0; i < m_Timelines.Count; i++)
                {
                    m_Timelines[i].inc();
                }
            }

            // Signal the puppy (if there was an actual rewind)
            if (bangThePuppy)
            {
                m_PuppyController.hearNoise(m_PlayerTransform.position);
            }

            // Snap back puppy pointer to master
            m_PuppyPointer = m_MasterPointer;

            // Unfreeze time
            m_TimeStopped = false;

            if (m_EnableRewindToZero)
                m_AutoRewinding = false;
        }
        #endregion

        #region TurnTimeStopOn
        else
        {

            // Disable paradoxes
            m_DisableParadoxes = true;

            // Disable collisions
            Physics.IgnoreLayerCollision(LayerMask.NameToLayer(cloneLayer), LayerMask.NameToLayer(playerLayer), true);
            Physics.IgnoreLayerCollision(LayerMask.NameToLayer(cloneLayer), LayerMask.NameToLayer(doorLayer), true);

            // Halt AIs
            for (int i = 0; i < m_ActiveTimeline; i++)
            {
                m_Timelines[i].haltClones();
            }

            m_PuppyController.haltAI();

            // Freeze time and close the current timeline
            // Need also the move the pointers one notch back (for reasons)
            m_TimeStopped = true;
            m_MasterPointer--;
            m_PuppyPointer--;
            for (int i = 0; i < m_Timelines.Count; i++)
            {
                m_Timelines[i].timelineScrub(-1);
            }
            m_Timelines[m_ActiveTimeline].close(m_MasterPointer);
            m_ActiveTimeline++;
            m_Timelines.Add(new Timeline(m_MasterPointer, m_ClonePrefab, m_ActiveTimeline, m_MasterArray, this,GetNextColorCode(true)));

            if (m_EnableRewindToZero)
                m_AutoRewinding = true;
        }
        #endregion

    }

    private void autoRewind() { }

    public void masterScrub(int amount, int flipOffset = 0)
    {
        #region TimeStoppedCheck
        // Only allow scrubbing timelines while time is stopped (dog is sitting or paradoxing)
        if (!m_TimeStopped) return;
        #endregion

        #region ValidScrubCheck
        // Check to make sure the rewind is legal, otherwise apply max value (positive or negative)
        // We only check this if there is not in a paradox or reverting mode, in those modes all
        // scrubs are managed by the system and should be valid
        if (!m_Paradoxing)
        {
            int maxRewind = -1 * m_Timelines[0].m_TimelineIndex;
            int maxForward = m_MasterPointer - m_Timelines[m_ActiveTimeline - 1].m_TimelineIndex;

            if (amount < maxRewind)
                amount = maxRewind;
            if (amount > maxForward)
                amount = maxForward;
        }
        #endregion

        #region MoveClones
        // Move clones (except the one in the active timeline)
        for (int i = 0; i < m_Timelines.Count; i++)
        {
            if (m_Paradoxing || i != m_ActiveTimeline)
            {
                m_Timelines[i].timelineScrub(amount, flipOffset);
            }

            if (m_ActiveTimeline != i)
                m_Timelines[i].runClones(true);
            else
                m_Timelines[i].trash();
        }
        #endregion

        #region MovePuppy
        m_PuppyPointer += amount;
        m_PuppyController.restoreState(m_MasterArray[m_PuppyPointer]);
        #endregion

        #region MovePlayer (if needed)
        // Also move the main timeline while paradoxing and the puppy
        if (m_Paradoxing || m_Reverting)
        {
            m_MasterPointer += amount;

            State nextState = m_MasterArray[m_MasterPointer];
            m_PlayerTransform.position = nextState.m_DogPosition;
            m_PlayerTransform.rotation = nextState.m_DogRotation;
        }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="idToRevert"></param>
    /// <param name="lastPos">If this parameter is set, the method will ensure that the paradox corresponds to the position given in this transform.</param>
    public void handleParadox(int idToRevert, Transform lastPos = null)
    {

        // Paradoxes can't occur when time is stopped
        if (m_DisableParadoxes) return;

        // Slight variation on the code used on time stop toggle on
        m_TimeStopped = true;
        m_Paradoxing = true;
        m_RevertTimeline = idToRevert;

        // Disable paradoxes
        m_DisableParadoxes = true;

        // Disable collisions
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer(cloneLayer), LayerMask.NameToLayer(playerLayer), true);
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer(cloneLayer), LayerMask.NameToLayer(doorLayer), true);

        // Blocking paradox require an override in order to revert to the clone's
        // last valid position and not spawn through a door
        // Find the closest position and revert to that

        m_RevertIndex = m_Timelines[m_RevertTimeline].m_TimelineIndex;

        if (lastPos != null)
        {

#if DEBUG_VERBOSE
            Debug.Log("Blocking Paradox Detected");
#endif

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
        else
#if DEBUG_VERBOSE
            Debug.Log("Proximity Paradox Detected");
#endif

        m_MasterPointer--;
        m_PuppyPointer--;
        for (int i = 0; i < m_Timelines.Count; i++)
        {
            m_Timelines[i].timelineScrub(-1);
        }
        m_Timelines[m_ActiveTimeline].close(m_MasterPointer);

        // Block the user controller
        m_UserController.m_Paradoxing = true;
    }

    // This was a quick and dirty fix, take a closer look at this
    private Color GetNextColorCode(bool increment = false)
    {
        if (increment)
            cloneColorCodesIndex++;

        cloneColorCodesIndex = cloneColorCodesIndex % cloneColorCodes.Length;

        return cloneColorCodes[cloneColorCodesIndex];
    }
}
