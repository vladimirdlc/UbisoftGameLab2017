using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorSounds : MonoBehaviour {

    public AudioClip m_Closing;
    public AudioClip m_Timer;

    public AudioSource m_AS { get; private set;}

    private void Start()
    {
        m_AS = GetComponent<AudioSource>();
    }

    public void close()
    {
        m_AS.PlayOneShot(m_Closing);
    }

    public void timer(int seconds)
    {
        m_AS.clip = m_Timer;
        m_AS.time = 5 - seconds;
        m_AS.Play();
    }

}
