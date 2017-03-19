using UnityEngine;
using UnityEngine.Networking;
using UnityStandardAssets.Characters.FirstPerson;

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

    private bool grounded = false;
    private bool lockedMovement = false;

    NetworkedInput networkedInput;
    void Awake()
    {
        // Setup Added refernces
        m_MouseLook.Init(transform, m_Camera.transform);

        m_RigidBody = GetComponent<Rigidbody>();
        m_RigidBody.freezeRotation = true;
        m_RigidBody.useGravity = false;
        networkedInput = GetComponent<NetworkedInput>();
#if NETWORKING
        GameState.disableControls = false;
#endif
    }

    protected override void Update()
    {
        if (GameState.disableControls) return;
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
            float horizontal = Input.GetAxis("Horizontal");
            //networkedInput.vertical = Input.GetAxis("Vertical");
            float horizontalLook = Input.GetAxis("Mouse X");
            float verticalLook = Input.GetAxis("Mouse Y");

            if (disableStrafe)
                horizontal = 0;

            // Calculate how fast we should be moving
            Vector3 targetVelocity = new Vector3(horizontal, 0, networkedInput.vertical);
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

        UpdateAnimator(crouch);
        m_MouseLook.UpdateCursorLock();

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
        m_MouseLook.LookRotation(transform, m_Camera.transform);
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