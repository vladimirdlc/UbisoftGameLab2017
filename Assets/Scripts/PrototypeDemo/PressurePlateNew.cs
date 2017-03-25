using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressurePlateNew : MonoBehaviour
{

    public GameObject target;
    private Light myLight;
    private float targetPositionStart;
    private float targetPositionDown;
    private float targetPosition;
    public bool isActive;
    public GameObject[] wires;

    public AudioSource onSound;
    public AudioSource offSound;

    void Start()
    {
        myLight = GetComponent<Light>();
        myLight.enabled = false;
        targetPositionStart = transform.position.y;
        targetPositionDown = transform.position.y - 0.07f;
        targetPosition = targetPositionStart;
        isActive = false;
    }

    void Update()
    {
        if (isActive && transform.position.y > targetPosition)
        {
            Vector3 position = transform.position;
            position.y -= 0.005f;
            transform.position = position;
        }

        if (!isActive && transform.position.y < targetPosition)
        {
            Vector3 position = transform.position;
            position.y += 0.005f;
            transform.position = position;
        }
    }

    // Pierre - Required to fix a bug with clones
    public void forceExit()
    {
        if (isActive)
        {
            Door[] doors = target.GetComponentsInChildren<Door>();
            foreach (Door d in doors)
                d.DecCount();
            targetPosition = targetPositionStart;
            myLight.enabled = false;
            isActive = false;
            foreach (GameObject wire in wires)
            {
                wire.GetComponent<WiresCollision>().isActive = false;
            }

            offSound.Play();
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" || other.tag == "PlayerGround" || other.tag == "Clone")
        {
            targetPosition = targetPositionDown;
            myLight.enabled = true;
            isActive = true;
            foreach (GameObject wire in wires)
            {
                wire.GetComponent<WiresCollision>().isActive = true;
            }

            onSound.Play();
            Door[] doors = target.GetComponentsInChildren<Door>();
            foreach (Door d in doors)
                d.IncCount();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player" || other.tag == "PlayerGround" || other.tag == "Clone")
        {
                targetPosition = targetPositionStart;
            myLight.enabled = false;
            isActive = false;
            foreach (GameObject wire in wires)
            {
                wire.GetComponent<WiresCollision>().isActive = false;
            }

            Door[] doors = target.GetComponentsInChildren<Door>();
            foreach (Door d in doors)
                d.DecCount();
        }

        offSound.Play();
    }
}

