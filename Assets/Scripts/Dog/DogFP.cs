using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]

public class DogFP : MonoBehaviour
{

    public float speed = 10.0f;
    public float gravity = 10.0f;
    public float maxVelocityChange = 10.0f;
    public bool canJump = true;
    public float jumpHeight = 2.0f;
    public MouseLook m_MouseLook;
    public Camera m_Camera;

    private bool grounded = false;
    private Rigidbody rigidbody;
    private Animator m_Animator;

    #region Animator Variables
    public float walkAnimSpeed;
    #endregion

    void Awake()
    {
        // Setup Added refernces
        m_MouseLook.Init(transform, m_Camera.transform);

        // Setup added references
        m_Animator = GetComponentInChildren<Animator>();

        rigidbody = GetComponent<Rigidbody>();
        rigidbody.freezeRotation = true;
        rigidbody.useGravity = false;
    }

    private void Update()
    {
        RotateView();
    }

    void FixedUpdate()
    {
        
    }

    public void Move(bool crouch)
    {
        if (grounded)
        {
            // Calculate how fast we should be moving
            Vector3 targetVelocity = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            targetVelocity = transform.TransformDirection(targetVelocity);
            targetVelocity *= speed;

            // Apply a force that attempts to reach our target velocity
            Vector3 velocity = rigidbody.velocity;
            Vector3 velocityChange = (targetVelocity - velocity);
            velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
            velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
            velocityChange.y = 0;
            rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);

            // Jump
            //if (canJump && Input.GetButton("Jump"))
            //{
            //    rigidbody.velocity = new Vector3(velocity.x, CalculateJumpVerticalSpeed(), velocity.z);
            //}
        }

        // We apply gravity manually for more tuning control
        rigidbody.AddForce(new Vector3(0, -gravity * rigidbody.mass, 0));

        UpdateAnimator();
        m_MouseLook.UpdateCursorLock();

        grounded = false;
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

    private void UpdateAnimator()
    {
        m_Animator.SetFloat("walkingSpeedMultiplier", walkAnimSpeed);
        Vector3 tempVector = rigidbody.velocity;
        tempVector.y = 0;
        m_Animator.SetFloat("walkingSpeed", tempVector.magnitude);
    }
}