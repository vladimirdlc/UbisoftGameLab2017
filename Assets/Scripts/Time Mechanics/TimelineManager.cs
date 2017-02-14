using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimelineManager : Singleton<TimelineManager>
{
    public GameObject shadowPrefab;
    public GameObject trailRendererPrefab;

    List<GameObject> loopingShadows;
    List<float> loopingShadowStartTime;

    private float masterTimer;                  // Overall game timer clock

    // Private state variables
    private bool rewinding;

    // Use this for initialization
    void Start()
    {
        loopingShadows = new List<GameObject>();
        loopingShadowStartTime = new List<float>();
    }

    // Update is called once per frame
    void Update()
    {
        // Udate master clock when not rewinding
        if (!rewinding)
            masterTimer += Time.deltaTime;

        int i = 0;

        foreach (float shadowStartTime in loopingShadowStartTime)
        {
            if (shadowStartTime >= masterTimer)
            {

            }

            i++;
        }

        #region rewind
        // Rewind detection/implementation
        if (Input.GetButton("Rewind"))
        {
            foreach (GameObject shadow in loopingShadows)
            {
                rewinding = true;
                shadow.GetComponent<RewindableLooper>().LockUnlockRewind(true);
                shadow.GetComponent<RewindableLooper>().Rewind(Time.deltaTime);
            }
        }
        else if (Input.GetButtonUp("Rewind"))
        {
            foreach (GameObject shadow in loopingShadows)
            {
                rewinding = false;
                shadow.GetComponent<RewindableLooper>().LockUnlockRewind(false);
            }
        }
        #endregion
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
