using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomDebugTools : MonoBehaviour {

	public static void PrintList<T>(List<T> list)
    {
        foreach (T instance in list)
            Debug.Log(instance.ToString());
    }
}
