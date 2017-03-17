using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class OverseerCamera : MonoBehaviour
{
    public static OverseerCamera Instance;
    public OverseerTarget target;
    public string leftAxisKey;
    public string rightAxisKey;
    public float delayTime = 3;
    public float currentDelayTime = 0;
    private RTSCamera cam;

    public string horizontalAxis;
    public string verticalAxis;

    public float speed = 0.1f;
    public float flickTotaltime;
    private float flickThresholdTime = 0.05f;

    void Start()
    {
        Instance = this;
        cam = GetComponent<RTSCamera>();
        flickTotaltime = 0;
        startCooldown();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameState.disableControls) return;

#if NETWORKING
        if (NetworkCustom.isServer)
        {
            GetComponent<Camera>().enabled = false;
            return;
        }
#endif

        if (Input.GetAxisRaw(horizontalAxis) == 0 && Input.GetAxisRaw(verticalAxis) == 0)
        {
            flickTotaltime = 0;
            if (OverseerTarget.currentPivot)
            {
                OverseerTarget.currentPivot.localPosition = Vector3.zero;
            }
        }

        float directionx = (transform.position.x < target.transform.position.x) ? 1 : -1;
        float directionz = (transform.position.z < target.transform.position.z) ? 1 : -1;


        Quaternion futureRotation = Quaternion.LookRotation(cam.followTarget.position - transform.position, Vector3.up);

        //Debug.Log(transform.rotation.y+"x:"+directionx+",z"+directionz);

        if (currentDelayTime < 0 && (Input.GetAxisRaw(horizontalAxis) != 0 || Input.GetAxisRaw(verticalAxis) != 0))
        {
            flickTotaltime += Time.deltaTime;

            Transform pivot = OverseerTarget.currentPivot;

            float savedy = pivot.transform.position.y;
            Vector3 startingPosition = pivot.position;
            Vector3 forwardScaled = cam.transform.forward * Input.GetAxis(verticalAxis);
            pivot.position += new Vector3(forwardScaled.x, 0, forwardScaled.z) * speed;
            Vector3 rigthScaled = cam.transform.right * Input.GetAxis(horizontalAxis);
            pivot.position += new Vector3(rigthScaled.x, 0, rigthScaled.z) * speed;
            pivot.position = new Vector3(pivot.position.x, savedy, pivot.position.z);

            OverseerFlickPosition flickPosition = getClosestFlick(pivot);

            if (flickTotaltime > flickThresholdTime && pivot.transform.position != Vector3.zero &&
                flickPosition.target != null)
            {
                flickTotaltime = 0;
                startCooldown();

                pivot.localPosition = Vector3.zero;
                OverseerTarget newTarget = flickPosition.target.GetComponent<OverseerTarget>();
                OverseerTarget.currentPivot = newTarget.pivot;
                if (flickPosition.overrideSmoothTime > 0)
                {
                    cam.changeTarget(flickPosition.target.transform, flickPosition.overrideSmoothTime);
                }
                else
                {
                    cam.changeTarget(flickPosition.target.transform);
                }



                target = flickPosition.target;
                GetComponent<OSPointer>().updateTarget(true);
                OverseerTarget.currentPivot.localPosition = Vector3.zero;

                return;
            }
        }


        currentDelayTime -= Time.deltaTime;
    }

    public void setNewTargetRaw(OverseerTarget newTarget, float smootTime = 0)
    {
        flickTotaltime = 0;
        startCooldown();

        if (smootTime > 0)
        {
            cam.changeTarget(newTarget.transform, smootTime);
        }
        else
        {
            cam.changeTarget(newTarget.transform);
        }

        if (newTarget.GetComponent<OSCinematicTarget>())
        {
            newTarget.GetComponent<OSCinematicTarget>().startCount();
        }
        else //Is not cinematic
        {
            GameState.disableControls = false;
            OverseerTarget.currentPivot = newTarget.pivot;
        }
   
        if (newTarget.positionOffset != Vector3.zero)
        {
            cam.CameraTargetPosition = newTarget.positionOffset;
        }

        target = newTarget;
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
