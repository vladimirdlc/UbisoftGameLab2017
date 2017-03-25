using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.ImageEffects;
using UnityEngine;

public class optionSelection : MonoBehaviour {

	public GameObject[] children;
	public GameObject selector;
	public int selection;
	private bool m_isAxisInUse = false;
	private bool m_isHAxisInUse = false;
	public Material matBlack;
	public Material matWhite;
	public bool isInteractable = false;
	private bool invertY;
	private float xSens;
	private float ySens;
	private int temp;

	// Use this for initialization
	void Start () {
		selection = 0;
		temp = PlayerPrefs.GetInt("InvertY");
		if(temp == 1) invertY = true; else invertY = false;
		if(invertY) {
			children[1].GetComponent<MeshRenderer>().enabled = false;
			children[2].GetComponent<MeshRenderer>().enabled = true;
		}
		else {
			children[1].GetComponent<MeshRenderer>().enabled = true;
			children[2].GetComponent<MeshRenderer>().enabled = false;						
		}
		xSens = PlayerPrefs.GetFloat("SensitivityX");
		ySens = PlayerPrefs.GetFloat("SensitivityY");

		float xSliderPos = Mathf.Lerp (-27f, -42f, Mathf.InverseLerp (0f, 1f, xSens));
		children[5].transform.localPosition = new Vector3(xSliderPos,children[5].transform.localPosition.y,children[5].transform.localPosition.z);
		float ySliderPos = Mathf.Lerp (-27f, -42f, Mathf.InverseLerp (0f, 1f, ySens));
		children[8].transform.localPosition = new Vector3(ySliderPos,children[8].transform.localPosition.y,children[8].transform.localPosition.z);
	}
	
	void Update()
	{	
		if(Input.GetButtonDown("Cancel")) {
			PlayerPrefs.SetFloat("SensitivityX", xSens);
			PlayerPrefs.SetFloat("SensitivityY", ySens);
			if(invertY)
			PlayerPrefs.SetInt("InvertY", 1);
			else
			PlayerPrefs.SetInt("InvertY", 0);
		}

		switch(selection) {
				case 0: //INVERT Y AXIS
				if(Input.GetButtonDown("Confirm") || (Input.GetAxisRaw("Horizontal") < -0.95 || Input.GetAxisRaw("Horizontal") > 0.95)) {
					if(!m_isHAxisInUse) {
						invertY = !invertY;
						if(invertY) {
							children[1].GetComponent<MeshRenderer>().enabled = false;
							children[2].GetComponent<MeshRenderer>().enabled = true;
						}
						else {
							children[1].GetComponent<MeshRenderer>().enabled = true;
							children[2].GetComponent<MeshRenderer>().enabled = false;						
						}
					}
				}
				break;
				case 1: //X SENSITIVITY
				if( Input.GetAxisRaw("Horizontal") < -0.95 && children[5].transform.localPosition.x < -27f)
				{
					Vector3 jump = new Vector3(0.1f,0f,0f);
					children[5].transform.localPosition+=jump;
				}

				if( Input.GetAxisRaw("Horizontal") > 0.95 && children[5].transform.localPosition.x > -42f)
				{
					Vector3 jump = new Vector3(-0.1f,0f,0f);
					children[5].transform.localPosition+=jump;
				}
				xSens = Mathf.Lerp (0f, 1f, Mathf.InverseLerp (-27f, -42f, children[5].transform.localPosition.x));
				break;
				case 2:
				if( Input.GetAxisRaw("Horizontal") < -0.95 && children[8].transform.localPosition.x < -27f)
				{
					Vector3 jump = new Vector3(0.1f,0f,0f);
					children[8].transform.localPosition+=jump;
				}

				if( Input.GetAxisRaw("Horizontal") > 0.95 && children[8].transform.localPosition.x > -42f)
				{
					Vector3 jump = new Vector3(-0.1f,0f,0f);
					children[8].transform.localPosition+=jump;
				}
				ySens = Mathf.Lerp (0f, 1f, Mathf.InverseLerp (-27f, -42f, children[8].transform.localPosition.x));
				break;

			}

			if( Input.GetAxisRaw("Vertical") < -0.95)
			{
				if(m_isAxisInUse == false && selection < 2)
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

			if(Input.GetAxisRaw("Horizontal") > 0.95 || Input.GetAxisRaw("Horizontal") < -0.95 ) {
				m_isHAxisInUse = true;
			}

			if(Input.GetAxisRaw("Vertical") > -0.5 && Input.GetAxisRaw("Vertical") < 0.5)
			{
				m_isAxisInUse = false;
			} 

			if(Input.GetAxisRaw("Horizontal") > -0.5 && Input.GetAxisRaw("Horizontal") < 0.5)
			{
				m_isHAxisInUse = false;
			}  
		}
	}
