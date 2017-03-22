using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BillboardSlideshow : MonoBehaviour {

    Image m_BillboardImage;

    public Sprite[] spritesToReplace;

    int i;

	// Use this for initialization
	void Start () {
        m_BillboardImage = GetComponentInChildren<Image>();
        i = 0;
	}
	
    public void SwitchImage()
    {
        i++;

        if(i >= spritesToReplace.Length)
        {
            i = 0;
        }

        m_BillboardImage.sprite = spritesToReplace[i];
    }
}
