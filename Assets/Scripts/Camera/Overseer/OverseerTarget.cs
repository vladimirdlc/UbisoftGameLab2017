using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverseerTarget : MonoBehaviour {
    public Transform pivot;
    public bool startingTarget;
    public static OverseerTarget startTarget;
    public static Transform currentPivot;

    private void Awake()
    {
        if (startingTarget)
        {
            currentPivot = pivot;
            startTarget = this;
        }
    }

}
