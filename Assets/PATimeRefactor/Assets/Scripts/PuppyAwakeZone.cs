using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuppyAwakeZone : MonoBehaviour {

    private PuppyCharacterController m_Puppy;

    private void Start()
    {
        m_Puppy = GameObject.FindGameObjectWithTag("Puppy").GetComponent<PuppyCharacterController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            m_Puppy.m_IsAware = true;
        }
    }
}
