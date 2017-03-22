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

    private BoxCollider boundingCollider;
    private Transform puppyTransform;

    private void Start()
    {
        boundingCollider = GetComponent<BoxCollider>();

        if (requirePuppy)
        {
            puppyTransform = GameObject.FindGameObjectWithTag("Puppy").GetComponent<Transform>();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            loadScene(nextSceneName);
        }
        else if (Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            loadScene(previousSceneName);
        }
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
            loadScene(nextSceneName);
        }
    }

    public void loadScene(string sceneToLoad)
    {

#if NETWORKING
            UnityEngine.Networking.NetworkManager.singleton.ServerChangeScene(sceneToLoad);
#else
        SceneManager.LoadScene(sceneToLoad);
#endif

    }
}