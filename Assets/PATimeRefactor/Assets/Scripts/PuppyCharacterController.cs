using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))]

#if !USING_ETHAN_CHARACTER
[RequireComponent(typeof(PuppyMovement))]
#else
[RequireComponent(typeof(Character))]
#endif
public class PuppyCharacterController : MonoBehaviour
{
    public enum PuppySate
    {
        IDLE_HOME,
        IDLE_PLAYER,
        IDLE_SOUND_SOURCE,
        MOVING_SOUND,
        MOVING_PLAYER
    }
    public bool m_IsAware { get; set; }

    // Almost the same script as the CloneCharacterController.
    // The setTarget script takes care of all the logic

    // A dog landing in a timeline creates a noise that attracts the puppy
    // The puppy will runs towards a dog if he finds one on his way to the sound source
    // The clones will fire a proximity paradox event if the puppy gets too close
    // If the puppy reaches the player, then he will latch onto him

    public TimeManager m_TimeManager;
    public GameObject m_Player;

    public PuppySate m_PuppyState { get; private set; }

    // This should be a box collider with its origin on the floor, so that the puppy can reach it
    public GameObject m_Home;

    public NavMeshAgent m_Agent { get; private set; }

#if !USING_ETHAN_CHARACTER
    public PuppyMovement m_Character { get; private set; }
#else
    public Character m_Character { get; private set; }
#endif

    public Vector3 m_Target { get; private set; }

    public Transform m_FollowDogTransform { get; private set; }

    private Vector3 m_LastDogPosition;

    private Transform m_Transform;

    private Animator m_Animator;

    public bool m_LatchToClones;

    private PlayerUserController m_PlayerUserController;
    private Vector3 m_HomePosition;

    private bool m_HaltPathing;

    private void Awake()
    {
#if NETWORKING
        enabled = false;
#endif
    }

    private void Start()
    {
        m_Home = GameObject.FindGameObjectWithTag("Home");
        m_PuppyState = PuppySate.IDLE_HOME;
        m_Agent = GetComponent<UnityEngine.AI.NavMeshAgent>();

#if !USING_ETHAN_CHARACTER
        m_Character = GetComponent<PuppyMovement>();
#else
        m_Character = GetComponent<Character>();
#endif

        m_Player = GameObject.FindGameObjectWithTag("Player");
        m_PlayerUserController = m_Player.GetComponent<PlayerUserController>();
        m_HomePosition = m_Home.GetComponent<Transform>().position;
        m_Target = m_HomePosition;
        m_Agent.updateRotation = true;
        m_Agent.updatePosition = true;
        m_HaltPathing = false;
        m_Transform = GetComponent<Transform>();
        m_Animator = GetComponent<Animator>();
        m_IsAware = false;
    }

    // There are two ways to assign a destination
    // Through noise, or dog positions
    // Noises sets the target once
    // Dog positions are updated on LateUpdate()
    public void hearNoise(Vector3 source)
    {
        // Disregard unless game state is NORMAL
        if (m_TimeManager.m_GameState != TimeManager.GameState.NORMAL)
            return;


        // Disregard noises if already latched
        if (m_PuppyState == PuppySate.MOVING_PLAYER || !m_IsAware)
            return;

        m_PuppyState = PuppySate.MOVING_SOUND;
        m_Character.StateModification(m_PuppyState);

        // Set agent target to sound source
        m_Target = source;
        m_Agent.SetDestination(m_Target);
        
    }

    private void OnTriggerEnter(Collider other)
    {
        // Disregard unless game state is NORMAL
        if (m_TimeManager.m_GameState != TimeManager.GameState.NORMAL)
            return;

        // Disable aggro on collider when puppy is following a player/clone
        if (m_PuppyState == PuppySate.MOVING_PLAYER)
            return;

        if (other.tag == "Player" || (m_LatchToClones && other.tag == "Clone"))
        {
            // For safety, even though isAware should be true by now
            m_IsAware = true;

            if (other.tag == "Player")
            {
                m_PlayerUserController.m_HasPuppy = true;
            }

            // Set state to moving and fetch the last know position (if the players manage to warp out in a single frame)
            m_FollowDogTransform = other.GetComponent<Transform>();

            m_LastDogPosition = m_FollowDogTransform.position;
            m_Agent.SetDestination(m_LastDogPosition);

            m_PuppyState = PuppySate.MOVING_PLAYER;
            m_Character.StateModification(m_PuppyState);
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

    public void warpOutTarget()
    {
        m_FollowDogTransform = null;
    }

    public void restoreState(global::TimeManager.State state)
    {
        m_Transform.position = state.m_PuppyPosition;
        m_Transform.rotation = state.m_PuppyRotation;
        m_Target = state.m_PuppyTarget;
        m_FollowDogTransform = state.m_PuppyTargetTransform;
        m_PuppyState = state.m_PuppyState;
        m_IsAware = state.m_PuppyAware;
    }

    private void LateUpdate()
    {
        // Don't issue any move commands if puppy AI is halted
        if (m_HaltPathing) return;

        // The only destination(target) that requires updating is when a dog is the agent's target
        if (m_PuppyState == PuppySate.MOVING_PLAYER || m_PuppyState == PuppySate.IDLE_PLAYER)
        {
            // If dog warps out, move to last know position and revert to noise finding state
            if (m_FollowDogTransform == null)
            {
                m_Target = m_LastDogPosition;
                m_PuppyState = PuppySate.MOVING_SOUND;
                m_Character.StateModification(m_PuppyState);
            }
            // Otherwise target dog and update last know position
            else
            {
                m_LastDogPosition = m_FollowDogTransform.position;
                m_Target = m_FollowDogTransform.position;
            }

            m_Agent.SetDestination(m_Target);
        }

        // @Jesus - I'm not modifiying the state in your script when going from IDLE_PLAYER to MOVING_PLAYER and vis versa
        if (m_Agent.remainingDistance > m_Agent.stoppingDistance)
        {
            m_Character.Move(m_Agent.desiredVelocity, false);

            if (m_PuppyState == PuppySate.IDLE_PLAYER)
            {
                m_PuppyState = PuppySate.MOVING_PLAYER;
            }
        }
        else
        {
            m_Character.Move(Vector3.zero, false);

            if (m_PuppyState == PuppySate.MOVING_PLAYER)
            {
                m_PuppyState = PuppySate.IDLE_PLAYER;
            }
            else if (m_PuppyState == PuppySate.MOVING_SOUND)
            {
                m_PuppyState = PuppySate.IDLE_SOUND_SOURCE;
                m_Character.StateModification(m_PuppyState);
            }
        }
    }
}
