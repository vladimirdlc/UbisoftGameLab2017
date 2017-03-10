using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;


public class PlayerUserController : MonoBehaviour
{
    private Character m_Character;          // A reference to the ThirdPersonCharacter on the object
    public Transform m_Cam;                 // A reference to the main camera in the scenes transform
    private Vector3 m_CamForward;           // The current forward direction of the camera

    private Vector3 m_Move;

    public TimeManager m_TimeManager;
    public float m_ScrubSpeed;

    public bool m_IsRewindController;
    public bool m_DisableRewindWhenLatched;
    public bool m_HasPuppy { get; set; }

    private void Start()
    {
        m_Character = GetComponent<Character>();
        m_HasPuppy = false;
    }

    // Fixed update is called in sync with physics
    private void FixedUpdate()
    {
        // Read inputs
        float h = CrossPlatformInputManager.GetAxis("Horizontal");
        float v = CrossPlatformInputManager.GetAxis("Vertical");
        bool crouch = Input.GetButton("Ground Stop Time");

        float FF = CrossPlatformInputManager.GetAxis("FF");
        float RW = CrossPlatformInputManager.GetAxis("RW");

        // Compute move vector
        if (crouch)
        {
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
            // Do not consider any user input if the game state is in paradox or revert mode
            case TimeManager.GameState.PARADOX:
            case TimeManager.GameState.REVERT:
                break;
            
            case TimeManager.GameState.NORMAL:
                m_Character.Move(m_Move, crouch);
                if (crouch && !(m_DisableRewindWhenLatched && m_HasPuppy))
                {
                    m_TimeManager.timeStopToggle(crouch);
                }
                break;

            case TimeManager.GameState.REWIND:

                switch (m_TimeManager.m_RewindMode)
                {
                    case TimeManager.RewindType.SCRUB:
                        m_Character.Move(m_Move, crouch);
                        if (!crouch)
                            m_TimeManager.timeStopToggle(crouch);
                        if (m_IsRewindController)
                            m_TimeManager.masterScrub((int)((FF - RW) * m_ScrubSpeed));
                        break;
                    case TimeManager.RewindType.HOLD_AND_RELEASE:
                        m_Character.Move(m_Move, crouch);
                        if (!crouch)
                            m_TimeManager.timeStopToggle(crouch);
                        break;
                    case TimeManager.RewindType.TO_ZERO:
                        if (m_TimeManager.m_WaitingForPlayer)
                        {
                            m_Character.Move(m_Move, crouch);
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

