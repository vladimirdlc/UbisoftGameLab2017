using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class SceneReload : NetworkBehaviour
{

    private NetworkManager _manager;

	// Use this for initialization
	void Start ()
	{
	    _manager = FindObjectOfType<NetworkManager>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnGUI()
    {
        if (_manager != null && isServer)
        {
            if (GUI.Button(new Rect(10, 300, 100, 20), "Change Scene"))
            {
                _manager.ServerChangeScene("HlapiGameScene");
            }
        }
    }
}
