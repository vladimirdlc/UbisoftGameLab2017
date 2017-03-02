using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))]
public class CloneTimeAttachment : MonoBehaviour
{

    // This script acts as a paradox monitor. Calls the Time Manager
    // to handle paradoxes as they occur

    // The m_BlockingParadoxRange field sets how far the clone must be
    // from his target destination in order to assume that something is
    // blocking its path. There is a monitoring field in the CloneCharacterController
    // that can be checked at runtime to get an idea of what is a good value to put here.
    // If the clones seem to be triggering blocking paradoxes for no reason, than that's the
    // value that needs to be changed.

    public GameObject timeManagerObject;

    public TimeManager manager { get; set; }
    public int timelineID { get; set; }

    public float m_BlockingParadoxRange;

    public UnityEngine.AI.NavMeshAgent m_Agent { get; private set; }

    private Transform m_Transform;

    private void Awake()
    {
        m_Transform = GetComponent<Transform>();
        m_Agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
    }

    // Late to make sure that any disabling script has time to run
    private void LateUpdate()
    {
        if (m_Agent.pathPending) return;
        // Blocking Paradox
        if (m_Agent.remainingDistance > m_BlockingParadoxRange && m_Agent.remainingDistance != Mathf.Infinity)
        {
            manager.handleParadox(timelineID, m_Transform);
        }
    }

    // Proximity Paradox
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" || other.tag == "Puppy")
        {
            manager.handleParadox(timelineID);
        }


    }
}
