﻿using UnityEngine.Networking;
using UnityEngine;

public class NetworkedInput : NetworkBehaviour
{
    //for movement
    [SyncVar]
    public float horizontal;
    [SyncVar]
    public float vertical;
    [SyncVar]
    public Vector3 v;
    //


    public float yRot;
    public float xRot;
    [SyncVar]
    public Quaternion rotn;
    [SyncVar]
    public bool crouch;
    [SyncVar]
    public float RW;
    [SyncVar]
    public float FF;
    [SyncVar]
    public bool m_BarkInput;
}
