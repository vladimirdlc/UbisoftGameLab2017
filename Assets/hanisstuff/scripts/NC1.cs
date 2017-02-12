using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;


public class NC1 : NetworkManager
{
    public GameObject duck;
    public GameObject dog;
    public static int i = 0;
    void Start()
    {
        if (i == 0)
            NetworkManager.singleton.playerPrefab = duck;
        else
            NetworkManager.singleton.playerPrefab = dog;
        ++i;
    }
}
