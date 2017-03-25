using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuppyAwakeZone : MonoBehaviour {

    public PuppyCharacterController m_Puppy;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            m_Puppy.m_IsAware = true;
        }
    }
}
