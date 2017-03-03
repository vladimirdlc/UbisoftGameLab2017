using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class PlayerTimeAttachment : MonoBehaviour {

    public GameObject m_TimeManager;
    public float scrubSpeed;

    private TimeManager m_Manager;
    private Character m_Player;
    public bool m_HasPuppy { get; set; }

    void Start () {
        m_Manager = m_TimeManager.GetComponent<TimeManager>();
        m_Player = gameObject.GetComponent<Character>();
        m_HasPuppy = false;
    }
	
	void Update () {

        float FF = CrossPlatformInputManager.GetAxis("FF");
        float RW = CrossPlatformInputManager.GetAxis("RW");

        // Prevent the dog from stopping time if there is a puppy with him

        if (Input.GetButtonDown("Ground Stop Time") && !m_HasPuppy)
        {
            m_Manager.timeStopToggle();
        }
        if (Input.GetButtonUp("Ground Stop Time") && !m_HasPuppy)
        {
            m_Manager.timeStopToggle();
        }

        if (FF != RW) m_Manager.masterScrub((int)((FF - RW) * scrubSpeed));

    }
    
}
