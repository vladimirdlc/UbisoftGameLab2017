using System.Collections;
using System.Collections.Generic;
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
	private int creditsCounter = 0;

	// Use this for initialization
	void Start () {
		selection = 0;
	}
	
	void Update()
	{	
		if(GameObject.Find("Main Camera").transform.position.z == 3.15f) {
			GameObject.Find("ScreenCamera").GetComponent<Camera>().enabled = true;
			isInteractable = true;
		}
		else {
			GameObject.Find("ScreenCamera").GetComponent<Camera>().enabled = false;
			isInteractable = false;
		}

		if(isCredits) {
			Debug.Log("In Credits");
			if(Input.GetButtonDown("Confirm")) {
				GameObject.Find("Main Camera").GetComponent<Animator>().SetTrigger("doCreditsComeBack");
				isCredits = false;
				selection = 0;
	        	Vector3 jump = new Vector3(0f,-1.2f,0f);
	            selector.transform.position-=jump;
			}
		}

		if(isInteractable && !isCredits) {
		if(Input.GetButtonDown("Confirm")) {
			switch(selection) {
				case 2:
					if(creditsCounter == 0) {
						GameObject.Find("Main Camera").GetComponent<Animator>().SetTrigger("doCredits");
						creditsCounter++;
					}
					else	
						GameObject.Find("Main Camera").GetComponent<Animator>().SetTrigger("doCreditsAgain");
					isCredits = true;
					selection = 0;
					break;
				case 3:
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
}
