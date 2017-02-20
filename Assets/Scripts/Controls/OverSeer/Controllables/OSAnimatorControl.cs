using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OSAnimatorControl : MonoBehaviour {

    public void triggerAnimator()
    {
        GetComponent<Animator>().SetTrigger("toggleObject");
    }
}
