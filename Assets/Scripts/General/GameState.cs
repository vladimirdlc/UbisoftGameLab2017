using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState : MonoBehaviour
{
    public static bool disableControls = false;

    public bool disableControlsOnAwake = true;

    public Animator[] cinematicAnimators;

    private RTSCamera rtsCamera;

    public static GameState Instance;

    private void Start()
    {
        Instance = this;
        disableControls = disableControlsOnAwake;
        rtsCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<RTSCamera>();
        rtsCamera.enabled = false;
    }

    public void onCinematicFinish()
    {
        foreach (Animator anim in cinematicAnimators)
        {
            anim.enabled = false;
        }

        rtsCamera.enabled = true;
        disableControls = false;
    }
}
