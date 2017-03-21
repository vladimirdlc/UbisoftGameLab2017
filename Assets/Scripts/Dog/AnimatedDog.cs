using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatedDog : MonoBehaviour
{

    protected Animator m_Animator;
    protected Rigidbody m_RigidBody;
    protected TimeManager m_TimeManager;

    [Header("---- Animator Variables ----")]
    public float walkAnimSpeed;
    public float sitAnimSpeed;
    public float idleAnimSpeed;
    [Header("See random tail wag speed for proper control, this one is just initial")]
    public float walkingTailWagAnimSpeed;
    public float tiltLeftAnimSpeed;
    public float tiltRightAnimSpeed;
    public float strafeSpeed;
    public float unstrafeSpeed;

    protected bool tiltLeft;
    protected bool tiltRight;

    public virtual void Start()
    {
        // Setup added references
        m_Animator = GetComponentInChildren<Animator>();
        m_TimeManager = GameObject.FindGameObjectWithTag("Time Manager").GetComponent<TimeManager>();

        // Setup animator
        m_Animator.SetFloat("walkingTailWagSpeed", walkingTailWagAnimSpeed);
        m_Animator.SetFloat("walkingSpeedMultiplier", walkAnimSpeed);
        m_Animator.SetFloat("sitSpeed", sitAnimSpeed);
        m_Animator.SetFloat("idleSpeed", idleAnimSpeed);
        m_Animator.SetFloat("strafeSpeed", strafeSpeed);
        m_Animator.SetFloat("unstrafeSpeed", unstrafeSpeed);
    }

    protected virtual void Update()
    {
        // Require reset of tilt head flags
        tiltLeft = false;
        tiltRight = false;

        if (m_TimeManager.m_GameState == TimeManager.GameState.TIME_STOPPED)
        {
            // Kill animator
            m_Animator.speed = 0;
        }
        else
        {
            m_Animator.speed = 1;
        }
    }

    protected void UpdateAnimator(float horizontal, bool sitting = false)
    {
        m_Animator.SetBool("sitting", sitting);
        m_Animator.SetBool("headTiltLeft", tiltLeft);
        m_Animator.SetBool("headTiltRight", tiltRight);

        m_Animator.SetBool("strafeLeft", horizontal <= -0.1f);
        m_Animator.SetBool("strafeRight", horizontal >= 0.1f);

        Vector3 tempVector = m_RigidBody.velocity;
        tempVector.y = 0;

        m_Animator.SetFloat("walkingSpeed", tempVector.magnitude);
        m_Animator.SetFloat("tiltLeftSpeed", tiltLeftAnimSpeed);
        m_Animator.SetFloat("tiltRightSpeed", tiltRightAnimSpeed);
    }
}
