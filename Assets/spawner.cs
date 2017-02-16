using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class spawner : NetworkBehaviour
{
    [Command]
    public void CmdSpawn(GameObject o)
    {
        Debug.Log(gameObject.name);
        Debug.Log("FWIOEHDFWOEUFHOUWEFHUOEFHEHW");
        ClientScene.RegisterPrefab(o);
        NetworkServer.Spawn(o);
    }
}
