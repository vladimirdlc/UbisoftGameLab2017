using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OSAnimatorControl : MonoBehaviour, OSControllable {

    void OSControllable.triggerAction()
    {
        GetComponent<Animator>().SetTrigger("toggleObject");
    }
}
