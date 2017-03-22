using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Animator))]
public class PuppyMovement : MonoBehaviour
{
    [SerializeField]
    float m_MovingTurnSpeed = 360;
    [SerializeField]
    float m_StationaryTurnSpeed = 180;
    [SerializeField]
    float m_JumpPower = 12f;
    [Range(1f, 4f)]
    [SerializeField]
    float m_GravityMultiplier = 2f;
    [SerializeField]
    float m_RunCycleLegOffset = 0.2f; //specific to the character in sample assets, will need to be modified to work with others
    [SerializeField]
    float m_MoveSpeedMultiplier = 1f;
    [SerializeField]
    float m_GroundCheckDistance = 0.1f;

    Rigidbody m_Rigidbody;
    Animator m_Animator;
    bool m_IsGrounded;
    float m_OrigGroundCheckDistance;
    const float k_Half = 0.5f;
    float m_TurnAmount;
    float m_ForwardAmount;
    Vector3 m_GroundNormal;
    float m_CapsuleHeight;
    Vector3 m_CapsuleCenter;
    CapsuleCollider m_Capsule;
    bool m_Crouching;

    [Header("-------- Animator Variables --------")]
    [Tooltip("Amount of time it takes to cycle between idle animations")]
    public float idleCountdown;
    public float walkingAnimatorSpeed;
    public float barkAnimatorSpeed;
    public float loveEmoteAnimatorSpeed;
    public float pawSpeed;
    public float tailChaseSpeed;
    public float sitSpeed;
    public float sittingTailWagSpeed;

    private float m_IdleTimer;
    private bool m_CycleIdle = false;
    private PuppyCharacterController.PuppySate m_PuppyState;

    private PuppySounds m_PuppySpounds;
    public AudioSource m_MovementAudiosource;

    void Start()
    {
        m_Animator = GetComponent<Animator>();
        m_Rigidbody = GetComponent<Rigidbody>();
        m_Capsule = GetComponent<CapsuleCollider>();
        m_CapsuleHeight = m_Capsule.height;
        m_CapsuleCenter = m_Capsule.center;

        m_PuppySpounds = GetComponent<PuppySounds>();

        m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        m_OrigGroundCheckDistance = m_GroundCheckDistance;

        // Animator setup
        m_Animator.SetFloat("loveSpeed", loveEmoteAnimatorSpeed);

        m_IdleTimer = idleCountdown;
        Random.InitState(42);
    }

    private void Update()
    {
        m_IdleTimer -= Time.deltaTime;
        if (m_IdleTimer <= 0)
        {
            m_IdleTimer = idleCountdown;
            m_CycleIdle = true;
        }
    }

    public void Move(Vector3 move, bool crouch)
    {

        // convert the world relative moveInput vector into a local-relative
        // turn amount and forward amount required to head in the desired
        // direction.
        if (move.magnitude > 1f)
            move.Normalize();


        move = transform.InverseTransformDirection(move);
        CheckGroundStatus();
        move = Vector3.ProjectOnPlane(move, m_GroundNormal);
        m_TurnAmount = Mathf.Atan2(move.x, move.z);
        m_ForwardAmount = move.z;
        m_Crouching = crouch;
        m_IsGrounded = true;


        ApplyExtraTurnRotation();

        if (move.magnitude > 0.5f)
        {
            // MOVE SOUND
            if (!m_MovementAudiosource.isPlaying)
                m_MovementAudiosource.Play();
        }
        else
        {
            m_MovementAudiosource.Pause();
        }

        // send input and other state parameters to the animator
        UpdateAnimator(move);
    }

    void UpdateAnimator(Vector3 move)
    {
        // update the animator parameters
        m_Animator.SetFloat("walkingSpeed", m_ForwardAmount, 0.1f, Time.deltaTime);

        //TODO
        //m_Animator.SetFloat("Turn", m_TurnAmount, 0.1f, Time.deltaTime);

        m_Animator.SetBool("sitting", m_Crouching);


        //m_Animator.SetBool("OnGround", m_IsGrounded);

        m_Animator.SetFloat("walkingSpeed", walkingAnimatorSpeed * m_ForwardAmount);
        m_Animator.SetFloat("barkingSpeed", barkAnimatorSpeed);
        m_Animator.SetFloat("pawSpeed", pawSpeed);
        m_Animator.SetFloat("tailChaseSpeed", tailChaseSpeed);
        m_Animator.SetFloat("sitSpeed", sitSpeed);
        m_Animator.SetFloat("sittingTailWagSpeed", sittingTailWagSpeed);

        if (m_CycleIdle)
        {
            PlayRandomIdle();
            m_CycleIdle = false;
        }
    }

