using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class PlayerUserController : MonoBehaviour
{
#if !USING_ETHAN_CHARACTER
    private DogFP m_Character;          // A reference to the FirstPersonController on the object
#else
    private Character m_Character;      // A reference to the ThirdPersonCharacter on the object
#endif

    public Transform m_Cam;                 // A reference to the main camera in the scenes transform
    private Vector3 m_CamForward;           // The current forward direction of the camera

    private Vector3 m_Move;

    public float m_ScrubBraking;
    public float m_ScrubAcceleration;

    public float m_MaxScrubSpeed;

    private float m_ScrubSpeed;

    private TimeManager m_TimeManager;

    public bool m_IsRewindController;
    public bool m_DisableRewindWhenLatched;
    public bool m_HasPuppy { get; set; }
    public NetworkedInput netInput;
    public NetworkingCharacterAttachment amI;

    private PuppySounds m_SoundBoard;
    private AudioSource m_AudioSource;

    // Input
    private bool m_BarkInput;
    public float m_BarkCD;
    private float m_BarkCDCounter;
    private bool m_BarkReady;
    private bool m_BarkStatePush;

    private PuppySounds m_DogSounds;

    private void Start()
    {
        m_SoundBoard = gameObject.GetComponent<PuppySounds>();
        m_AudioSource = gameObject.GetComponent<AudioSource>();


        m_SoundBoard.PlayBackgroundHum(true);

        m_TimeManager = GameObject.FindGameObjectWithTag("Time Manager").GetComponent<TimeManager>();
        m_DogSounds = GetComponent<PuppySounds>();
        m_ScrubSpeed = 0;
        m_BarkInput = false;
        m_BarkCDCounter = 0.0f;
        m_BarkReady = true;

#if !USING_ETHAN_CHARACTER
        m_Character = GetComponent<DogFP>();
#else
        m_Character = GetComponent<Character>();
#endif
        m_HasPuppy = false;

#if NETWORKING
        amI = GetComponent<NetworkingCharacterAttachment>();
        netInput = GetComponent<NetworkedInput>();
#endif
    }

    public void bark()
    {
        m_AudioSource.PlayOneShot(m_SoundBoard.m_Bark);
    }
    public bool barkTestAndSet()
    {
        if (m_BarkStatePush)
        {
            m_BarkStatePush = false;
            return true;
        }
        else
            return false;
    }
    private void Update()
    {
        if (!m_BarkReady)
        {
            if (Time.time >= m_BarkCDCounter)
                m_BarkReady = true;
        }
    }

    // Fixed update is called in sync with physics
    private void FixedUpdate()
    {
        if (GameState.disableControls) return;

        // Read inputs
        float h = CrossPlatformInputManager.GetAxis("Horizontal");
        float v = CrossPlatformInputManager.GetAxis("Vertical");

#if NETWORKING
        if (amI.host)
        {
            netInput.crouch = Input.GetButton("Ground Stop Time");
            netInput.RW = CrossPlatformInputManager.GetAxis("RW");
            netInput.FF = CrossPlatformInputManager.GetAxis("FF");
            netInput.m_BarkInput = Input.GetButton("Bark");
        }
        bool crouch = netInput.crouch;
        float RW = netInput.RW;
        float FF = netInput.FF;
        m_BarkInput = netInput.m_BarkInput;
#else
        bool crouch = Input.GetButton("Ground Stop Time");
        // NEW INPUT THAT PROBABLY NEEDS TO BE NETWORKED
        m_BarkInput = Input.GetButton("Bark");
        
        float FF = CrossPlatformInputManager.GetAxis("FF");
        float RW = CrossPlatformInputManager.GetAxis("RW");
#endif



        // Compute move vector
        if (crouch)
        {
            m_Character.LockMovement(crouch);
            m_Move = Vector3.zero;
        }
        else if (m_Cam != null)
        {
            // calculate camera relative direction to move:
            m_CamForward = Vector3.Scale(m_Cam.forward, new Vector3(1, 0, 1)).normalized;
            m_Move = v * m_CamForward + h * m_Cam.right;
        }
        else
        {
            // we use world-relative directions in the case of no main camera
            m_Move = v * Vector3.forward + h * Vector3.right;
        }

        // Switch to send user input depending on game state
        switch (m_TimeManager.m_GameState)
        {
            // Do not consider any user input if the game state is in paradox/revert or block mode
            case TimeManager.GameState.PARADOX:
            case TimeManager.GameState.REVERT:
            case TimeManager.GameState.BLOCK:
                break;

            case TimeManager.GameState.NORMAL:

#if !USING_ETHAN_CHARACTER
                m_Character.Move(crouch);
#else
                m_Character.Move(m_Move, crouch);
#endif
                if (m_BarkInput && m_BarkReady)
                {
                    m_BarkStatePush = true;
                    m_BarkReady = false;
                    m_BarkCDCounter = Time.time + m_BarkCD;
                    m_TimeManager.handleBark();
                    m_DogSounds.Bark();
                }
                if (crouch && !(m_DisableRewindWhenLatched && m_HasPuppy))
                {
                    m_TimeManager.timeStopToggle(crouch);
                }
                break;

            case TimeManager.GameState.REWIND:
            case TimeManager.GameState.FORWARD:
            case TimeManager.GameState.TIME_STOPPED:

                switch (m_TimeManager.m_RewindMode)
                {
                    case TimeManager.RewindType.SCRUB:

#if !USING_ETHAN_CHARACTER
                        m_Character.Move(crouch);
#else
                        m_Character.Move(m_Move, crouch);
#endif

                        if (!crouch)
                        {
                            m_ScrubSpeed = 0;
                            m_TimeManager.timeStopToggle(crouch);
                            break;
                        }
                        if (m_IsRewindController)
                        {
                            if (FF != RW)
                            {
                                // Apply acceleration to scrub speed
                                m_ScrubSpeed += FF * m_ScrubAcceleration;
                                m_ScrubSpeed -= RW * m_ScrubAcceleration;

                                if (m_ScrubSpeed > FF)
                                    m_ScrubSpeed = FF;
                                if (m_ScrubSpeed < -RW)
                                    m_ScrubSpeed = -RW;

                            }
                            else if (FF <= 0.01f && RW <= 0.01f)
                            {
                                if (m_ScrubSpeed < 0)
                                {
                                    m_ScrubSpeed += m_ScrubBraking;
                                    if (m_ScrubSpeed > 0)
                                        m_ScrubSpeed = 0;
                                }
                                else if (m_ScrubSpeed > 0)
                                {
                                    m_ScrubSpeed -= m_ScrubBraking;
                                    if (m_ScrubSpeed < 0)
                                        m_ScrubSpeed = 0;
                                }
                            }
                            // Apply scrubSpeed
                            if (m_ScrubSpeed >= 0.01f || m_ScrubSpeed <= -0.01f)
                            {
                                m_TimeManager.lerpScrub(m_ScrubSpeed + (-1 * (int)m_ScrubSpeed));
                                m_TimeManager.masterScrub(((int)m_ScrubSpeed));
                            }

                        }
                        break;
                    case TimeManager.RewindType.HOLD_AND_RELEASE:

#if !USING_ETHAN_CHARACTER
                        m_Character.Move(crouch);
#else
                        m_Character.Move(m_Move, crouch);
#endif

                        if (!crouch)
                        {
                            m_TimeManager.timeStopToggle(crouch);
                        }
                        break;
                    case TimeManager.RewindType.TO_ZERO:
                        if (m_TimeManager.m_WaitingForPlayer)
                        {

#if !USING_ETHAN_CHARACTER
                            m_Character.Move(crouch);
#else
                            m_Character.Move(m_Move, crouch);
#endif

                            if (!crouch)
                            {
                                m_TimeManager.timeStopToggle(crouch);
                            }
                        }
                        break;
                }

                break;
        }
    }
}

