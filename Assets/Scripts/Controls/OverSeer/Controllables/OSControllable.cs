using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class OSControllable : MonoBehaviour {

    public abstract void TriggerAction();

    protected void TriggerAnimator()
    {
        GetComponent<Animator>().SetTrigger("toggleObject");
    }
}
