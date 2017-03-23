using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverseerFOV : MonoBehaviour {
    public string zoomIn = "Overseer LB";
    public string zoomOut = "Overseer RB";
    public Camera[] cameras;

    public float zoomSpeed = 5;
    public float minFOV = 30;
    public float maxFOV = 90;

    void Update()
    {
        float zoomType = 0;
        if (Input.GetButton(zoomIn))
        {
            zoomType += 1;
        }
        if (Input.GetButton(zoomOut))
        {
            zoomType -= 1;
        }

        // assign zoom value to a variable
        float delta = zoomType * -zoomSpeed * Time.deltaTime;
        if (zoomType != 0)
        {
            foreach (Camera c in cameras)
            {
                // make sure the current FOV is within min/max values
                if ((c.fieldOfView + delta > minFOV) &&
                  (c.fieldOfView + delta < maxFOV))
                {
                    // apply the change to the main camera
                    c.fieldOfView = c.fieldOfView + delta;
                }
            }
        }
    }
}
