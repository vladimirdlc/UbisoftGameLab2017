using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomCollectionManipulation : MonoBehaviour {
	
    public static List<T> RemoveElementsAfterIndex<T>(List<T> list, int index)
    {
        for (int i = index; i < list.Count; i++)
        {
            list.RemoveAt(i);
        }

        return list;
    }
}
