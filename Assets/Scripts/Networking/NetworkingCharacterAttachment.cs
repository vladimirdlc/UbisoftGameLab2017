﻿//#define NETWORKING
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

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

    //Add the main level names to this array (level 1, level 2, etc.)
    private string[] sceneLevelNames = { "Level 1 Final Net", "Level 2 Final", "Level 3 Final", "Level 4 Final", "Level 5 Final", "Level 6 Final" };

    /// <summary>
    /// When inheriting from this class, override the start method and add this base
    /// at the end of the child Start overriding method
    /// </summary>
    bool goodLevel = false;

    protected virtual void Start()
    {
        foreach (string levelName in sceneLevelNames)
        {
            if (SceneManager.GetActiveScene().name == levelName)
                goodLevel = true;

        }
        if (!goodLevel)
        {
            gameObject.SetActive(false);
            goodLevel = false;
        }

        server = NetworkCustom.isServer;

        clientsHost = !isLocalPlayer && !server; //Server-Replica in Client
        client = !server && isLocalPlayer;
        host = server && isLocalPlayer;
        hostsClient = server && !isLocalPlayer; //Client-Replica in Server

        if (hostsClient)
        {
            gameObject.SetActive(false);
            gameObject.name = "hostsClient";
        }

        //host is the DOG
        if (host)
        {
            BothHostAndClientsHost();

            //Below is for the tutorials
            var groundTutCan = GameObject.FindGameObjectsWithTag("Tutorial Canvas Ground Player");
            ChangeToDisplay1(groundTutCan);
            groundTutCan = GameObject.FindGameObjectsWithTag("CanvasGroundCharacter");
            ChangeToDisplay1(groundTutCan);

            var overseerTutCan = GameObject.FindGameObjectsWithTag("Tutorial Canvas Overseer");
            Deactivate(overseerTutCan);
            overseerTutCan = GameObject.FindGameObjectsWithTag("CanvasOverseer");
            Deactivate(overseerTutCan);
            //ends here

            GameObject overseer = GameObject.FindGameObjectWithTag("Overseer");

            //Might be a good idea to use a different tag for this
            var overseerCamera = GameObject.FindGameObjectWithTag("MainCamera");
            overseerCamera.SetActive(false);

            var lucky = GameObject.FindGameObjectWithTag("Camera Ground Character");

            //for some reason 0 changes display to 1
            lucky.GetComponent<Camera>().targetDisplay = 0;

            if (!overseer || !overseerCamera || !lucky)
                print("ERROR: We have a missing tag for networking");



            if (overseer != null)
                GameObject.FindGameObjectWithTag("Overseer").SetActive(false);

            gameObject.name = "host";
        }


        if (client)
        {
            gameObject.SetActive(false);
            gameObject.name = "client";
        }

        //The client host disables components he should not have
        //this should probably be on a seperate script
        if (clientsHost)
        {
            //we are probably getting lucky here since it finds the first 
            //object tagged which in this case is clientsHost
            BothHostAndClientsHost();

            //Below is for the tutorials
            var overseerTutCan = GameObject.FindGameObjectsWithTag("Tutorial Canvas Overseer");
            ChangeToDisplay1(overseerTutCan);
            overseerTutCan = GameObject.FindGameObjectsWithTag("CanvasOverseer");
            ChangeToDisplay1(overseerTutCan);

            var groundTutCan = GameObject.FindGameObjectsWithTag("Tutorial Canvas Ground Player");
            Deactivate(groundTutCan);
            groundTutCan = GameObject.FindGameObjectsWithTag("CanvasGroundCharacter");
            Deactivate(groundTutCan);
            //ends here

            TimeManager.disableCameraForOverseer = true;
            var lucky = GameObject.FindGameObjectWithTag("Camera Ground Character");
            lucky.GetComponent<Camera>().enabled = false;
            gameObject.name = "clientsHost";
        }
    }

    void ChangeToDisplay1(GameObject[] changeMe)
    {
        foreach (var can in changeMe)
        {
            //0 MEANS DISPLAY 1
            can.GetComponent<Canvas>().targetDisplay = 0;
        }
    }

    void Deactivate(GameObject[] deactivateMe)
    {
        foreach (var can in deactivateMe)
        {
            can.SetActive(false);
        }
    }

    void BothHostAndClientsHost()
    {

        var timeManager = GameObject.FindGameObjectWithTag("Time Manager").GetComponent<TimeManager>();
        timeManager.enabled = true;

        var player = GameObject.FindGameObjectWithTag("Camera Ground Character");
        timeManager.m_PlayerCamera = player.GetComponent<Camera>();
        player.GetComponent<AudioListener>().enabled = true;

        var puppy = GameObject.FindGameObjectWithTag("Puppy").GetComponent<PuppyCharacterController>();
        puppy.enabled = true;
        puppy.m_TimeManager = timeManager;

        if (!timeManager || !player || !puppy)
            print("ERROR: We have a missing tag for networking");
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

    protected void GetInput(ref bool pressed)
    {
        if (host)
            pressed = Input.GetButtonDown("Test Button");
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

        if (client)
            result = true;


        if (buttonInputFlag && client)
        {
            buttonInputFlagOfClientsHost = true;
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
