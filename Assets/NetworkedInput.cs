using UnityEngine.Networking;
using UnityEngine;

public class NetworkedInput : NetworkBehaviour
{
    [SyncVar]
    public float horizontal;
    [SyncVar]
    public float vertical;
    public float yRot;
    public float xRot;
    [SyncVar]
    public Quaternion rotn;
}
