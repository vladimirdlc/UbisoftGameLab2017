using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimelineManager : Singleton<TimelineManager> {

    public GameObject shadowPrefab;
    public GameObject trailRendererPrefab;

    List<GameObject> loopingShadows;

	// Use this for initialization
	void Start () {
        loopingShadows = new List<GameObject>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void CreateShadow(Queue<Vector3> recordedPositions, Queue<Quaternion> recordedRotations, Queue<float> recordedTimes)
    {
        // NOTE TO SELF: CAST CONCERNS?
        GameObject shadow = (GameObject) Instantiate(shadowPrefab);

        GameObject trail = (GameObject)Instantiate(trailRendererPrefab, shadow.transform);
        trail.transform.localPosition = new Vector3(0, 0, 0);

        loopingShadows.Add(shadow);

        shadow.GetComponent<Looper>().StartLooping(recordedPositions, recordedRotations, recordedTimes);
    }
}
