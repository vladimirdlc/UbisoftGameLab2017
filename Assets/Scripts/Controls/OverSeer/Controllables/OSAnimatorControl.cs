using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OSAnimatorControl : MonoBehaviour, OSControllable {

    void OSControllable.triggerAction()
    {
        Debug.Log("A");
        GetComponent<Animator>().SetTrigger("toggleObject");
    }

    // Update is called once per frame
    void Update () {
		
	}
}
