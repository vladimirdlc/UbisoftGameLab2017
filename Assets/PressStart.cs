using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.ImageEffects;
using UnityEngine.UI;
using UnityEngine;

public class PressStart : MonoBehaviour {

	private bool onTitleScreen;
	private bool doTransition;
	public Image m_targetImage;
	public Image m_targetImage2;
	public Animator[] m_Animator;
	public AudioClip menuBut;
    AudioSource audio;

	// Use this for initialization
	void Start () {
		audio = GetComponent<AudioSource>();
		onTitleScreen = true;
		doTransition = false;
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetButtonDown("Confirm")) {
			onTitleScreen = false;
			//GetComponent<Blur>().enabled = false;
		}

		if(!onTitleScreen) {
			GetComponent<Blur>().iterations-=0.25;
			Color c = m_targetImage.color;
        	c.a -= 1f * Time.deltaTime;
        	if(c.a < 1) {
        		if(!doTransition)
        			audio.PlayOneShot(menuBut, 0.7F);
        		doTransition = true;
        		foreach(Animator anim in m_Animator)
        		{
        		anim.SetTrigger("doTranslate");
        	}
        	}
        	m_targetImage.color = c; 

			c = m_targetImage2.color;
        	c.a -= 2f * Time.deltaTime;
        	m_targetImage2.color = c; 
		}

		if(GetComponent<Blur>().iterations < 0.1)
			GetComponent<Blur>().enabled = false;
	}
}