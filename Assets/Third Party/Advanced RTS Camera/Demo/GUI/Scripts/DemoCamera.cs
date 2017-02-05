using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

namespace AdvanceRTSCameraDemo
{
    public class DemoCamera : MonoBehaviour
    {

        public RTSCamera rtsCamera;
        public GUIVector3 directionVector;
        public GUIVector3 multiplierVector;

        public void Start()
        {
            // Set Vector values
            directionVector.VectorValue = rtsCamera.directionToMove;
            multiplierVector.VectorValue = rtsCamera.directionMultiplier;

            // Setup events
            directionVector.vectorChanged += DirectionChanged;
            multiplierVector.vectorChanged += MultiplierChanged;
        }

        #region Controls

        private void DirectionChanged(object sender, VectorEventArgs args)
        {
            rtsCamera.directionToMove = args.Vector;
        }

        private void MultiplierChanged(object sender, VectorEventArgs args)
        {
            rtsCamera.directionMultiplier = args.Vector;
        }

        public bool Vertical
        {
            get { return rtsCamera.verticalSetup == RTSCamera.ControlSetup.Axis; }
            set
            {
                if (value)
                    rtsCamera.verticalSetup = RTSCamera.ControlSetup.Axis;
                else
                    rtsCamera.verticalSetup = RTSCamera.ControlSetup.Disabled;
            }
        }

        public bool Horizontal
        {
            get { return rtsCamera.horizontalSetup == RTSCamera.ControlSetup.Axis; }
            set
            {
                if (value)
                    rtsCamera.horizontalSetup = RTSCamera.ControlSetup.Axis;
                else
                    rtsCamera.horizontalSetup = RTSCamera.ControlSetup.Disabled;
            }
        }

        public bool Rotate
        {
            get { return rtsCamera.rotateSetup == RTSCamera.ControlSetup.Axis; }
            set
            {
                if (value)
                    rtsCamera.rotateSetup = RTSCamera.ControlSetup.KeyCode;
                else
                    rtsCamera.rotateSetup = RTSCamera.ControlSetup.Disabled;
            }
        }

        public bool Tilt
        {
            get { return rtsCamera.tiltSetup == RTSCamera.ControlSetup.Axis; }
            set
            {
                if (value)
                    rtsCamera.tiltSetup = RTSCamera.ControlSetup.KeyCode;
                else
                    rtsCamera.tiltSetup = RTSCamera.ControlSetup.Disabled;
            }
        }

        public bool Orbit
        {
            get { return rtsCamera.orbitSetup == RTSCamera.ControlSetup.Axis; }
            set
            {
                if (value)
                    rtsCamera.mouseXSetup = RTSCamera.MiddleMouseSetup.Orbit;
                else
                    rtsCamera.mouseXSetup = RTSCamera.MiddleMouseSetup.Disabled;
            }
        }

        public void SetDirection(GUIVector3 vector)
        {
            rtsCamera.directionToMove = vector.VectorValue;
        }

        #region Middle Mouse

        public bool LocksCursor
        {
            set { rtsCamera.middleMouseLocksMouse = value; }
        }

        public bool InvertMouseX
        {
            set { rtsCamera.invertMouseX = value; }
        }

        public bool InvertMouseY
        {
            set { rtsCamera.invertMouseY = value; }
        }

        public string MouseXControls
        {
            set
            {
                RTSCamera.MiddleMouseSetup setup = (RTSCamera.MiddleMouseSetup)Enum.Parse(typeof(RTSCamera.MiddleMouseSetup), value, true);
                rtsCamera.mouseXSetup = setup;
            }
        }

        public string MouseYControls
        {
            set
            {
                RTSCamera.MiddleMouseSetup setup = (RTSCamera.MiddleMouseSetup)Enum.Parse(typeof(RTSCamera.MiddleMouseSetup), value, true);
                rtsCamera.mouseYSetup = setup;
            }
        }

        #endregion

        #endregion

        #region Behaviour

        public string AutoAdjustState
        {
            set
            {
                RTSCamera.AutoAdjustState state = (RTSCamera.AutoAdjustState)Enum.Parse(typeof(RTSCamera.AutoAdjustState), value, true);
                rtsCamera.autoAdjustState = state;
            }
        }

        public string AdjustToTilt
        {
            set
            {
                int tilt;
                if (int.TryParse(value, out tilt))
                    rtsCamera.terrainAdjustTilt = tilt;
            }
        }

        public bool SmoothMovement
        {
            set
            {
                rtsCamera.smoothPosition = value;
            }
        }

        public bool SmoothTilt
        {
            set
            {
                rtsCamera.smoothTilt = value;
            }
        }

        public bool SmoothRotate
        {
            set
            {
                rtsCamera.smoothRotate = value;
            }
        }

        public string SmoothMovementTime
        {
            set
            {
                int smooth;
                if (int.TryParse(value, out smooth))
                    rtsCamera.movementAdjustSpeed = smooth;
            }
        }

        public string SmoothTiltTime
        {
            set
            {
                int smooth;
                if (int.TryParse(value, out smooth))
                    rtsCamera.tiltAdjustSpeed = smooth;
            }
        }

        public string SmoothRotateTime
        {
            set
            {
                int smooth;
                if (int.TryParse(value, out smooth))
                    rtsCamera.rotateAdjustSpeed = smooth;
            }
        } 

        #endregion

    }

}