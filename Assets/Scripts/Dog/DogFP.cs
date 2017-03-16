using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]

public class DogFP : AnimatedDog
{
    [Header("---- Movement Variables ----")]
    public float speed = 10.0f;
    public float gravity = 10.0f;
    public float maxVelocityChange = 10.0f;
    public bool canJump = true;
    public float jumpHeight = 2.0f;
    public MouseLook m_MouseLook;
    public Camera m_Camera;

    private bool grounded = false;
    private bool lockedMovement = false;

    void Awake()
    {
        // Setup Added refernces
        m_MouseLook.Init(transform, m_Camera.transform);

        m_RigidBody = GetComponent<Rigidbody>();
        m_RigidBody.freezeRotation = true;
        m_RigidBody.useGravity = false;
    }

    private void Update()
    {
        if (GameState.disableControls) return;
        RotateView();
    }

    void FixedUpdate()
    {
        if (GameState.disableControls) return;
        Move(false);
    }

    public void Move(bool crouch)
    {
        if (grounded && !lockedMovement)
        {
            // Calculate how fast we should be moving
            Vector3 targetVelocity = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            targetVelocity = transform.TransformDirection(targetVelocity);
            targetVelocity *= speed;

            // Apply a force that attempts to reach our target velocity
            Vector3 velocity = m_RigidBody.velocity;
            Vector3 velocityChange = (targetVelocity - velocity);
            velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
            velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
            velocityChange.y = 0;
            m_RigidBody.AddForce(velocityChange, ForceMode.VelocityChange);

            // Jump
            //if (canJump && Input.GetButton("Jump"))
            //{
            //    rigidbody.velocity = new Vector3(velocity.x, CalculateJumpVerticalSpeed(), velocity.z);
            //}
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