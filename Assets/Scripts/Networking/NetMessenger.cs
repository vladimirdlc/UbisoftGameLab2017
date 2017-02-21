using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetMessenger : NetworkBehaviour
{
    public static NetMessenger Instance;
    public GameObject GreenDoor;
    public GameObject BlueDoor;


    private void Start()
    {
        Instance = this;
        GreenDoor = GameObject.FindGameObjectWithTag("Door Green");
        BlueDoor = GameObject.FindGameObjectWithTag("Door Blue");
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
        if (color == "Green")
            GreenDoor.GetComponent<Animator>().SetTrigger("toggleObject");
        else if (color == "Blue")
            BlueDoor.GetComponent<Animator>().SetTrigger("toggleObject");
        //GreenDoor.GetComponent<Animator>().SetTrigger("toggleObject");
    }
}
