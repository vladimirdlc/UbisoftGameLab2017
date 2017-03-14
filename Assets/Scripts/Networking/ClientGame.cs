using UnityEngine;
using UnityEngine.Networking;

public class ClientGame : MonoBehaviour
{

    private NetworkManager netManager;

    void Start()
    {
        netManager = NetworkManager.singleton;
        gameObject.GetComponent<NetworkManagerHUD>().showGUI = false;
    }

    public void PlayGameClient()
    {
        netManager.StartClient();
    }
}
