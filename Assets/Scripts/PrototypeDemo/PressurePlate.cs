using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    public GameObject mesh;
    public Material active;
    public Material inactive;

    public GameObject target;

    void Start()
    {
        mesh.GetComponent<Renderer>().material = inactive;
    }

    private void OnTriggerEnter(Collider other)
    {
        target.GetComponent<Door>().IncCount();
        mesh.GetComponent<Renderer>().material = active;
    }

    private void OnTriggerExit(Collider other)
    {
        target.GetComponent<Door>().DecCount();
        mesh.GetComponent<Renderer>().material = inactive;
    }
}
