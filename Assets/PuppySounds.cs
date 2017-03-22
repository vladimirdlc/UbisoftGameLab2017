using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuppySounds : MonoBehaviour
{

    public AudioClip m_TailChase;
    public AudioClip m_Paw;
    public AudioClip m_Bark;

    private AudioSource m_AudioSource;
    public AudioSource m_MovementAudiosource;

    TimeManager m_TimeManager;

    // Use this for initialization
    void Start()
    {
        m_AudioSource = GetComponent<AudioSource>();
        m_TimeManager = GameObject.FindGameObjectWithTag("Time Manager").GetComponent<TimeManager>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void TailChase()
    {
        m_AudioSource.PlayOneShot(m_TailChase);
    }

    public void Paw()
    {
        m_AudioSource.PlayOneShot(m_Paw);
    }

    public void Bark()
    {
        m_AudioSource.PlayOneShot(m_Bark);
    }

    public void MoveSound(bool play)
    {
        if (play && m_TimeManager.m_GameState == TimeManager.GameState.NORMAL)
        {
            if (!m_MovementAudiosource.isPlaying)
            {
                m_MovementAudiosource.Play();
            }
        }
        else
        {
            m_MovementAudiosource.Pause();
        }
    }
}
