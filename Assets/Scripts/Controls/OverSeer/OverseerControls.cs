using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverseerControls : MonoBehaviour {
    public string actionButtonX;
    public string actionButtonA;
    public string actionButtonB;
    public string actionButtonY;

    public List<GameObject> controllablesX;
    public List<GameObject> controllablesA;
    public List<GameObject> controllablesB;
    public List<GameObject> controllablesY;

    // Update is called once per frame
    void Update () {
        if (Input.GetButtonDown(actionButtonA))
        {
            foreach(GameObject control in controllablesA)
            {
                OSControllable trigger = control.GetComponent(typeof(OSControllable)) as OSControllable;
                trigger.triggerAction();
            }
        }
        if (Input.GetButtonDown(actionButtonX))
        {
            foreach (GameObject control in controllablesX)
            {
                OSControllable trigger = control.GetComponent(typeof(OSControllable)) as OSControllable;
                trigger.triggerAction();
            }
        }
        if (Input.GetButtonDown(actionButtonB))
        {
            foreach (GameObject control in controllablesB)
            {
                OSControllable trigger = control.GetComponent(typeof(OSControllable)) as OSControllable;
                trigger.triggerAction();
            }
        }
        if (Input.GetButtonDown(actionButtonY))
        {
            foreach (GameObject control in controllablesY)
            {
                OSControllable trigger = control.GetComponent(typeof(OSControllable)) as OSControllable;
                trigger.triggerAction();
            }
        }
    }
}