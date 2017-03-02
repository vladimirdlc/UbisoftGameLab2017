using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    public GameObject mesh;
    public Material active;
    public Material inactive;

    public GameObject target;

    private bool isActivated;

    void Start()
    {
        mesh.GetComponent<Renderer>().material = inactive;
        isActivated = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isActivated) return;
        target.GetComponent<Door>().IncCount();
        mesh.GetComponent<Renderer>().material = active;
        isActivated = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!isActivated) return;
        target.GetComponent<Door>().DecCount();
        mesh.GetComponent<Renderer>().material = inactive;
        isActivated = false;
    }
}
