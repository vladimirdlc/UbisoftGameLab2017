using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class OSControllable : MonoBehaviour {

    public abstract void TriggerAction();

    void ToggleAnimator()
    {
        GetComponent<Animator>().SetTrigger("toggleObject");
    }
}
