using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverseerCamera : MonoBehaviour {
    public OverseerTarget target;
    public string leftAxisKey;
    public string rightAxisKey;
    public float delayTime = 3;
    public float currentDelayTime = 0;
    private float invDegree = 0.5f;

	// Update is called once per frame
	void Update () {
        if (currentDelayTime < 0)
        {
            //Debug.Log(Input.GetAxis("Vertical Right"));
            if (Input.GetButtonDown(rightAxisKey) && transform.rotation.y > invDegree || Input.GetButtonDown(leftAxisKey) && transform.rotation.y < invDegree)
            {
                if (target.nextTarget)
                {
                    GetComponent<RTSCamera>().followTarget = target.nextTarget.transform;
                    target = target.nextTarget;
                    startCooldown();
                }
            }
            if (Input.GetButtonDown(leftAxisKey) && transform.rotation.y > invDegree || Input.GetButtonDown(rightAxisKey) && transform.rotation.y < invDegree)
            {
                if (target.previousTarget)
                {
                    GetComponent<RTSCamera>().followTarget = target.previousTarget.transform;
                    target = target.previousTarget;
                    startCooldown();
                }
            }
        }

        currentDelayTime -= Time.deltaTime;
    }

    void startCooldown()
    {
        currentDelayTime = delayTime;
    }
}
