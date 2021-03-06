﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarpBubble : MonoBehaviour {

    private Material m_Bolt;
    public int m_CurrentIndex { get; set; }

    public float m_MaxXTiling;
    public float m_MaxYTiling;

    // This must be set at the same amount as the one in TimeLineManager script, will bake it in eventually but for testing we need to set it manually
    public int m_LifeTime;

    public Transform m_Poof;


	// Use this for initialization
	void Awake () {
        m_Bolt = gameObject.GetComponent<Renderer>().material;
        m_CurrentIndex = 0;
    }
	
    public void scrub(int amount)
    {
        m_CurrentIndex += amount;
        if (m_CurrentIndex >= m_LifeTime/2)
        {
            m_CurrentIndex = Mathf.Abs(m_CurrentIndex - m_LifeTime);
        }
        float poofRotation = (float)m_CurrentIndex / m_LifeTime;
        if (m_CurrentIndex <= m_LifeTime && m_CurrentIndex >= 0)
            m_Poof.Rotate(new Vector3(0.0f, 0.0f, 1.0f), 10 * amount);
        float x = (float)m_CurrentIndex / m_LifeTime * m_MaxXTiling;
        float y = (float)m_CurrentIndex / m_LifeTime * m_MaxYTiling;
        m_Bolt.SetTextureScale("_MainTex", new Vector2(x, y));
    }

}
