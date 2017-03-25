using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScene : MonoBehaviour {

    public static string m_SceneToLoad;
    public static int m_ImageToLoad;

    public Animator m_CanvasAnimator;
    public Image m_ImageToChange;
    public Sprite[] m_BillboardImages;
    public float posterDisplayTime;

    private float timer;
    private bool fading = false;

	// Use this for initialization
	void Start () {
        m_CanvasAnimator.SetTrigger("fadeIn");
        m_ImageToChange.sprite = m_BillboardImages[m_ImageToLoad];
        timer = posterDisplayTime;
	}

    private void Update()
    {
        timer -= Time.deltaTime;

        if(timer <= 0 && !fading)
        {
            fading = true;
            m_CanvasAnimator.SetTrigger("fadeOut");
        }
    }

    public void LoadScene()
    {
#if NETWORKING
            UnityEngine.Networking.NetworkManager.singleton.ServerChangeScene(m_SceneToLoad);
#else
        SceneManager.LoadScene(m_SceneToLoad);
#endif
    }
}
