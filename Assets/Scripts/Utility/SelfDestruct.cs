using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDestruct : MonoBehaviour {

    public float selfDesctructCountDown;
    public bool onlyDeactivate;

    private float timer;

	// Use this for initialization
	void Start () {
        timer = selfDesctructCountDown;
	}
	
	// Update is called once per frame
	void Update () {
        timer -= Time.deltaTime;

        if (timer <= 0)
            if (onlyDeactivate)
                gameObject.SetActive(false);
            else
                Destroy(gameObject);
	}
}
