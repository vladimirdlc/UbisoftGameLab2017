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
    private RTSCamera cam;

    public string horizontalAxis;
    public string verticalAxis;

    public Transform pointer;
    public float speed = 0.1f;

    void Start()
    {
        cam = GetComponent<RTSCamera>();
    }

    // Update is called once per frame
    void Update () {
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

        float directionx = (transform.position.x < target.transform.position.x) ? 1 : -1;
        float directionz = (transform.position.z < target.transform.position.z) ? 1 : -1;

        currentDelayTime -= Time.deltaTime;
        Debug.Log(transform.rotation.y);
        //Arrow Targeting
        if ((transform.rotation.y > 0.25) && (transform.rotation.y < 0.75))
        {
            pointer.position = new Vector3(pointer.position.x + (speed * Input.GetAxisRaw(verticalAxis) * directionx), pointer.position.y, pointer.position.z);
           // pointer.position = new Vector3(pointer.position.x, pointer.position.y, pointer.position.z - (speed * Input.GetAxisRaw(horizontalAxis) * directionz));
        }
        else
        {
           // pointer.position = new Vector3(pointer.position.x - (speed * Input.GetAxisRaw(horizontalAxis) * directionz), pointer.position.y, pointer.position.z);
            pointer.position = new Vector3(pointer.position.x, pointer.position.y, pointer.position.z - (speed * Input.GetAxisRaw(verticalAxis) * directionx));
        }
        /*else if ((transform.rotation.y < -0.25 && transform.rotation.y < 0.75))
        {
            pointer.position = new Vector3(pointer.position.x +
                (speed * Input.GetAxisRaw(verticalAxis)), pointer.position.y, pointer.position.z);
        }*/
        /*else
        {
            pointer.position = new Vector3(pointer.position.x, pointer.position.y, pointer.position.z + (speed * Input.GetAxis(verticalAxis)));
        }*/
        /*else if ((transform.rotation.y > -0.25f && transform.rotation.y < -0.75))
        {
            pointer.position = new Vector3(pointer.position.x - (speed * Input.GetAxis(verticalAxis)), pointer.position.y, pointer.position.z);
        }*/


        /*else {
            pointer.position = new Vector3(pointer.position.x, pointer.position.y, pointer.position.z + (speed * Input.GetAxis(verticalAxis)));
        }*/

        /*if (Input.GetAxis(verticalAxis) > 0 && transform.rotation.y > invDegree || Input.GetAxis(verticalAxis) < 0 && transform.rotation.y < invDegree)
        {
            pointer.position = new Vector3(pointer.position.x, pointer.position.y, pointer.position.z - speed);
        }
        if (Input.GetAxis(verticalAxis) < 0 && transform.rotation.y > invDegree || Input.GetAxis(verticalAxis) > 0 && transform.rotation.y < invDegree)
        {
            pointer.position = new Vector3(pointer.position.x, pointer.position.y, pointer.position.z + speed);
        }*/

    }

    void startCooldown()
    {
        currentDelayTime = delayTime;
    }
}
