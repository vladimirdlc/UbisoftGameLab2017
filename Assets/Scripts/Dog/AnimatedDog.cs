using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatedDog : MonoBehaviour {

    protected Animator m_Animator;
    protected Rigidbody m_RigidBody;

    public float walkAnimSpeed;

    public virtual void Start()
    {
        // Setup added references
        m_Animator = GetComponentInChildren<Animator>();
    }

    protected void UpdateAnimator()
    {
        m_Animator.SetFloat("walkingSpeedMultiplier", walkAnimSpeed);
        Vector3 tempVector = m_RigidBody.velocity;
        tempVector.y = 0;
        m_Animator.SetFloat("walkingSpeed", tempVector.magnitude);
    }
}
