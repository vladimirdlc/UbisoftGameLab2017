using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScene : MonoBehaviour {

    public static string m_SceneToLoad;
    public static int m_ImageToLoad;

    public Animator m_CanvasAnimator;
    public Image m_ImageToChange;
    public Sprite[] m_BillboardImages;

	// Use this for initialization
	void Start () {
        m_CanvasAnimator.SetTrigger("fadeIn");
        m_ImageToChange.sprite = m_BillboardImages[m_ImageToLoad];
	}
}
