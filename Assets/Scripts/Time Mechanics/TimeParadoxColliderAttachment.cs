using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeParadoxColliderAttachment : MonoBehaviour {

    /// <summary>
    /// Attach this script as to an empty child of every object that has colliders that will serve as time paradox triggers.
    /// This script will take all colliders in the parent, duplicate them and set them to triggers. It will also set
    /// their layer to TimeParadoxCollider to trigger time paradoxes.
    /// </summary>

    public string layerName = "TimeParadoxCollider";

    // Use this for initialization
    void Start () {

        Collider[] colliders = GetComponentsInParent<Collider>();

        foreach(Collider collider in colliders)
        {
            Collider newCollider = ComponentCopier.CopyComponent(collider, gameObject);
            newCollider.isTrigger = true;
            gameObject.layer = LayerMask.NameToLayer(layerName);
        }
	}

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Clone")
        {
            Debug.Log("Time paradox trigger by " + gameObject + " on " + other.gameObject);

            other.GetComponent<Looper>().TimeParadox();
        }
            
    }
}
