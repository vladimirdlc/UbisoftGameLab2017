using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

namespace AdvanceRTSCameraDemo
{

    public class GUIVector3 : MonoBehaviour
    {

        public InputField[] vectorInputs;
        public bool clampInput;
        public float minInput;
        public float maxInput;
        public event EventHandler<VectorEventArgs> vectorChanged;

        private Vector3 vectorValue;

        public Vector3 VectorValue
        {
            get { return vectorValue; }
            set
            {
                vectorValue = value;
                vectorInputs[0].text = vectorValue.x.ToString();
                vectorInputs[1].text = vectorValue.y.ToString();
                vectorInputs[2].text = vectorValue.z.ToString();
                ValidateVector();
            }
        }

        public void ValidateVector()
        {
            float x = 0, y = 0, z = 0;
            // Only set the text if the float is parsed over maxInput or under minINput
            // Otherwise if the user inputs "0." it will parse back to 0
            // and never allow the user to input any decimal values
            if (float.TryParse(vectorInputs[0].text, out x) && clampInput && (x > maxInput || x < minInput))
            {
                x = Mathf.Clamp(x, minInput, maxInput);
                vectorInputs[0].text = x.ToString();
            }

            if (float.TryParse(vectorInputs[1].text, out y) && clampInput && (y > maxInput || y < minInput))
            {
                y = Mathf.Clamp(y, minInput, maxInput);
                vectorInputs[1].text = y.ToString();
            }

            if (float.TryParse(vectorInputs[2].text, out z) && clampInput && (z > maxInput || z < minInput))
            {
                z = Mathf.Clamp(z, minInput, maxInput);
                vectorInputs[2].text = z.ToString();
            }

            vectorValue = new Vector3(x, y, z);
            if (vectorChanged != null)
                vectorChanged(this, new VectorEventArgs(vectorValue));
        }


    }

    public class VectorEventArgs : EventArgs
    {
        public Vector3 Vector
        {
            get;
            private set;
        }

        public VectorEventArgs(Vector3 vector)
        {
            this.Vector = vector;
        }
    }

}