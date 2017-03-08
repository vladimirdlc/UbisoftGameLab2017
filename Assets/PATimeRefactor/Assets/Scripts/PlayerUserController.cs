using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;


public class PlayerUserController : MonoBehaviour
{
    private bool m_disableMovement = false;

    private Character m_Character;          // A reference to the ThirdPersonCharacter on the object
    public Transform m_Cam;                  // A reference to the main camera in the scenes transform
    private Vector3 m_CamForward;             // The current forward direction of the camera
    private Vector3 m_Move;

    public bool m_Paradoxing { get; set; }
    private void Start()
    {
        m_Character = GetComponent<Character>();
        m_Paradoxing = false;
    }

    // Fixed update is called in sync with physics
    private void FixedUpdate()
    {
        if (m_Paradoxing) return;

        // read inputs
        float h = CrossPlatformInputManager.GetAxis("Horizontal");
        float v = CrossPlatformInputManager.GetAxis("Vertical");
        bool crouch = Input.GetButton("Ground Stop Time");

        m_Move = v * Vector3.forward + h * Vector3.right;
        if (crouch)
        {
            m_Move = 0 * Vector3.forward + 0 * Vector3.right;
        }

        //This is the original script for Ethan's movement. The actual controller should use something relative

        //to the attached camera on the player(like what they are doing here).
        //The current version uses world space to figure out where to go.

                    // calculate move direction to pass to character
            if (m_Cam != null)
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
#if !MOBILE_INPUT
        // walk speed multiplier
        if (Input.GetKey(KeyCode.LeftShift)) m_Move *= 0.5f;
#endif

        //pass all parameters to the character control script
        if (!m_disableMovement)
            m_Character.Move(m_Move, crouch);
        else
            // We still pass the crouch if the player isn't moving
            m_Character.Move(new Vector3(0, 0, 0), crouch);
    }

    /// <summary>
    /// Turn movement for this character on or off
    /// </summary>
    public void ToggleMovement()
    {
        m_disableMovement = !m_disableMovement;
    }

    /// <summary>
    /// Overload where you can specify the state of the movement flag
    /// </summary>
    /// <param name="flag">True for on, flase for off</param>
    public void ToggleMovement(bool flag)
    {
        m_disableMovement = !flag;
    }
}

