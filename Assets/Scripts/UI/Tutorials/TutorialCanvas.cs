using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialCanvas : MonoBehaviour
{
    // The order of these tutorials should resemble the order in the heirarchy
    public static string[] tutorialOptionsGroundPlayer =
    {
        "Move", "Objective", "Time Scrub", "Time Stop + Scrub", "Single Button Time Mechanics", "Require Puppy Warning", "Bark"
    };

    public static string[] tutorialOptionsOverseerPlayer =
    {
        "Move", "Beacon", "Door A", "Require Puppy Warning"
    };

    public enum PlayerType { Dog, Overseer };

    [Header("Warning: the orded of the heirarchy matters for the Tutorial triggers")]
    private Text[] textTutorials;

    private void Start()
    {
        // The objects need to be on for the assignment to happen properly
        textTutorials = GetComponentsInChildren<Text>();

        // Turn them off after they have been assigned
        foreach(Text text in textTutorials)
        {
            text.enabled = false;
        }
    }

    public virtual void TiggerTutorial(int index)
    {
        textTutorials[index].enabled = true;

        SelfDestruct script = textTutorials[index].GetComponent<SelfDestruct>();
        if(script)
        {
            script.StartSelfDestruct();
        }
    }
}
