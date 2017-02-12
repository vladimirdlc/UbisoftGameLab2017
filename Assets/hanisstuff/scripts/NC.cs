using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;


public class NC : NetworkManager
{
    public GameObject ground;
    public GameObject god;

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        GameObject _object = Instantiate(ground);
        //Send spawn message to client and make him the owner
        NetworkServer.AddPlayerForConnection(conn, _object, playerControllerId);
    }

    public override void OnStartClient(NetworkClient client)
    {
        //Create a custom handler for the server prefab, so we can spawn a custom client prefab
        ClientScene.RegisterSpawnHandler(ground.GetComponent<NetworkIdentity>().assetId, SpawnPlayer, UnspawnPlayer);
    }

    public GameObject SpawnPlayer(Vector3 position, NetworkHash128 assetID)
    {
        //Finally: Load the client prefab. It must NOT have a NetworkIdentity! This is important
        //Create an instance
        GameObject _object = Instantiate(god);
        //Add a NetworkIdentity
        _object.AddComponent<NetworkIdentity>();

        return _object;
    }

    void UnspawnPlayer(GameObject inObject)
    {
        Destroy(inObject);
    }
}
