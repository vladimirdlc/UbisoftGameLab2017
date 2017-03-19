using UnityEngine.Networking;
using UnityEngine;

public class NetworkedInput : NetworkBehaviour
{
#if NETWORKING
 [SyncVar]
#endif
    public float vertical;

}
