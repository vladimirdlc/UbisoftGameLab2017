using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class spawner : NetworkBehaviour
{

    public GameObject beaconPrefab;
    [Command]
    public void CmdSpawn(Vector3 position)
    //public void CmdSpawn(ref GameObject )
    {
        Debug.Log(gameObject.name);
        Debug.Log("FWIOEHDFWOEUFHOUWEFHUOEFHEHW");
        //ClientScene.RegisterPrefab(o);
        var beacon = Instantiate(beaconPrefab, position, Quaternion.identity);
        //NetworkServer.Spawn(beacon);
    }

}
