using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomNumberCompare : MonoBehaviour {

    public static bool EqualsWithMarginOfError(float val1, float val2, float error)
    {
        return (Mathf.Abs(val1 - val2) <= error);
    }
}
