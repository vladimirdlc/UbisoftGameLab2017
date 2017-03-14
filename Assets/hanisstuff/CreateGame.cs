using UnityEngine;
using UnityEngine.Networking;

public class CreateGame : NetworkDiscovery
{
    private NetworkManager networkManager;
    private float timeOut = 2f;

    void Start()
    {
        Initialize();
        networkManager = NetworkManager.singleton;
        //why this works?????? new does not work if part of monobeha??
        //networkDiscovery = this;
        StartAsClient();
    }

    //YOU NEED LATEUPDATE HERE, OFCOURSE UNITY DOCUMENTATION DOESNT SAY THAT:
    //http://answers.unity3d.com/questions/1214729/onreceivedbroadcast-not-being-called-with-network.html
    void LateUpdate()
    {
        timeOut -= Time.deltaTime;
        if (timeOut < 0)
        {
            BecomeHost();
        }
    }

    public override void OnReceivedBroadcast(string fromAddress, string data)
    {
        base.OnReceivedBroadcast(fromAddress, data);
        networkManager.networkAddress = fromAddress;
        BecomeClient();
    }

    private void BecomeHost()
    {
        StopBroadcast();
        networkManager.StartHost();
        Initialize();
        StartAsServer();
        gameObject.SetActive(false);
    }

    private void BecomeClient()
    {
        networkManager.StartClient();
        gameObject.SetActive(false);
    }
}
