using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkingCharacterAttachment : NetworkBehaviour
{
    /// <summary>
    /// Have the character controllers that are networked inherit
    /// from this class.
    /// hostsClient: Client relative to host. Client-Replica in Server
    /// client: client relative to client.
    /// clientsHost: Host relative to client. Server-Replica in Client
    /// host: the actual host relative to host
    /// </summary>
    public enum ButtonEventType { GetButtonDown, GetButtonUp, GetButton }

    //All components to disable on clients host
    [SerializeField]
    private Behaviour[] DisableOnClientsHost;

    // Use this for initialization
    private bool server;
    public bool clientsHost;
    public bool hostsClient;
    public bool client;
    public bool host;

    /// <summary>
    /// When inheriting from this class, override the start method and add this base
    /// at the end of the child Start overriding method
    /// </summary>
    protected virtual void Start()
    {
        server = NetworkCustom.isServer;

        clientsHost = !isLocalPlayer && !server; //Server-Replica in Client
        client = !server && isLocalPlayer;
        host = server && isLocalPlayer;
        hostsClient = server && !isLocalPlayer; //Client-Replica in Server

        if (host)
        {
            GameObject overseer = GameObject.FindGameObjectWithTag("Overseer");

            if (overseer != null)
                GameObject.FindGameObjectWithTag("Overseer").SetActive(false);

            gameObject.name = "host";                                               // FIXME
        }

        //QUESTION: Why we have to do this twice????
        //Deactivate client player.
        if (hostsClient)
        {
            //This only works on host side,
            //meaning if you deactivate client
            //it will only deactive client relative
            //to the host, but the client connected
            //to the host is still active.
            gameObject.SetActive(false);
            gameObject.name = "hostsClient";
        }

        //If you are the client and you are the localplayer
        //disable your camera so overseer can use his camera instead
        //also diable all unnecessary components on clientsHost
        if (client)
        {
            //gameObject.SetActive(false);
            transform.GetChild(0).GetComponent<Camera>().enabled = false;
            foreach (var comp in DisableOnClientsHost)
            {
                comp.enabled = false;
            }
            gameObject.name = "client";
            //GetComponent<TrackRenderer>().enabled = true;
        }

        //Disable other persons camera...what?
        if (clientsHost)
        {
            foreach (var comp in DisableOnClientsHost)
            {
                comp.enabled = false;
            }
            gameObject.name = "clientsHost";
        }
    }

    /// <summary>
    /// Use this to process input from the controllers.
    /// buttonInputFlag = the "buttonPressed" flag on the child character controller
    /// buttonInputFlagOfClientsHost = same as above, except a reference to the child character controller of the clientsHost
    /// TODO: provide a similar method for axis if it ever comes up
    /// </summary>
    protected bool ProcessButtonInput(ButtonEventType buttonEventType, string inputName, bool buttonInputFlag)
    {
        //Only the host (dog) is allowed to get input
        if (host)
        {
            switch (buttonEventType)
            {
                case ButtonEventType.GetButtonDown:
                    buttonInputFlag = Input.GetButtonDown(inputName);
                    break;
                case ButtonEventType.GetButtonUp:
                    buttonInputFlag = Input.GetButtonUp(inputName);
                    break;
                case ButtonEventType.GetButton:
                    buttonInputFlag = Input.GetButton(inputName);
                    break;
            }
        }
        return buttonInputFlag;
    }

    /// <summary>
    /// Second step after ProcessButtonInput, this will pass messages from the client to the clientsHost.
    /// The child CharacterController should stop execution if this function returns true.
    /// For multiple button flags, execute this function for every flag, and break when
    /// all have been processed
    /// </summary>
    protected bool CheckIfBreak(bool buttonInputFlag, ref bool buttonInputFlagOfClientsHost)
    {
        bool result = false;

        if (buttonInputFlag && client)
        {
            buttonInputFlagOfClientsHost = true;
            result = true;                       
        }

        return result;
    }

    /// <summary>
    /// After processing and using the buttonPresse bool, call input cleanup from the child character controller 
    /// for every input that was processed through the ProcessButtonInput method
    /// </summary>
    protected void ProcessButtonCleanup(ref bool buttonInputFlag)
    {
        if (clientsHost)
            buttonInputFlag = false;
    }
}
