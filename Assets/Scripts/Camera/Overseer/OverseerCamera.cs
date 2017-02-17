using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class OverseerCamera : MonoBehaviour
{
    public OverseerTarget target;
    public string leftAxisKey;
    public string rightAxisKey;
    public float delayTime = 3;
    public float currentDelayTime = 0;
    private float invDegree = 0.5f;
    private RTSCamera cam;

    public string horizontalAxis;
    public string verticalAxis;

    public float speed = 0.1f;

    void Start()
    {
        cam = GetComponent<RTSCamera>();
    }

    // Update is called once per frame
    void Update () {
        if (NetworkCustom.isServer)
        {
            GetComponent<Camera>().enabled = false;
            return;
        }

        Quaternion futureRotation = Quaternion.LookRotation(cam.followTarget.position - transform.position, Vector3.up);

        if (currentDelayTime < 0)
        {
            //Debug.Log(Input.GetAxis("Vertical Right"));
            if (Input.GetButtonDown(rightAxisKey) && futureRotation.y > invDegree || Input.GetButtonDown(leftAxisKey) && futureRotation.y < invDegree)
            {
                if (target.nextTarget)
                {
                    cam.changeTarget(target.nextTarget.transform);
                    target = target.nextTarget;
                    startCooldown();
                    return;
                }
            }
            if (Input.GetButtonDown(leftAxisKey) && futureRotation.y > invDegree || Input.GetButtonDown(rightAxisKey) && futureRotation.y < invDegree)
            {
                if (target.previousTarget)
                {
                    cam.changeTarget(target.previousTarget.transform);
                    target = target.previousTarget;
                    startCooldown();
                    return;
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
