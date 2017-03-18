using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowKillerTrigger : MonoBehaviour {
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "SpawnedArrow" && 
            other.GetComponentInParent<DestroyAfterTime>().currentTimeAlive > GetComponentInParent<DestroyAfterTime>().currentTimeAlive)
        {
            Destroy(other.transform.parent.gameObject);
        }
    }
}
