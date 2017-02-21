using System;
using UnityEngine;

/// <summary>
/// This is the same Camera PivotBasedCameraRig of the Stardard Assets but with the CrossPlatform Input Maganger Removed
/// </summary>
namespace MalbersAnimations
{
    public abstract class PivotBasedCameraRig : AbstractTargetFollower
    {
        protected Transform m_Cam; // the transform of the camera
        protected Transform m_Pivot; // the point at which the camera pivots around
        protected Vector3 m_LastTargetPosition;
        protected virtual void Awake()
        {
            // find the camera in the object hierarchy
            m_Cam = GetComponentInChildren<Camera>().transform;
            m_Pivot = m_Cam.parent;
        }
    }
}
