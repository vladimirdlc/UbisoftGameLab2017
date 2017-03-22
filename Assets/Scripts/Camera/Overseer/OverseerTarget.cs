using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverseerTarget : MonoBehaviour {
    public Transform pivot;
    public bool startingTarget;
    public static OverseerTarget startTarget;
    public static Transform currentPivot;

    [Header("Camera Settings")]
    [Tooltip("If this value is (0, 0, 0) it will not be set")]
    public Vector3 positionOffset;

    private void Awake()
    {
        if (startingTarget)
        {
            currentPivot = pivot;
            startTarget = this;

            if (GetComponent<OSCinematicTarget>())
            {
                GetComponent<OSCinematicTarget>().startCount();
            }
        }
    }

    private void Start()
    {
        if (!startTarget)
            Debug.Log("Your scene needs an starting target for the Overseer to work OK");

    }

}
