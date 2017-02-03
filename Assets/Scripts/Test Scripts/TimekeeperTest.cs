using Chronos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimekeeperTest : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetButtonDown("Test Button"))
        {
            GetComponent<GlobalClock>().localTimeScale = 0.5f;
        }
	}
}
