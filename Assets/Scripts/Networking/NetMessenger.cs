using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

//All commands are called by hostsClient
//we do this by sending a message from the client
//to hostsClient which is on the other computer.
public class NetMessenger : NetworkBehaviour
{
    public static NetMessenger Instance;
    public GameObject GreenDoor;
    public GameObject BlueDoor;
    public OverseerControls o;

    private void Awake()
    {
        //Functions in this class MUST be invoked by the client and only the client
        //we are getting lucky here because the network manager is spawning
        //clientsHost and then client so this ends up being the client
        Instance = this;

        GreenDoor = GameObject.FindGameObjectWithTag("Door Green");
        //GreenDoor = GameObject.Find("DoorGreen");
        //BlueDoor = GameObject.FindGameObjectWithTag("Door Blue");
        BlueDoor = GameObject.Find("DoorBlue");
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
        {
            if (o == null)
            {
                o = GameObject.FindObjectOfType<OverseerControls>();
            }
            o.triggerList(o.controllablesA);
            //GreenDoor.GetComponent<Animator>().SetTrigger("toggleObject");
        }
        //GreenDoor.GetComponent<Animator>();
        else if (color == "Blue")
            BlueDoor.GetComponent<Animator>().SetTrigger("toggleObject");
    }

    [Command]
    public void CmdStartTimer()
    {
        GameObject.FindGameObjectWithTag("PlayerGround").GetComponent<Timer>().StartTimer();
    }
}
