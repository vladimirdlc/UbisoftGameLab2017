using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomMathf : MonoBehaviour {

    public static Vector3 Vector3Lerp(Vector3 vectorA, Vector3 vectorB, float t)
    {
        Vector3 result = new Vector3();

        result.x = Mathf.Lerp(vectorA.x, vectorB.x, t);
        result.y = Mathf.Lerp(vectorA.y, vectorB.y, t);
        result.z = Mathf.Lerp(vectorA.z, vectorB.z, t);

        return result;
    }

    public static Quaternion QuaternionLerp(Quaternion quaternionA, Quaternion quaternionB, float t)
    {
        Quaternion result = new Quaternion();

        result.x = Mathf.Lerp(quaternionA.x, quaternionB.x, t);
        result.y = Mathf.Lerp(quaternionA.y, quaternionB.y, t);
        result.z = Mathf.Lerp(quaternionA.z, quaternionB.z, t);
        result.w = Mathf.Lerp(quaternionA.w, quaternionB.w, t);

        return result;
    }
}
