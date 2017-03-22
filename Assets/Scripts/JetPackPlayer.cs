using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JetPackPlayer : MonoBehaviour {

    public AudioSource m_JetPack;

    public float m_Volume;

    public enum Phase
    {
        RUN,
        LOOP,
        END,
        STOP
    }

    public Phase m_Phase { get; set; }

    private int m_LoopStart;
    private int m_LoopEnd;

	// Use this for initialization
	void Start () {

        m_LoopStart = 6126;
        m_LoopEnd = 78494;

        m_JetPack.volume = m_Volume;
	}
	
	// Update is called once per frame
	void Update () {
		
        switch (m_Phase)
        {
            case Phase.RUN:
                if (!m_JetPack.isPlaying)
                {
                    m_JetPack.Play();
                }
                else
                {
                    if (m_JetPack.timeSamples >= m_LoopStart)
                    {
                        m_Phase = Phase.LOOP;
                    }
                }
                break;

            case Phase.LOOP:
                if (m_JetPack.timeSamples > m_LoopEnd - 1000)
                {
                    m_JetPack.timeSamples = m_LoopStart;
                }
                break;

            case Phase.STOP:
                if (m_JetPack.isPlaying)
                {
                    m_Phase = Phase.END;
                }
                break;

            case Phase.END:
                if (m_JetPack.timeSamples < m_LoopEnd)
                {
                    m_JetPack.timeSamples = m_LoopEnd;
                }
                break;
        }

	}
}
