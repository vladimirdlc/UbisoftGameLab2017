using UnityEngine;
using UnityEngine.Networking;
using UnityStandardAssets.Characters.FirstPerson;
using UnityStandardAssets.CrossPlatformInput;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]

public class DogFP : AnimatedDog
{
    [Header("---- Movement Variables ----")]
    public float speed = 10.0f;
    public bool enableTilt;
    public bool disableStrafe;
    public float gravity = 10.0f;
    public float maxVelocityChange = 10.0f;
    public bool canJump = true;
    public float jumpHeight = 2.0f;
    public MouseLook m_MouseLook;
    public Camera m_Camera;

    public GameObject m_LeftRockets;
    public GameObject m_RightRpckets;

    private bool grounded = false;
    private bool lockedMovement = false;
    private bool controlsEnabled = false;

    private AudioSource m_MovementAudiosource;

    // Input
    float horizontal = 0;
    float vertical = 0;

#if NETWORKING
    NetworkedInput networkedInput;
    NetworkingCharacterAttachment amI;
#endif
    void Awake()
    {
        // Setup Added refernces
        m_MouseLook.Init(transform, m_Camera.transform);

        m_RigidBody = GetComponent<Rigidbody>();
        m_MovementAudiosource = GetComponentInChildren<AudioSource>();
        m_RigidBody.freezeRotation = true;
        m_RigidBody.useGravity = false;

#if NETWORKING
        networkedInput = GetComponent<NetworkedInput>();

        amI = GetComponent<NetworkingCharacterAttachment>();
        GameState.disableControls = false;
#endif
    }

    protected override void Update()
    {
        if (!controlsEnabled)
            if (GameState.disableControls) return;
            else
            {
                controlsEnabled = true;
                m_Camera.transform.localPosition = Vector3.zero;
                m_Camera.transform.localRotation = Quaternion.identity;
            }

#if NETWORKING
        if (amI.host)
        {
            networkedInput.horizontal = Input.GetAxis("Horizontal");
            networkedInput.vertical = Input.GetAxis("Vertical");
        }
#else
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

#endif

        base.Update();
        RotateView();
    }

    void FixedUpdate()
    {
        if (GameState.disableControls) return;
        Move(false);
    }



    public void Move(bool crouch)
    {
        if (grounded && !lockedMovement && !GameState.disableControls)
        {
            float horizontalLook = Input.GetAxis("Mouse X");
            float verticalLook = Input.GetAxis("Mouse Y");

            if (disableStrafe)
            {
#if NETWORKING
                networkedInput.horizontal = 0;
#else
                horizontal = 0;
#endif
            }
            else
                // Strafe speed is slower than the forward speed
                horizontal *= 0.8f;

            // Calculate how fast we should be moving
#if NETWORKING
            Vector3 targetVelocity = new Vector3(networkedInput.horizontal, 0, networkedInput.vertical);
#else
            Vector3 targetVelocity = new Vector3(horizontal, 0, vertical);
#endif

            targetVelocity = transform.TransformDirection(targetVelocity);
            targetVelocity *= speed;

            // Apply a force that attempts to reach our target velocity
            Vector3 velocity = m_RigidBody.velocity;
            Vector3 velocityChange = (targetVelocity - velocity);
            velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
            velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
            velocityChange.y = 0;
            m_RigidBody.AddForce(velocityChange, ForceMode.VelocityChange);

            // Tilt head when turning
            if (enableTilt)
            {
                if (horizontalLook >= 0.2f)
                    tiltRight = true;

                if (horizontalLook <= -0.2f)
                    tiltLeft = true;
            }

            // Rockets
#if NETWORKING
            if (networkedInput.horizontal < 0.05f && networkedInput.horizontal > -0.05f)
#else
            if(horizontal < 0.05f && horizontal >-0.05f)
#endif
            {
                m_LeftRockets.SetActive(false);
                m_RightRpckets.SetActive(false);
            }
            else
#if NETWORKING
                  if (networkedInput.horizontal > 0.1f)
#else
            if(horizontal > 0.1f)
#endif
            {
                m_LeftRockets.SetActive(true);
                m_RightRpckets.SetActive(false);
            }
#if NETWORKING
            else if (networkedInput.horizontal < -0.1f)
#else
                      else if (horizontal < -0.1f)
#endif
            {
                m_LeftRockets.SetActive(false);
                m_RightRpckets.SetActive(true);
            }


            // Jump
            //if (canJump && Input.GetButton("Jump"))
            //{
            //    rigidbody.velocity = new Vector3(velocity.x, CalculateJumpVerticalSpeed(), velocity.z);
            //}
        }

        if (lockedMovement || GameState.disableControls)
        {
            m_RigidBody.velocity = new Vector3(0, 0, 0);
        }

        // We apply gravity manually for more tuning control
        m_RigidBody.AddForce(new Vector3(0, -gravity * m_RigidBody.mass, 0));

        UpdateAnimator(horizontal, crouch);
        m_MouseLook.UpdateCursorLock();

        if (Mathf.Abs(vertical) >= 0.1f)
        {
            // MOVE SOUND
            if (!m_MovementAudiosource.isPlaying)
                m_MovementAudiosource.Play();
        }
        else
        {
            m_MovementAudiosource.Pause();
        }

        // Reset state flags
        grounded = false;
        lockedMovement = false;
    }

    void OnCollisionStay()
    {
        grounded = true;
    }

    private void RotateView()
    {
#if NETWORKING
        if (amI.host)
        {
            networkedInput.yRot = CrossPlatformInputManager.GetAxis("Mouse X");
            networkedInput.xRot = CrossPlatformInputManager.GetAxis("Mouse Y");
        }
        m_MouseLook.LookRotation(transform, m_Camera.transform, networkedInput.xRot, networkedInput.yRot);
        if (amI.host)
            networkedInput.rotn = transform.rotation;
        if (amI.clientsHost)
            transform.rotation = networkedInput.rotn;
#else
        float yRot = CrossPlatformInputManager.GetAxis("Mouse X");
        float xRot = CrossPlatformInputManager.GetAxis("Mouse Y");
        m_MouseLook.LookRotation(transform, m_Camera.transform, xRot, yRot);
#endif

    }

    float CalculateJumpVerticalSpeed()
    {
        // From the jump height and gravity we deduce the upwards speed 
        // for the character to reach at the apex.
        return Mathf.Sqrt(2 * jumpHeight * gravity);
    }

    /// <summary>
    /// The lock movement flag gets reset at the end of fixed update, so call this function on update only.
    /// </summary>
    /// <param name="moveLock"></param>
    public void LockMovement(bool moveLock)
    {
        lockedMovement = moveLock;
    }
}