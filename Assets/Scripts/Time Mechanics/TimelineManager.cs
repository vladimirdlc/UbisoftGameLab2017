using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if CUSTOM_DEBUG
using UnityEngine.UI;
#endif

public class TimelineManager : Singleton<TimelineManager>
{
    public GameObject shadowPrefab;
    public GameObject trailRendererPrefab;
    public float samplingRate;            // Once every X seconds  
    public float maxRecordingPeriod;      // Basically the maximum recording time TODO: CHECK HOW I'M DOING THIS IN THE RECORDER, TAKE THIS STATIC VALUE
    public float clockCompareMarginOfError;

    List<GameObject> loopingShadows;
    public List<float> loopingShadowStartTime;

    private float masterTimer;                  // Overall game timer clock

#if CUSTOM_DEBUG
    private Text timerText;
#endif

    // Private state variables
    private bool rewinding;

    // Use this for initialization
    void Start()
    {
        loopingShadows = new List<GameObject>();
        loopingShadowStartTime = new List<float>();

#if CUSTOM_DEBUG
        timerText = GameObject.FindGameObjectWithTag("Timer Text").GetComponent<Text>();
#endif
    }

    // Update is called once per frame
    void Update()
    {
        // Udate master clock when not rewinding
        if (!rewinding)
            masterTimer += Time.deltaTime;
        else
        {
            masterTimer -= Time.deltaTime;
            masterTimer = Mathf.Clamp(masterTimer, 0, maxRecordingPeriod);
        }

#if CUSTOM_DEBUG
        // Camera timer on screen
        if (timerText == null)
            timerText = GameObject.FindGameObjectWithTag("PlayerGround").GetComponentInChildren<Text>();

        timerText.text = masterTimer.ToString();
#endif
        //Debug.Log(masterTimer);

        int i = 0;

        #region rewind
        // Rewind detection/implementation
        if (Input.GetButton("Rewind"))
        {
            rewinding = true;

            i = 0;

            foreach (GameObject shadow in loopingShadows)
            {
                if (loopingShadowStartTime[i] > masterTimer)
                {
                    shadow.GetComponent<RewindableLooper>().LockUnlockRewind(true);
                    shadow.GetComponent<RewindableLooper>().Rewind(Time.deltaTime);
                }

                i++;
            }
        }

        if (Input.GetButtonUp("Rewind"))
        {
            rewinding = false;

            foreach (GameObject shadow in loopingShadows)
            {
                shadow.GetComponent<RewindableLooper>().LockUnlockRewind(false);
            }
        }
        #endregion

        if (!rewinding)
            foreach (float shadowStartTime in loopingShadowStartTime)
            {
                if (CustomNumberCompare.EqualsWithMarginOfError(masterTimer, shadowStartTime, clockCompareMarginOfError))
                {
                    loopingShadows[i].GetComponent<Looper>().Reloop();
                }

                i++;
            }
    }

    public void CreateShadow(List<Vector3> recordedPositions, List<Quaternion> recordedRotations, List<float> recordedTimes)
    {
        // NOTE TO SELF: CAST CONCERNS?
        GameObject shadow = (GameObject)Instantiate(shadowPrefab);

        GameObject trail = (GameObject)Instantiate(trailRendererPrefab, shadow.transform);
        trail.transform.localPosition = new Vector3(0, 0, 0);

        loopingShadows.Add(shadow);
        loopingShadowStartTime.Add(masterTimer);

        shadow.GetComponent<Looper>().StartLooping(recordedPositions, recordedRotations, recordedTimes);
    }
}
