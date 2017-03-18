using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkCustom : NetworkManager
{
    public GameObject groundPlayer;
    public static bool isServer;
    //public NetworkDiscovery discovery;

    public override void OnStartServer()
    {
        base.OnStartServer();
        isServer = true;
        GameObject.FindGameObjectWithTag("Time Manager").GetComponent<TimeManager>().enabled = true;
        //discovery.Initialize();
        //discovery.StartAsServer();
    }
}
