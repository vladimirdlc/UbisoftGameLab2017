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
    public float flickTotaltime;
    private float flickThresholdTime = 0.05f;

    void Start()
    {
        cam = GetComponent<RTSCamera>();
        flickTotaltime = 0;
        startCooldown();
    }

    // Update is called once per frame
    void Update()
    {
#if NETWORKING
        if (NetworkCustom.isServer)
        {
            GetComponent<Camera>().enabled = false;
            return;
        }
#endif
        /* if (Input.GetAxis("Horizontal Overseer") != 0 || Input.GetAxis("Vertical Overseer") != 0)
         {
             float savedy = sphere.transform.position.y;
             Vector3 newForward = new Vector3(Input.GetAxis("Horizontal Overseer"), 0, Input.GetAxis("Vertical Overseer")).normalized;
             sphere.transform.Translate(newForward, target.transform);
             sphere.transform.localPosition = new Vector3(transform.position.x, savedy, transform.position.z);
             //transform.position += newForward
             //if (newForward != Vector3.zero) transform.forward = (transform.forward + newForward);
             //transform.Translate(Vector3.forward * Time.deltaTime);
         }
         else sphere.transform.localPosition = Vector3.zero;*/


        if (Input.GetAxisRaw("Horizontal Overseer") == 0 && Input.GetAxisRaw("Vertical Overseer") == 0)
        {
            flickTotaltime = 0;
            OverseerTarget.currentPivot.localPosition = Vector3.zero;
        }

        Quaternion futureRotation = Quaternion.LookRotation(cam.followTarget.position - transform.position, Vector3.up);

        if (currentDelayTime < 0 && (Input.GetAxisRaw("Horizontal Overseer") != 0 || Input.GetAxisRaw("Vertical Overseer") != 0))
        {
            flickTotaltime += Time.deltaTime;

            //Debug.Log("stju");
            Transform pointer = OverseerTarget.currentPivot;
            
            float savedy = pointer.transform.position.y;
            Vector3 startingPosition = pointer.position;
            Vector3 forwardScaled = cam.transform.forward * Input.GetAxis("Vertical Overseer");
            pointer.position += new Vector3(forwardScaled.x, 0, forwardScaled.z) * speed;
            Vector3 rigthScaled = cam.transform.right * Input.GetAxis("Horizontal Overseer");
            pointer.position += new Vector3(rigthScaled.x, 0, rigthScaled.z) * speed;
            pointer.position = new Vector3(pointer.position.x, savedy, pointer.position.z);

            OverseerFlickPosition flickPosition = getClosestFlick(pointer);

            if (flickTotaltime > flickThresholdTime && pointer.transform.position != Vector3.zero &&
                flickPosition.target != null)
            {
                flickTotaltime = 0;
                startCooldown();

                pointer.localPosition = Vector3.zero;
                OverseerTarget.currentPivot = flickPosition.target.GetComponent<OverseerTarget>().pivot;
                cam.changeTarget(flickPosition.target.transform);
                target = flickPosition.target;
                OverseerTarget.currentPivot.localPosition = Vector3.zero;
                return;
            }
        }


        currentDelayTime -= Time.deltaTime;
    }

    OverseerFlickPosition getClosestFlick(Transform child)
    {
        OverseerFlickPosition bestTarget = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = child.position;
        foreach (OverseerFlickPosition flickPosition in child.parent.GetComponentsInChildren<OverseerFlickPosition>())
        {
            Vector3 directionToTarget = flickPosition.transform.position - currentPosition;
            float dSqrToTarget = directionToTarget.sqrMagnitude;
            if (dSqrToTarget < closestDistanceSqr)
            {
                closestDistanceSqr = dSqrToTarget;
                bestTarget = flickPosition;
            }
        }

        return bestTarget;
    }

    void startCooldown()
    {
        currentDelayTime = delayTime;
    }
}
