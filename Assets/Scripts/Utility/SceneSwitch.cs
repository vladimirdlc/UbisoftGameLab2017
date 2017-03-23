using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(BoxCollider))]
public class SceneSwitch : MonoBehaviour
{
    public string previousSceneName;
    public string nextSceneName;
    public bool requirePuppy = false;
    public Text[] tutorialMessages;
    public int m_TutorialPosterIndex;

    private BoxCollider boundingCollider;
    private Transform puppyTransform;
    private Canvas m_SceneLoadCanvas;
    private Animator m_Animator;
    private string m_SceneToLoad;

    private void Start()
    {
        boundingCollider = GetComponent<BoxCollider>();
        m_Animator = GetComponent<Animator>();

        if (requirePuppy)
        {
            puppyTransform = GameObject.FindGameObjectWithTag("Puppy").GetComponent<Transform>();
        }
    }

    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.KeypadPlus))
        //{
        //    loadScene(nextSceneName);
        //}
        //else if (Input.GetKeyDown(KeyCode.KeypadMinus))
        //{
        //    loadScene(previousSceneName);
        //}
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player" || other.tag == "PlayerGround")
        {
            if (requirePuppy)
            {
                if (!boundingCollider.bounds.Contains(puppyTransform.position))
                {
                    // Trigger warning message
                    foreach (Text triggered in tutorialMessages)
                    {
                        triggered.enabled = true;
                        SelfDestruct script = triggered.gameObject.GetComponent<SelfDestruct>();
                        if (script)
                        {
                            script.StartSelfDestruct();
                        }
                    }

                    return;
                }
            }

            Cursor.visible = false;
            StartLoadingScene(nextSceneName);
        }
    }

    public void StartLoadingScene(string sceneToLoad)
    {
        m_Animator.SetTrigger("fadeOut");
        m_SceneToLoad = sceneToLoad;
    }

    public void LoadScene()
    {
        LoadingScene.m_ImageToLoad = m_TutorialPosterIndex;
#if NETWORKING
            LoadingScene.m_SceneToLoad = m_SceneToLoad;
            UnityEngine.Networking.NetworkManager.singleton.ServerChangeScene("LoadingScreen");
#else
        LoadingScene.m_SceneToLoad = m_SceneToLoad;
        SceneManager.LoadScene("LoadingScene");
#endif
    }
}