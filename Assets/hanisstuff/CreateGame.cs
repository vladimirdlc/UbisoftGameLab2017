using UnityEngine;
using UnityEngine.Networking;

public class CreateGame : NetworkDiscovery
{
    private NetworkManager networkManager;
    //public NetworkDiscovery networkDiscovery;
    private float timeOut = 2f;
    void Start()
    {
        Initialize();
        networkManager = NetworkManager.singleton;
        //why this works?????? new does not work if part of monobeha??
        //networkDiscovery = this;
        //networkDiscovery.Initialize();
        //networkDiscovery.StartAsClient();
        //networkDiscovery.StartAsServer();
        StartAsClient();
    }

    bool noMoreUpdate = false;
    //YOU NEED LATEUPDATE HERE, OFCOURSE UNITY DOCUMENTATION DOESNT SAY THAT:
    //http://answers.unity3d.com/questions/1214729/onreceivedbroadcast-not-being-called-with-network.html
    void LateUpdate()
    {
        if (noMoreUpdate)
            return;
        timeOut -= Time.deltaTime;
        if (timeOut < 0)
        {
            //networkDiscovery.StopBroadcast();
            StopBroadcast();
            networkManager.StartHost();
            //networkDiscovery.Initialize();
            Initialize();
            //networkDiscovery.broadcastData = "WORKKKK";
            //networkDiscovery.StartAsServer();
            StartAsServer();
            noMoreUpdate = true;
        }
    }
    public override void OnReceivedBroadcast(string fromAddress, string data)
    {
        base.OnReceivedBroadcast(fromAddress, data);
        Debug.Log("sfdesdf");
        //if (noMoreUpdate)
        //    return;
        networkManager.networkAddress = fromAddress;
        networkManager.StartClient();
        noMoreUpdate = true;
    }
}
