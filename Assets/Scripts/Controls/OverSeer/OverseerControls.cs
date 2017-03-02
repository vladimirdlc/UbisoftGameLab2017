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
#if NETWORKING
            NetMessenger.Instance.CmdOpenDoor("Green");
#endif
        }
        if (Input.GetButtonDown(actionButtonX))
        {
            triggerList(controllablesX);
#if NETWORKING
            NetMessenger.Instance.CmdOpenDoor("Blue");
#endif
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
            // For single controllables...
            if (control.gameObject.layer == LayerMask.NameToLayer("OSControllable"))
            {
                OSControllable trigger = control.GetComponent(typeof(OSControllable)) as OSControllable;
                trigger.TriggerAction();
            }
            //... or for controllable gameObjects that have multiple nested controllable gameObjects as children
            else if (control.gameObject.layer == LayerMask.NameToLayer("OSControllable Nested"))
            {
                OSControllable[] nestedTriggers = control.GetComponentsInChildren<OSControllable>();

                foreach(OSControllable trigger in nestedTriggers)
                {
                    trigger.TriggerAction();
                }
            }
        }
    }
}