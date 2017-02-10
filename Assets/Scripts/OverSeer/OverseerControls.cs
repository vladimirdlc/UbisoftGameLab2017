using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverseerControls : MonoBehaviour {
    public string actionButtonX;
    public string actionButtonA;
    public string actionButtonB;
    public string actionButtonY;

	// Update is called once per frame
	void Update () {
        if (Input.GetButtonDown(actionButtonA))
        {
            Debug.Log("A");
        }
        if (Input.GetButtonDown(actionButtonX))
        {
            Debug.Log("X");
        }
        if (Input.GetButtonDown(actionButtonB))
        {
            Debug.Log("B");
        }
        if (Input.GetButtonDown(actionButtonY))
        {
            Debug.Log("Y");
        }
    }
}