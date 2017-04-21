using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Multidisplay : MonoBehaviour {

    private static bool mCheckDone = false;

	// Use this for initialization
	void Start () {

        if (!mCheckDone)
        {
            // Display.displays[0] is the primary, default display and is always ON.
            // Check if additional displays are available and activate each.
            if (Display.displays.Length > 1)
                Display.displays[1].Activate();

            mCheckDone = false;
        }
    }
}
