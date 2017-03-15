﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))]
// @PIERRE: I CHANGED THE NAME OF THIS SCRIPT IF YOU HAVE PROBLEMS YOU CAN PUT IT BACK
[RequireComponent(typeof(PuppyMovement))]
public class PuppyCharacterController : MonoBehaviour {

    // Almost the same script as the CloneCharacterController.
    // The setTarget script takes care of all the logic

    // A dog landing in a timeline creates a noise that attracts the puppy
    // The puppy will runs towards a dog if he finds one on his way to the sound source
    // The clones will fire a proximity paradox event if the puppy gets too close
    // If the puppy reaches the player, then he will latch onto him

    public TimeManager m_Manager;
    public GameObject m_Player;
    
    // This should be a box collider with its origin on the floor, so that the puppy can reach it
    public GameObject m_Home;

    public NavMeshAgent m_Agent { get; private set; }

#if USING_DOG_CHARACTER
    public PuppyMovement m_Character { get; private set; }
#else
    public Character m_Character { get; private set; }
#endif

    public Vector3 m_Target { get; private set; }
    
    public Transform m_FollowTargetTransform { get; private set; }

    private Vector3 m_LastTargetPosition;

    private Transform m_Transform;

    private Animator m_Animator;

    // These two boolean variables can by used to describe all possible states of the puppy
    // isHome == false && isLactched == false 
    //      The puppy is either roaming or has reached the sound's origin
    //      In this state he will aggro and dog that he finds or head towards the sound source
    // isHome == false && isLactched == true 
    //      Will maintain the player as the target until he reaches home
    // isHome == true && isLatched == false;
    //      Puppy is in his starting state, only aggros sound
    // isHome == true && isLatched == true
    //      This is when the dog brings the puppy home
    public bool m_IsLatched { get; set; }
    public bool m_IsHome { get; set; }

    private PlayerUserController m_PlayerUserController;
    private Vector3 m_HomePosition;

    private bool m_HaltPathing;

    private void Start()
    {
        m_Agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        m_Character = GetComponent<PuppyMovement>();
        m_PlayerUserController = m_Player.GetComponent<PlayerUserController>();
        m_HomePosition = m_Home.GetComponent<Transform>().position;
        m_Target = m_HomePosition;
        m_Agent.updateRotation = true;
        m_Agent.updatePosition = true;
        m_IsHome = true;
        m_IsLatched = false;
        m_HaltPathing = false;
        m_Transform = GetComponent<Transform>();
        m_Animator = GetComponent<Animator>();
    }

    public void hearNoise(Vector3 source)
    {
        // Disregard noises if already latched
        if (m_IsLatched) return;

        m_IsHome = false;
        m_Target = source;

        // TODO: MOVE THIS SOMEWHERE ELSE USING THE PUPPY STATES
        m_Animator.SetTrigger("love");
    }

    // This is the trigger collider to aggro the puppy, it needs reworking
    // In an ideal situation, there would be a rather larger collider for the puppy that aggros
    // whatever touches it, and then the clone's collider would create a paradox if that puppy gets too close.
    // Since I'm using a single script with a single collider, those two messages get tangled together.
    // To fix this (I think) we would need a seperate script living on a seperate object with its own collider
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" || other.tag == "Clone")
        {
            // Disregard aggro if puppy is home or already latched
            if (m_IsHome || m_IsLatched) return;

            if (other.tag == "Player")
            {
                m_PlayerUserController.m_HasPuppy = true;
            }
            m_IsLatched = true;
            m_IsHome = false;
            m_FollowTargetTransform = other.GetComponent<Transform>();
        }
        if (other.tag == "Home" && m_IsLatched)
        {
            // Set destination to home
            m_IsHome = true;
            m_IsLatched = false;
            m_Target = m_HomePosition;
            m_PlayerUserController.m_HasPuppy = false;
        }
    }

    public void haltAI()
    {
        m_Agent.Stop();
        m_Agent.ResetPath();
        m_HaltPathing = true;
    }

    public void resumeAI()
    {
        m_Agent.Resume();
        m_HaltPathing = false;
    }

    public void restoreState(global::TimeManager.State state)
    {
        m_Transform.position = state.m_PuppyPosition;
        m_Transform.rotation = state.m_PuppyRotation;
        m_IsHome = state.m_PuppyIsHome;
        m_IsLatched = state.m_PuppyIsLatched;
        m_Target = state.m_PuppyTargetSound;
        m_FollowTargetTransform = state.m_PuppyTargetTransform;
    }

    private void LateUpdate()
    {
        if (m_HaltPathing) return;

        if (m_Target != null && !m_IsLatched)
        {
            m_Agent.SetDestination(m_Target);
        }

        else if (m_IsLatched)
        {
            if (m_FollowTargetTransform != null)
            {
                m_LastTargetPosition = m_FollowTargetTransform.position;
            }
            if (m_FollowTargetTransform == null)
            {
                m_IsLatched = false;
            }

            m_Agent.SetDestination(m_LastTargetPosition);
        }

        if (m_Agent.remainingDistance > m_Agent.stoppingDistance)
            m_Character.Move(m_Agent.desiredVelocity, false);

        else
            m_Character.Move(Vector3.zero, false);
    }
}