    void PlayRandomIdle()
    {
        /*
        Debug.Log("IDLE_PLAYER is :" + (m_PuppyState == PuppyCharacterController.PuppySate.IDLE_PLAYER));
        Debug.Log("Aware is: " + GameObject.FindGameObjectWithTag("Puppy").GetComponent<PuppyCharacterController>().m_IsAware);
        */


        if (m_PuppyState == PuppyCharacterController.PuppySate.IDLE_PLAYER || m_PuppyState == PuppyCharacterController.PuppySate.IDLE_HOME)
        {

            float rndFloat = Random.Range(0.0f, 1.0f);

            if (rndFloat <= 0.25)
            {
                m_Animator.SetTrigger("bark");
                m_PuppySpounds.Bark();
            }
            else if (rndFloat <= 0.50)
            {
                m_Animator.SetTrigger("tailChase");
                m_PuppySpounds.TailChase();
            }
            else if (rndFloat <= 0.75)
            {
                m_Animator.SetTrigger("sitOnce");
            }
            else
            {
                m_Animator.SetTrigger("paw");
                m_PuppySpounds.Paw();
            }
        }
    }

    void ApplyExtraTurnRotation()
    {
        // help the character turn faster (this is in addition to root rotation in the animation)
        float turnSpeed = Mathf.Lerp(m_StationaryTurnSpeed, m_MovingTurnSpeed, m_ForwardAmount);
        transform.Rotate(0, m_TurnAmount * turnSpeed * Time.deltaTime, 0);
    }

    /// <summary>
    /// To sit the puppy down while he is staring at something (namely the player)
    /// @Pierre
    /// TODO: Actually make the call
    /// </summary>
    public void Sit()
    {
        m_Animator.SetTrigger("sitContinuous");
    }

    /// <summary>
    /// The puppy stands back up (goes back to the idel loop) if it was sitting. No effect if it wasn't
    /// </summary>
    public void StandUp()
    {
        m_Animator.SetTrigger("standUp");
    }

    public void OnAnimatorMove()
    {
        // we implement this function to override the default root motion.
        // this allows us to modify the positional speed before it's applied.
        if (m_IsGrounded && Time.deltaTime > 0)
        {
            Vector3 v = (m_Animator.deltaPosition * m_MoveSpeedMultiplier) / Time.deltaTime;

            // we preserve the existing y part of the current velocity.
            v.y = m_Rigidbody.velocity.y;
            m_Rigidbody.velocity = v;
        }
    }

    // Not sure we need this, left it in anyway
    // Our character is always grounded anyway
    void CheckGroundStatus()
    {
        RaycastHit hitInfo;
#if UNITY_EDITOR
        // helper to visualise the ground check ray in the scene view
        Debug.DrawLine(transform.position + (Vector3.up * 0.1f), transform.position + (Vector3.up * 0.1f) + (Vector3.down * m_GroundCheckDistance));
#endif
        // 0.1f is a small offset to start the ray from inside the character
        // it is also good to note that the transform position in the sample assets is at the base of the character
        if (Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, out hitInfo, m_GroundCheckDistance))
        {
            m_GroundNormal = hitInfo.normal;
            m_IsGrounded = true;
            m_Animator.applyRootMotion = true;
        }
        else
        {
            m_IsGrounded = false;
            m_GroundNormal = Vector3.up;
            m_Animator.applyRootMotion = false;
        }
    }

    public void StateModification(PuppyCharacterController.PuppySate puppySate)
    {
        if (m_PuppyState == puppySate)
            return;

        m_PuppyState = puppySate;

        switch (m_PuppyState)
        {
            case PuppyCharacterController.PuppySate.MOVING_PLAYER:
                m_Animator.SetTrigger("movingPlayer");
                break;
            case PuppyCharacterController.PuppySate.MOVING_SOUND:
                m_Animator.SetTrigger("movingSound");
                break;
            case PuppyCharacterController.PuppySate.IDLE_PLAYER:
                break;
            default:
                m_Animator.SetTrigger("stopEmotes");
                break;
        }
    }
}

