using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDog : MonoBehaviour {

    Animator anim;
    Rigidbody rb;

    public float walkingSpeed;
    public float animatorWalkingSpeed;

	// Use this for initialization
	void Start () {
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();

        anim.SetFloat("walkingSpeed", animatorWalkingSpeed);
	}
	
	// Update is called once per frame
	void Update () {
        float horizontalInput = Input.GetAxis("Horizontal Ground");
        float verticalInput = Input.GetAxis("Vertical Ground");

        rb.AddForce(new Vector3(horizontalInput, 0, verticalInput) * walkingSpeed);
	}
}
