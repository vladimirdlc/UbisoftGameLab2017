using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OSPointer : MonoBehaviour
{
    public string horizontalAxis;
    public string verticalAxis;
    public string beaconButton;

    public bool autoHidePointer;

    [Tooltip("This will be added to the starting position of the arrow")]
    public Vector3 pointerInstantiateOffset;            

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

    void Start()
    {
        GameObject pointerInstance = Instantiate(beaconContainerPrefab) as GameObject;
        pointer = pointerInstance.transform;
        cam = GetComponent<RTSCamera>();
        overseerCam = GetComponent<OverseerCamera>();
    }

    public void updateTarget()
    {
        pointer.transform.position = cam.followTarget.transform.position + pointerInstantiateOffset;
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
            pointer.position = cam.followTarget.position + pointerInstantiateOffset;
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
        pointer.position = new Vector3(pointer.position.x, overseerCam.target.transform.position.y, pointer.position.z) + pointerInstantiateOffset;
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

