using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleToMusic : MonoBehaviour {

	public int bpm;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		float scale = 1 + Mathf.Cos(Time.time * (bpm / 60) * Mathf.PI * 2);
		scale = Mathf.Lerp (0.95f, 1.05f, Mathf.InverseLerp (0f, 2f, scale));
		transform.localScale = new Vector3(scale, scale, scale);
	}
}
