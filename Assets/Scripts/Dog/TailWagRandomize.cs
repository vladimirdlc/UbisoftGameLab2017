using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TailWagRandomize : MonoBehaviour
{

    public float m_MinSpeed;
    public float m_MaxSpeed;

    private Animator m_Animator;

    // Use this for initialization
    void Start()
    {
        m_Animator = GetComponent<Animator>();
    }

    public void NewRandomAnimationSpeed()
    {
        float newSpeed = Random.Range(m_MinSpeed, m_MaxSpeed);
        m_Animator.SetFloat("walkingTailWagSpeed", newSpeed);
    }
}
