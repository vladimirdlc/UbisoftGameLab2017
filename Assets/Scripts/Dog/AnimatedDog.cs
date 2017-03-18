using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatedDog : MonoBehaviour
{

    protected Animator m_Animator;
    protected Rigidbody m_RigidBody;

    [Header("---- Animator Variables ----")]
    public float walkAnimSpeed;
    public float sitAnimSpeed;
    [Header("See random tail wag speed for proper control, this one is just initial")]
    public float walkingTailWagAnimSpeed;
    public float tiltLeftAnimSpeed;
    public float tiltRightAnimSpeed;

    protected bool tiltLeft;
    protected bool tiltRight;

    public virtual void Start()
    {
        // Setup added references
        m_Animator = GetComponentInChildren<Animator>();

        // Setup animator
        m_Animator.SetFloat("walkingTailWagSpeed", walkingTailWagAnimSpeed);
    }

    protected virtual void Update()
    {
        // Require reset of tilt head flags
        tiltLeft = false;
        tiltRight = false;
    }

    protected void UpdateAnimator(bool sitting = false)
    {
        m_Animator.SetFloat("walkingSpeedMultiplier", walkAnimSpeed);
        m_Animator.SetFloat("sitSpeed", sitAnimSpeed);

        m_Animator.SetBool("sitting", sitting);
        m_Animator.SetBool("headTiltLeft", tiltLeft);
        m_Animator.SetBool("headTiltRight", tiltRight);

        Vector3 tempVector = m_RigidBody.velocity;
        tempVector.y = 0;
        m_Animator.SetFloat("walkingSpeed", tempVector.magnitude);
        m_Animator.SetFloat("tiltLeftSpeed", tiltLeftAnimSpeed);
        m_Animator.SetFloat("tiltRightSpeed", tiltRightAnimSpeed);
    }
}
