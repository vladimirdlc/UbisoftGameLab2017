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
	public Animator[] m_Animators;
	public AudioClip menuBut;
	AudioSource audio;
	private Animation animn;
	private bool isInteractable = true;

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
		animn = GetComponent<Animation>();
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetButtonDown("Confirm")) {
			onTitleScreen = false;
		}

		if(!onTitleScreen) {
			GetComponent<Blur>().iterations-=0.25f;
			Color c = m_targetImage.color;
			c.a -= 1f * Time.deltaTime;
			if(c.a < 1) {
				if(!doTransition)
				audio.PlayOneShot(menuBut, 0.7F);
				doTransition = true;
				if(isInteractable) {
					//StartCoroutine(WaitThenDoThings(animn.clip.length/0.36f));
				}		
				foreach(Animator anim in m_Animators)
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

	IEnumerator WaitThenDoThings(float time)
	{
		yield return new WaitForSeconds(time);
		GameObject.Find("ScreenCamera").GetComponent<Camera>().enabled = true;
		GameObject.Find("menutext").GetComponent<MenuSelection>().isInteractable = true;
		isInteractable = false;
		yield break;
	}
}