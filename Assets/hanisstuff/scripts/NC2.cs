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
        test.chosenClass = 0;
        print("Called");
        ClientScene.AddPlayer(conn, 1, test);
    }

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId, NetworkReader extraMessageReader)
    {
        print("DSDS");
        NetworkMessage message = extraMessageReader.ReadMessage<NetworkMessage>();
        int mes = message.chosenClass;
        GameObject player = Instantiate(mes == 0 ? ground : god) as GameObject;

        NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
    }
}
