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
    private bool isActive;

    void Start()
    {
        myLight = GetComponent<Light>();
        myLight.enabled = false;
        targetPositionStart = transform.position.y;
        targetPositionDown = transform.position.y-0.07f;
        targetPosition = targetPositionStart;
        isActive = false;
    }

    void Update ()
    {    
        if(isActive && transform.position.y > targetPosition) {
            Vector3 position = transform.position;
            position.y -= 0.005f;
            transform.position = position;
        }

        if(!isActive && transform.position.y < targetPosition) {
            Vector3 position = transform.position;
            position.y += 0.005f;
            transform.position = position;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player" || other.tag == "PlayerGround" || other.tag == "Clone")
        {   
            targetPosition = targetPositionDown;
            myLight.enabled = true;
            isActive = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Light off");
        if (other.tag == "Player" || other.tag == "PlayerGround" || other.tag == "Clone")
        {
            targetPosition = targetPositionStart;
            myLight.enabled = false;
            isActive = false;
            target.GetComponent<Door>().DecCount();
        }

    }
}
