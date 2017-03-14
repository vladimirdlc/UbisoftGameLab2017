using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteGameobject : MonoBehaviour {

    public GameObject objectToDestroy;

	public void Delete()
    {
        Destroy(objectToDestroy);
    }

    public void Hide()
    {
        objectToDestroy.SetActive(true);
    }
}
