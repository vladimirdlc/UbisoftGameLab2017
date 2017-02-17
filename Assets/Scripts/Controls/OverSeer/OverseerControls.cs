using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverseerControls : MonoBehaviour
{
    public string actionButtonX;
    public string actionButtonA;
    public string actionButtonB;
    public string actionButtonY;

    public List<GameObject> controllablesX;
    public List<GameObject> controllablesA;
    public List<GameObject> controllablesB;
    public List<GameObject> controllablesY;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown(actionButtonA))
        {
            triggerList(controllablesA);
            NetMessenger.Instance.CmdOpenDoor("Green");
        }
        if (Input.GetButtonDown(actionButtonX))
        {
            triggerList(controllablesX);
            NetMessenger.Instance.CmdOpenDoor("Blue");
        }
        if (Input.GetButtonDown(actionButtonB))
        {
            triggerList(controllablesB);
        }
        if (Input.GetButtonDown(actionButtonY))
        {
            triggerList(controllablesY);
        }
    }

    public void triggerList(List<GameObject> triggerContainer)
    {
        foreach (GameObject control in triggerContainer)
        {
            OSControllable trigger = control.GetComponent(typeof(OSControllable)) as OSControllable;
            trigger.triggerAction();
        }
    }
}