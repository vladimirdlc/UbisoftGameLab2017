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
    public Text[] requirePuppyTutorialMessages;
    public int m_TutorialPosterIndex;
    public AudioClip m_WinningFanfare;
    public GameObject m_WinningEffect;

    private BoxCollider boundingCollider;
    private Transform puppyTransform;
    private Canvas m_SceneLoadCanvas;
    private Animator m_Animator;
    private string m_SceneToLoad;
    private Transform[] m_EffectInstantiateLocations;
    private bool won = false;

    private AudioSource m_AudioSource;

    private void Start()
    {
        boundingCollider = GetComponent<BoxCollider>();
        m_Animator = GetComponent<Animator>();
        m_AudioSource = GetComponent<AudioSource>();


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
                    foreach (Text triggered in requirePuppyTutorialMessages)
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

            if (!won)
                YouWin();

            Cursor.visible = false;
            StartLoadingScene(nextSceneName);
        }
    }

    public void StartLoadingScene(string sceneToLoad)
    {
        m_Animator.SetTrigger("fadeOut");
        m_SceneToLoad = sceneToLoad;
    }

    void YouWin()
    {
        m_EffectInstantiateLocations = transform.FindChild("Effects Instantiation").GetComponentsInChildren<Transform>();
        won = true;
        m_AudioSource.PlayOneShot(m_WinningFanfare);

        foreach (Transform t in m_EffectInstantiateLocations)
        {
            Instantiate(m_WinningEffect, t.position, t.rotation);
        }
    }

    public void LoadScene()
    {
        LoadingScene.m_ImageToLoad = m_TutorialPosterIndex;
#if NETWORKING
        LoadingScene.m_SceneToLoad = m_SceneToLoad;
        UnityEngine.Networking.NetworkManager.singleton.ServerChangeScene("LoadingScene");
#else
        LoadingScene.m_SceneToLoad = m_SceneToLoad;
        SceneManager.LoadScene("LoadingScene");
#endif
    }
}