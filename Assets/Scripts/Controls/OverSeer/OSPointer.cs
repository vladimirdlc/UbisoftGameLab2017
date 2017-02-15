using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class OSPointer : NetworkBehaviour
{
    public string horizontalAxis;
    public string verticalAxis;
    public string beaconButton;

    public GameObject beaconContainerPrefab;
    public GameObject beaconPrefab;
    public Transform pointer;
    public float speed = 0.1f;
    private RTSCamera cam;
    private bool beaconInUse;
    public float timeToDissapear = 3;
    private float currentTimeToDissapear;
    private bool teleportPointer;

    void Start()
    {
        GameObject pointerInstance = Instantiate(beaconContainerPrefab) as GameObject;
        pointer = pointerInstance.transform;
        cam = GetComponent<RTSCamera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetAxisRaw(verticalAxis) + Input.GetAxisRaw(horizontalAxis) == 0)
        {
            if ((currentTimeToDissapear -= Time.deltaTime) < 0)
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

        float directionx = (transform.position.x < cam.followTarget.transform.position.x) ? 1 : -1;
        float directionz = (transform.position.z < cam.followTarget.transform.position.z) ? 1 : -1;

        //Arrow Targeting
        if ((transform.rotation.y > 0.30) && (transform.rotation.y < 0.85))
        {
            pointer.position = new Vector3(pointer.position.x + (speed * Input.GetAxisRaw(verticalAxis) * directionx), pointer.position.y, pointer.position.z);
            pointer.position = new Vector3(pointer.position.x, pointer.position.y, pointer.position.z - (speed * Input.GetAxisRaw(horizontalAxis) * directionx));
        }
        else
        {
            pointer.position = new Vector3(pointer.position.x + (speed * Input.GetAxisRaw(horizontalAxis) * directionz), pointer.position.y, pointer.position.z);
            pointer.position = new Vector3(pointer.position.x, pointer.position.y, pointer.position.z + (speed * Input.GetAxisRaw(verticalAxis) * directionz));
        }

        if (Input.GetAxis(beaconButton) > 0)
        {
            if (!beaconInUse)
            {
                CmdSpawnBeacon();
            }

        }
        else if (Input.GetAxisRaw(beaconButton) == 0)
        {
            beaconInUse = false;
        }
    }

    //    [Command]
    void CmdSpawnBeacon()
    {

        var beacon = Instantiate(beaconPrefab, pointer.position, Quaternion.identity);
        beaconInUse = true;
        //       NetworkServer.Spawn(beacon);
    }
}

