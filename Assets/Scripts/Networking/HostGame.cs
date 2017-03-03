using UnityEngine;
using UnityEngine.Networking;

public class HostGame : MonoBehaviour
{

    private NetworkManager netManager;

    void Start()
    {
        netManager = NetworkManager.singleton;
        gameObject.GetComponent<NetworkManagerHUD>().showGUI = false;
    }

    public void PlayGameHost()
    {
        netManager.StartHost();
    }
}
