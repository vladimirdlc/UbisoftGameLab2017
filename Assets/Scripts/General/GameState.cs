using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState : MonoBehaviour
{
    public static bool disableControls = false;

    public bool disableControlsOnAwake = true;

    private void Awake()
    {
        disableControls = disableControlsOnAwake;
    }
}
