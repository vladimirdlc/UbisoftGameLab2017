using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomCollectionManipulation : MonoBehaviour {
	
    public static List<T> RemoveElementsAfterIndex<T>(List<T> list, int index)
    {

        list.RemoveRange(index,list.Count - index);

        /* This is wrong and I am dumb
        for (int i = index; i < list.Count; i++)
        {
            list.RemoveAt(index);
        }
        /*/

        return list;
    }
}
