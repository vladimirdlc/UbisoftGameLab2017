using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatedDog : MonoBehaviour {

    protected Animator m_Animator;
    protected Rigidbody m_RigidBody;

    [Header("---- Animator Variables ----")]
    public float walkAnimSpeed;
    public float sitAnimSpeed;

    public virtual void Start()
    {
        // Setup added references
        m_Animator = GetComponentInChildren<Animator>();
    }

    protected void UpdateAnimator(bool sitting = false)
    {
        m_Animator.SetFloat("walkingSpeedMultiplier", walkAnimSpeed);
        m_Animator.SetFloat("sitSpeed", sitAnimSpeed);
        m_Animator.SetBool("sitting", sitting);

        Vector3 tempVector = m_RigidBody.velocity;
        tempVector.y = 0;
        m_Animator.SetFloat("walkingSpeed", tempVector.magnitude);
    }
}
