using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverseerCamera : MonoBehaviour {
    public OverseerTarget target;
    public string axisKey;
    public float delayTime = 3;
    public float currentDelayTime = 0;
    private float invDegree = 0.5f;

	// Update is called once per frame
	void Update () {
        if (currentDelayTime < 0)
        {
            Debug.Log(transform.rotation.y);
            if (Input.GetAxis(axisKey) > 0 && transform.rotation.y > invDegree || Input.GetAxis(axisKey) < 0 && transform.rotation.y < invDegree)
            {
                if (target.nextTarget)
                {
                    GetComponent<RTSCamera>().followTarget = target.nextTarget.transform;
                    target = target.nextTarget;
                    startCooldown();
                }
            }
            if (Input.GetAxis(axisKey) < 0 && transform.rotation.y > invDegree || Input.GetAxis(axisKey) > 0 && transform.rotation.y < invDegree)
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
