using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComponentCopier : MonoBehaviour {

    public static void CopyComponent<T>(ref T original, GameObject destination)
    {
        System.Type type = original.GetType();
        Component copy = destination.AddComponent(type);
        System.Reflection.FieldInfo[] fields = type.GetFields();
        foreach (System.Reflection.FieldInfo field in fields)
        {
            field.SetValue(copy, field.GetValue(original));
        }
        //return copy as T;
    }
}
