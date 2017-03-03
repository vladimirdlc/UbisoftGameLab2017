using System;
using UnityEngine;


[RequireComponent(typeof (UnityEngine.AI.NavMeshAgent))]
[RequireComponent(typeof (Character))]
public class CloneCharacterController : MonoBehaviour
{
    public UnityEngine.AI.NavMeshAgent m_Agent { get; private set; }
    public Character m_Character { get; private set; }

    public global::TimeManager.State m_Target { get; set; }

    private TrailRenderer trail;

    // Use this to tweak the value to trigger the blocking paradox
    private float maxDistance;

    private void Start()
    {
        m_Agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        m_Character = GetComponent<Character>();
        trail = GetComponentInChildren<TrailRenderer>();

	    m_Agent.updateRotation = true;
	    m_Agent.updatePosition = true;

        maxDistance = 0;
    }

    public void setTarget(global::TimeManager.State target)
    {
        m_Target = target;
    }

    private void LateUpdate()
    {
        if (m_Target != null)
        {
            m_Agent.SetDestination(m_Target.m_DogPosition);
        }

        if (m_Agent.remainingDistance > m_Agent.stoppingDistance)
            m_Character.Move(m_Agent.desiredVelocity, false);
        else
            m_Character.Move(Vector3.zero, false);

        // Used for testing
        if (m_Agent.remainingDistance > maxDistance) maxDistance = m_Agent.remainingDistance;
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

