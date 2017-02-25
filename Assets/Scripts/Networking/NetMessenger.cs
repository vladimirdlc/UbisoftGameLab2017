using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetMessenger : NetworkBehaviour
{
    public static NetMessenger Instance;
    public GameObject GreenDoor;
    //public GameObject BlueDoor;


    private void Start()
    {
        Instance = this;
        //GreenDoor = GameObject.FindGameObjectWithTag("Door Green");
        //GreenDoor = GameObject.Find("DoorGreen");
        Debug.Log(GreenDoor);
        Debug.Log("FREEEEEEEferferf");
        if (GreenDoor == null)
            Debug.Break();
        //BlueDoor = GameObject.FindGameObjectWithTag("Door Blue");
    }

    public GameObject beaconPrefab;
    [Command]
    public void CmdSpawn(Vector3 position)
    //public void CmdSpawn(ref GameObject )
    {
        //ClientScene.RegisterPrefab(o);
        var beacon = Instantiate(beaconPrefab, position, Quaternion.identity);
        //NetworkServer.Spawn(beacon);
    }


    [Command]
    public void CmdOpenDoor(string color)
    {
        var io = GreenDoor.transform;
        if (color == "Green")
            GreenDoor.GetComponent<Animator>();
        //GreenDoor.GetComponent<Animator>().SetTrigger("toggleObject");
        //else if (color == "Blue")
        //    BlueDoor.GetComponent<Animator>().SetTrigger("toggleObject");
    }
}
