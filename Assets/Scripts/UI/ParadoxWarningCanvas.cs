using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParadoxWarningCanvas : MonoBehaviour
{

    public float textFlashSpeed;
    public float warningDuration;
    public bool enableSound;
    public AudioClip warningClip;

    private Animator m_Aniamtor;
    private AudioSource m_AudioSource;
    private float timer;
    private bool started;

    // Use this for initialization
    void Start()
    {
        m_Aniamtor = GetComponent<Animator>();
        m_AudioSource = GetComponent<AudioSource>();
        started = false;
    }

    private void Update()
    {

#if DEBUG_VERBOSE
        if (Input.GetButtonDown("Test Button"))
            StartWarning();
#endif

        if (started)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                StopWarning();
            }
        }
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

            if (time > 0.0f)
            {
                usedTime = time;
            }
            else
            {
                usedTime = this.warningDuration;
            }

            StartWarning();
            yield return new WaitForSeconds(usedTime);
            StopWarning();
        }
    }

    public void StartWarning()
    {
        started = true;
        timer = warningDuration;
        m_Aniamtor.SetBool("isWarning", true);

        if (enableSound)
            m_AudioSource.PlayOneShot(warningClip);
    }

    private void StopWarning()
    {
        started = false;
        m_Aniamtor.SetBool("isWarning", false);

        if (enableSound)
            m_AudioSource.Stop();
    }
}
