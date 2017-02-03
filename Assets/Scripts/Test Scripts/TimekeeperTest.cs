using Chronos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimekeeperTest : MonoBehaviour {

    public float localTimeScale;

    Clock clock;

	// Use this for initialization
	void Start () {
        clock = Timekeeper.instance.Clock("Test");
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetButtonDown("Test Button"))
        {
            clock.localTimeScale = localTimeScale;
        }
	}
}
