using System;
using UnityEngine;


[RequireComponent(typeof (UnityEngine.AI.NavMeshAgent))]
#if !USING_ETHAN_CHARACTER
[RequireComponent(typeof (DogCloneCharacter))]
#else
[RequireComponent(typeof (Character))]
#endif
public class CloneCharacterController : MonoBehaviour
{
    private PuppySounds m_SoundBoard;

    private AudioSource m_AudioSource;
    public UnityEngine.AI.NavMeshAgent m_Agent { get; private set; }

#if !USING_ETHAN_CHARACTER
    public DogCloneCharacter m_Character { get; private set; }
#else
    public Character m_Character { get; private set; }
#endif

    public global::TimeManager.State m_Target { get; set; }

    private TrailRenderer trail;
    TimeManager m_TimeManager;

    // Use this to tweak the value to trigger the blocking paradox
    private float maxDistance;

    private void Start()
    {
        m_SoundBoard = GetComponent<PuppySounds>();
        m_AudioSource = GetComponent<AudioSource>();
        m_Agent = GetComponent<UnityEngine.AI.NavMeshAgent>();

#if !USING_ETHAN_CHARACTER
        m_Character = GetComponent<DogCloneCharacter>();
#else
        m_Character = GetComponent<Character>();
#endif
        trail = GetComponentInChildren<TrailRenderer>();
        m_TimeManager = GameObject.FindGameObjectWithTag("Time Manager").GetComponent<TimeManager>();

        m_Agent.updateRotation = true;
	    m_Agent.updatePosition = true;

        maxDistance = 0;
    }

    public void setTarget(global::TimeManager.State target)
    {
        m_Target = target;
    }

    public void bark()
    {
        m_AudioSource.PlayOneShot(m_SoundBoard.m_Bark);
    }

    private void LateUpdate()
    {
        if (m_Target != null)
        {
            m_Agent.SetDestination(m_Target.m_DogPosition);
        }

        // Movement is self contained AKA controlled by DogCloneCharacter when scrubbing
        if (!(m_TimeManager.m_GameState == TimeManager.GameState.REWIND || m_TimeManager.m_GameState == TimeManager.GameState.FORWARD))
        {
            if (m_Agent.remainingDistance > m_Agent.stoppingDistance)
                m_Character.Move(m_Agent.desiredVelocity, false);
            else
                m_Character.Move(Vector3.zero, false);

            // Used for testing
            if (m_Agent.remainingDistance > maxDistance) maxDistance = m_Agent.remainingDistance;
        }
    }

    public void ColorCode(Color colorCode)
    {
        if(trail == null)
        {
            trail = GetComponentInChildren<TrailRenderer>();
        }

        trail.material.color = colorCode;
        trail.startColor = colorCode;
        trail.endColor = colorCode;

        MeshRenderer[] meshes = GetComponentsInChildren<MeshRenderer>();
        foreach(MeshRenderer mesh in meshes)
        {
            mesh.material.color = colorCode;
        }

        SkinnedMeshRenderer[] skMeshes = GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer mesh in skMeshes)
        {
            mesh.material.color = colorCode;
        }
    }


}

