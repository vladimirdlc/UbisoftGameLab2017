using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WiresCollision : MonoBehaviour {

	private Light myLight;
	public bool isActive;
	private Renderer rend;
	public Material accentRed;
	private Material[] mats;
	public Material accentRedGlow;

	// Use this for initialization
	void Start () {
		myLight = GetComponent<Light>();
		myLight.enabled = false;
		isActive = false;
		rend = GetComponent<Renderer>();
     	mats = rend.materials;
     	mats[0] = accentRed; 
     	rend.materials = mats;
	}

	void Update() {
		if(isActive) {
	     	mats[0] = accentRedGlow; 
	     	rend.materials = mats;
			isActive = true;
			myLight.enabled = true;
		}
		else {
	     	mats[0] = accentRed; 
	     	rend.materials = mats;
			isActive = false;
			myLight.enabled = false;
		}		
	}
}
