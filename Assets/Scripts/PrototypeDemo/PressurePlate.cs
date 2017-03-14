using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    public Material active;
    public Material inactive;

    public GameObject target;
    private bool m_IsActive;

    void Start()
    {
        gameObject.GetComponent<Renderer>().material = inactive;
        m_IsActive = false;
    }

    public void forceExit()
    {
        if (m_IsActive)
        {
            m_IsActive = false;
            target.GetComponent<Door>().DecCount();
            gameObject.GetComponent<Renderer>().material = inactive;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!m_IsActive && (other.tag == "Player" || other.tag == "PlayerGround" || other.tag == "Clone"))
        {
            m_IsActive = true;
            target.GetComponent<Door>().IncCount();
            gameObject.GetComponent<Renderer>().material = active;
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (m_IsActive && (other.tag == "Player" || other.tag == "PlayerGround" || other.tag == "Clone"))
        {
            m_IsActive = false;
            target.GetComponent<Door>().DecCount();
            gameObject.GetComponent<Renderer>().material = inactive;
        }

    }
}
