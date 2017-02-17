using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverseerTarget : MonoBehaviour {
    public OverseerTarget nextTarget;
    public OverseerTarget previousTarget;
    public bool startingTarget;
    public static OverseerTarget startTarget;

    private void Start()
    {
        if (startingTarget)
        {
            startTarget = this;
        }
    }

}
