using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script will make a UI element always face the given camera
/// </summary>
public class Billboard : MonoBehaviour
{
    public Camera m_camera;
    public bool m_toggleUpSignCorrection;

    void Update()
    {
        //transform.LookAt(Camera.main.transform.position, -Vector3.up * (m_toggleUpSignCorrection ? -1 : 1));
        Vector3 fwd = m_camera.transform.forward;
        fwd.y = 0;
        transform.rotation = Quaternion.LookRotation(fwd);
    }
}
