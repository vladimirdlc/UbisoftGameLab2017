using UnityEngine;
using UnityEngine.Networking;

public class ClientGame : MonoBehaviour
{

    private NetworkManager netManager;

    void Start()
    {
        netManager = NetworkManager.singleton;
        gameObject.GetComponent<NetworkManagerHUD>().showGUI = false;
        netManager.networkAddress = "localhost";
    }


    public void SetIpAddress(string ip)
    {
        netManager.networkAddress = ip;
    }

    public void PlayGameClient()
    {
        netManager.StartClient();
    }
}
