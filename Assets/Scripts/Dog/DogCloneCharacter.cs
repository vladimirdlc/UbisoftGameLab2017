using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class DogCloneCharacter : AnimatedDog
{
    public float m_ScrubbingWalkAnimationSpeed;
    [SerializeField]
    float m_AnimSpeedMultiplier = 1f;

    public float m_MoveSpeed;

    // Forced scrubbing animation variables
    [Header("Point distance between absolute positions for the walking animation to play while scrubbing")]
    public float m_RewindingAnimationDistanceTreshold;

    Rigidbody m_Rigidbody;
    DogFP m_DogFP;
    PuppySounds m_DogSounds;

    // For forcing animations when rewinding
    Vector3 m_LastPosition = new Vector3();

    public override void Start()
    {
        m_DogFP = GameObject.FindGameObjectWithTag("Player").GetComponent<DogFP>();
        m_DogSounds = GetComponent<PuppySounds>();

        // Reference animator components to match the player
        walkAnimSpeed = m_DogFP.walkAnimSpeed;
        walkingTailWagAnimSpeed = m_DogFP.walkingTailWagAnimSpeed;
        idleAnimSpeed = m_DogFP.idleAnimSpeed;
        tiltLeftAnimSpeed = m_DogFP.tiltLeftAnimSpeed;
        tiltRightAnimSpeed = m_DogFP.tiltRightAnimSpeed;

        base.Start();

        m_Rigidbody = GetComponent<Rigidbody>();

        m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
    }

    public void Move(Vector3 move, bool crouch)
    {
        move *= m_MoveSpeed;

        if (Time.deltaTime > 0)
        {
            Vector3 v = move;

            // we preserve the existing y part of the current velocity.
            v.y = m_Rigidbody.velocity.y;
            m_Rigidbody.velocity = v;
        }

        m_DogSounds.MoveSound(Mathf.Abs(move.magnitude) >= 1f);

        // send input and other state parameters to the animator
        UpdateAnimator(move);
    }

    protected override void Update()
    {
        base.Update();

        // This shit is ugly AF but you gotta do what you gotta do
        if (m_TimeManager.m_GameState == TimeManager.GameState.REWIND || m_TimeManager.m_GameState == TimeManager.GameState.FORWARD)
        {
            this.UpdateAnimator(new Vector3(0, 0, 0), true);
        }
    }

    void UpdateAnimator(Vector3 move, bool scrubOverrideAnimation = false)
    {
        bool forceWalkAnimationWhileScrubbing = false;

        // If position is being modified, animate walking
        if (scrubOverrideAnimation)
        {
            if (Vector3.Distance(m_LastPosition, transform.position) >= m_RewindingAnimationDistanceTreshold)
            {
                forceWalkAnimationWhileScrubbing = true;
            }

            m_LastPosition = transform.position;
        }

        // Do not include y component of velocity for the animator
        Vector3 tempVector = m_Rigidbody.velocity;
        tempVector.y = 0;

        // update the animator parameters
        if (!scrubOverrideAnimation)
        {
            //Debug.Log("No scrub override: " + m_TimeManager.m_GameState);
            m_Animator.SetFloat("walkingSpeedMultiplier", walkAnimSpeed);
            m_Animator.SetFloat("walkingSpeed", tempVector.magnitude);
            m_Animator.SetFloat("idleSpeed", m_DogFP.idleAnimSpeed);
        }
        else
        {
            //Debug.Log("Scrub override: " + m_TimeManager.m_GameState);
            if (forceWalkAnimationWhileScrubbing)
            {
                //Debug.Log("Force walk: " + m_TimeManager.m_GameState);
                // Just force it to walk
                m_Animator.SetFloat("walkingSpeedMultiplier", m_ScrubbingWalkAnimationSpeed);
                m_Animator.SetFloat("walkingSpeed", 1f);
            }
            else
            {
                // Don't move if rewinding but not moving
                m_Animator.SetFloat("walkingSpeed", 0f);
            }
        }

        if (m_TimeManager.m_GameState == TimeManager.GameState.TIME_STOPPED)
        {
            //Debug.Log("No force walk: " + m_TimeManager.m_GameState);
            m_Animator.SetFloat("idleSpeed", 0);
            m_Animator.SetFloat("walkingSpeedMultiplier", 0);
            m_Animator.SetFloat("walkingSpeedMultiplier", 0);
            m_Animator.SetFloat("walkingSpeed", 0);
            m_Animator.SetFloat("walkingSpeed", 0);
            m_Animator.SetFloat("walkingTailWagSpeed", 0);
        }

        // Increase the overall animator speed if requiresd
        m_Animator.speed = m_AnimSpeedMultiplier;
    }
}

