using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.ImageEffects;
using UnityEngine.UI;
using UnityEngine;

public class PressStart : MonoBehaviour {

	public bool onTitleScreen;
	public bool doTransition;
	public Image m_targetImage;
	public Image m_targetImage2;
	public Animator[] m_Animators;
	public AudioClip menuBut;
	AudioSource audio;
	public bool isInteractable = true;
	public bool doFlag = true;

	void Awake () {
		QualitySettings.vSyncCount = 0;
		Application.targetFrameRate = 60;
		Cursor.visible = false;
	}

	// Use this for initialization
	void Start () {
		audio = GetComponent<AudioSource>();
		onTitleScreen = true;
		doTransition = false;
		if(!PlayerPrefs.HasKey("SensitivityY"))
			PlayerPrefs.SetFloat("SensitivityY", 0.4f);
		if(!PlayerPrefs.HasKey("SensitivityX"))
			PlayerPrefs.SetFloat("SensitivityX", 0.4f);
		if(!PlayerPrefs.HasKey("InvertY"))
			PlayerPrefs.SetInt("InvertY", 0);
	}
	
	// Update is called once per frame
	void Update () {
		if(isInteractable && Input.GetButtonDown("Confirm")) {
			onTitleScreen = false;
		}

		if(!onTitleScreen) {
			GetComponent<Blur>().iterations-=0.25f;
			Color c = m_targetImage.color;
			c.a -= 1f * Time.deltaTime;
			if(c.a < 1 && doFlag) {
				if(!doTransition)
				audio.PlayOneShot(menuBut, 0.7F);
				doTransition = true;	
				foreach(Animator anim in m_Animators)
				{
					anim.SetTrigger("doTranslate");
				}
				doFlag = false;
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