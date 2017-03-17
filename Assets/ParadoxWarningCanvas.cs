using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParadoxWarningCanvas : MonoBehaviour
{

    public float textFlashSpeed;
    public float warningDuration;

    private Animator m_Aniamtor;
    private float timer;
    private bool started;

    // Use this for initialization
    void Start()
    {
        m_Aniamtor = GetComponent<Animator>();
        started = false;
    }

    private void Update()
    {
        if (started)
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

    /// <summary>
    /// Starts coroutine for the paradox warning message. The time variable is optional in case you want to override
    /// the public warning time specified at the canvas.
    /// </summary>
    /// <param name="time">Optional parameter to override the warning time specified on the canvas</param>
    /// <returns></returns>
    public IEnumerator StartWarningAsCoroutine(float time = 0.0f)
    {
        // Ignore timer implementation
        if (!started)
        {
            float usedTime;

            if(time > 0.0f)
            {
                usedTime = time;
            }
            else
            {
                usedTime = this.warningDuration;
            }

            m_Aniamtor.SetBool("isWarning", true);
            yield return new WaitForSeconds(usedTime);
            m_Aniamtor.SetBool("isWarning", false);
        }
    }
}
