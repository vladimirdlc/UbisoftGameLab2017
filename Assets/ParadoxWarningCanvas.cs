using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParadoxWarningCanvas : MonoBehaviour {

    public float textFlashSpeed;
    public float warningDuration;

    private Animator m_Aniamtor;
    private float timer;
    private bool started;

	// Use this for initialization
	void Start () {
        m_Aniamtor = GetComponent<Animator>();
        started = false;
	}

    private void Update()
    {
        if (Input.GetButtonDown("Test Button"))
            StartWarning();

        if(started)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                m_Aniamtor.SetBool("isWarning", false);
                started = false;
            }
        }
    }
    public void StartWarning()
    {
        started = true;
        timer = warningDuration;
        m_Aniamtor.SetBool("isWarning", true);
    }
}
