using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;

public class NetworkMessage : MessageBase
{
    public int chosenClass;
}

public class NC2 : NetworkManager
{
    public GameObject ground;
    public GameObject god;

    public override void OnClientConnect(NetworkConnection conn)
    {
        NetworkMessage test = new NetworkMessage();
        test.chosenClass = 1;
        Debug.Log("Called");
        ClientScene.Ready(conn);
        if (test.chosenClass == 0)
            ClientScene.RegisterPrefab(ground);
        else
            ClientScene.RegisterPrefab(god);
        ClientScene.AddPlayer(conn, 0, test);
    }

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId, NetworkReader extraMessageReader)
    {
        print("DSDS");
        NetworkMessage message = extraMessageReader.ReadMessage<NetworkMessage>();
        int mes = message.chosenClass;
        //if (mes == 0)
        //{
        //    base.OnServerAddPlayer(conn, playerControllerId);
        //}
        //else
        if (mes == 1)
            NetworkServer.Spawn(god);
        else
        {
            GameObject player = Instantiate(mes == 0 ? ground : god) as GameObject;
            NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
        }
    }
}
