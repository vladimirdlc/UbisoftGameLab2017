using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.ImageEffects;
using UnityEngine;

public class MenuSelection : MonoBehaviour {

	public GameObject[] children;
	public GameObject selector;
	private int selection;
	private bool m_isAxisInUse = false;
	public Material matBlack;
	public Material matWhite;
	public bool isInteractable = false;
	private bool isCredits = false;
	private bool isOptions = false;
	private int creditsCounter = 0;
	private int optionsCounter = 0;
	private tvScreen[] tvScripts;
	public Animation animn;

	// Use this for initialization
	void Start () {
		selection = 0;
		tvScripts = GameObject.Find("Console Screen").GetComponents<tvScreen>();
	}
	
	void Update()
	{	
		if(GameObject.Find("Main Camera").transform.position.z == 3.15f) {
			GameObject.Find("ScreenCamera").GetComponent<Camera>().enabled = true;
			tvScripts[0].enabled = false;
			tvScripts[1].enabled = true;
			isInteractable = true;
		}
		else {
			GameObject.Find("ScreenCamera").GetComponent<Camera>().enabled = false;
			isInteractable = false;
		}

		if(GameObject.Find("Main Camera").transform.position.z == 9.712f) {
			GameObject.Find("OptionsCamera").GetComponent<Camera>().enabled = true;
		}
		else {
			GameObject.Find("OptionsCamera").GetComponent<Camera>().enabled = false;
		}

		if(isCredits) {
			Debug.Log("In Credits");
			if(Input.GetButtonDown("Cancel")) {
				GameObject.Find("Main Camera").GetComponent<Animator>().SetTrigger("doCreditsComeBack");
				isCredits = false;
				selection = 0;
	        	Vector3 jump = new Vector3(0f,-1.2f,0f);
	            selector.transform.position-=jump;
			}
		}

		if(isOptions) {
			Debug.Log("In Options");
			if(Input.GetButtonDown("Cancel")) {
				GameObject.Find("Main Camera").GetComponent<Animator>().SetTrigger("doOptionsComeBack");
				isOptions = false;
				selection = 0;
	        	Vector3 jump = new Vector3(0f,-0.6f,0f);
	            selector.transform.position-=jump;
			}
		}

		if(isInteractable && !isCredits && !isOptions) {
		if(Input.GetButtonDown("Cancel")) {
			GameObject.Find("Main Camera").GetComponent<Animator>().SetTrigger("doReturn");
			GameObject.Find("Main Camera").GetComponent<PressStart>().isInteractable = false;
			GameObject.Find("Main Camera").GetComponent<PressStart>().doFlag = true;
			GameObject.Find("Main Camera").GetComponent<PressStart>().doTransition = false;
			GameObject.Find("Main Camera").GetComponent<PressStart>().onTitleScreen = true;
			StartCoroutine(WaitThenDoThings(animn.clip.length/0.36f));
		}
		if(Input.GetButtonDown("Confirm")) {
			switch(selection) {
				//HANI
				case 0: //START
					
					break;
				case 1: //OPTIONS
					if(optionsCounter == 0) {
						GameObject.Find("Main Camera").GetComponent<Animator>().SetTrigger("doOptions");
						optionsCounter++;
						creditsCounter++;
					}
					else	
						GameObject.Find("Main Camera").GetComponent<Animator>().SetTrigger("doOptionsAgain");
					isOptions = true;
					selection = 0;
					break;
				case 2: //CREDITS
					if(creditsCounter == 0) {
						GameObject.Find("Main Camera").GetComponent<Animator>().SetTrigger("doCredits");
						creditsCounter++;
						optionsCounter++;
					}
					else	
						GameObject.Find("Main Camera").GetComponent<Animator>().SetTrigger("doCreditsAgain");
					isCredits = true;
					selection = 0;
					break;
				case 3: //QUIT
					Application.Quit();
					break;
			}
		}

		switch(selection) {
			case 0:
				children[0].GetComponent<Renderer>().material = matBlack;
				children[1].GetComponent<Renderer>().material = matWhite;
				children[2].GetComponent<Renderer>().material = matWhite;
				children[3].GetComponent<Renderer>().material = matWhite;
				break;
			case 1:
				children[0].GetComponent<Renderer>().material = matWhite;
				children[1].GetComponent<Renderer>().material = matBlack;
				children[2].GetComponent<Renderer>().material = matWhite;
				children[3].GetComponent<Renderer>().material = matWhite;
				break;
			case 2:
				children[0].GetComponent<Renderer>().material = matWhite;
				children[1].GetComponent<Renderer>().material = matWhite;
				children[2].GetComponent<Renderer>().material = matBlack;
				children[3].GetComponent<Renderer>().material = matWhite;
				break;
			case 3:
				children[0].GetComponent<Renderer>().material = matWhite;
				children[1].GetComponent<Renderer>().material = matWhite;
				children[2].GetComponent<Renderer>().material = matWhite;
				children[3].GetComponent<Renderer>().material = matBlack;
				break;
		}

	    if( Input.GetAxisRaw("Vertical") < -0.95)
	    {
	        if(m_isAxisInUse == false && selection < 3)
	        {	
	        	Vector3 jump = new Vector3(0f,-0.6f,0f);
	            selector.transform.position+=jump;
	            selection++;
	            m_isAxisInUse = true;
	        }
	    }

	    if( Input.GetAxisRaw("Vertical") > 0.95)
	    {
	        if(m_isAxisInUse == false && selection > 0)
	        {	
	        	Vector3 jump = new Vector3(0f,-0.6f,0f);
	            selector.transform.position-=jump;
	            selection--;
	            m_isAxisInUse = true;
	        }
	    }
	    if(Input.GetAxisRaw("Vertical") > -0.5 && Input.GetAxisRaw("Vertical") < 0.5)
	    {
	        m_isAxisInUse = false;
	    }   
		}
	}

	IEnumerator WaitThenDoThings(float time)
	{
		yield return new WaitForSeconds(time);
		GameObject.Find("Main Camera").GetComponent<Blur>().enabled = true;
		GameObject.Find("Main Camera").GetComponent<PressStart>().isInteractable = true;
		GameObject.Find("Main Camera").GetComponent<Blur>().iterations=3;
		GameObject.Find("Moon").GetComponent<Animator>().SetTrigger("doReturn");
		GameObject.Find("Time").GetComponent<Animator>().SetTrigger("doReturn");
		GameObject.Find("Paradogs").GetComponent<Animator>().SetTrigger("doReturn");
		GameObject.Find("PressStart").GetComponent<Animator>().SetTrigger("doReturn");
		yield break;
	}
}
