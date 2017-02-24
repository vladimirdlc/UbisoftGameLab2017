using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OSPointer : MonoBehaviour
{
    public string horizontalAxis;
    public string verticalAxis;
    public string beaconButton;

    public bool autoHidePointer;

    public GameObject beaconContainerPrefab;
    public GameObject beaconPrefab;
    private Transform pointer;
    private float speed = 25f;
    private RTSCamera cam;
    private OverseerCamera overseerCam;
    private bool beaconInUse;
    private float timeToDissapear = 3;
    private float currentTimeToDissapear;
    private bool teleportPointer;
    private float startingY;

    void Start()
    {
        GameObject pointerInstance = Instantiate(beaconContainerPrefab) as GameObject;
        pointer = pointerInstance.transform;
        startingY = pointer.transform.position.y;
        cam = GetComponent<RTSCamera>();
        overseerCam = GetComponent<OverseerCamera>();
    }

    public void updateTarget()
    {
        pointer.transform.position = new Vector3(cam.followTarget.transform.position.x, startingY, cam.followTarget.transform.position.z);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetAxis(beaconButton) > 0)
        {
            if (!beaconInUse)
            {
                SpawnBeacon();
            }
        }
        else if (Input.GetAxisRaw(beaconButton) == 0)
        {
            beaconInUse = false;
        }

        var lookPos = cam.transform.position - pointer.position;
        lookPos.y = 0;
        var rotation = Quaternion.LookRotation(lookPos);
        pointer.rotation = rotation;

        if (Input.GetAxisRaw(verticalAxis) + Input.GetAxisRaw(horizontalAxis) == 0)
        {
            if (autoHidePointer && (currentTimeToDissapear -= Time.deltaTime) < 0)
            {
                pointer.gameObject.SetActive(false);
                teleportPointer = true;
            }
            return;
        }

        if (teleportPointer)
        {
            pointer.position = new Vector3(cam.followTarget.position.x, pointer.position.y, cam.followTarget.position.z);
            teleportPointer = false;
        }

        currentTimeToDissapear = timeToDissapear;
        pointer.gameObject.SetActive(true);
    }

    void FixedUpdate()
    {
        Vector3 startingPosition = pointer.position;
        Vector3 forwardScaled = cam.transform.forward * Input.GetAxis(verticalAxis);
        pointer.position += new Vector3(forwardScaled.x, 0, forwardScaled.z) * Time.fixedDeltaTime * speed;
        Vector3 rigthScaled = cam.transform.right * Input.GetAxis(horizontalAxis);
        pointer.position += new Vector3(rigthScaled.x, 0, rigthScaled.z) * Time.fixedDeltaTime * speed;
        pointer.position = new Vector3(pointer.position.x, startingY, pointer.position.z);
        
        if (!overseerCam.target.GetComponent<Collider>().bounds.Contains(pointer.position))
        {
            Vector3 testX = new Vector3(pointer.position.x, startingY, startingPosition.z);
            Vector3 testZ = new Vector3(startingPosition.x, startingY, pointer.position.z);

            if (overseerCam.target.GetComponent<Collider>().bounds.Contains(testX))
            {
                pointer.position = testX;
            }
            else if (overseerCam.target.GetComponent<Collider>().bounds.Contains(testZ))
            {
                pointer.position = testZ;
            }
            else
            {
                pointer.position = startingPosition;
            }
        }
    }

    void SpawnBeacon()
    {
        var beacon = Instantiate(beaconPrefab, pointer.position, Quaternion.identity);

        beaconInUse = true;

#if NETWORKING
        NetMessenger.Instance.CmdSpawn(pointer.position);
#endif
    }
}

