using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OSPointer : MonoBehaviour {
    public string horizontalAxis;
    public string verticalAxis;

    public Transform pointer;
    public float speed = 0.1f;

	// Update is called once per frame
	void Update () {
        if (Input.GetAxis(horizontalAxis) > 0)
        {
            pointer.position = new Vector3(pointer.position.x+speed, pointer.position.y, pointer.position.z);
        }
        if (Input.GetAxis(horizontalAxis) < 0)
        {
            pointer.position = new Vector3(pointer.position.x - speed, pointer.position.y, pointer.position.z);
        }
        if (Input.GetAxis(verticalAxis) > 0)
        {
            pointer.position = new Vector3(pointer.position.x, pointer.position.y, pointer.position.z + speed);
        }
        if (Input.GetAxis(verticalAxis) < 0)
        {
            pointer.position = new Vector3(pointer.position.x, pointer.position.y, pointer.position.z - speed);
        }
    }
}
