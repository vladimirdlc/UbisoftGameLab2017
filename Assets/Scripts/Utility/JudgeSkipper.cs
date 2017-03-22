using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JudgeSkipper : MonoBehaviour
{

    private SceneSwitch m_SceneSwitch;

    // Use this for initialization
    void Start()
    {
        m_SceneSwitch = GameObject.FindGameObjectWithTag("Scene Loader").GetComponent<SceneSwitch>();
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetButtonDown("Next Level"))
        {
            m_SceneSwitch.loadScene(m_SceneSwitch.nextSceneName);
        }

        if (Input.GetButtonDown("Previous Level"))
        {
            m_SceneSwitch.loadScene(m_SceneSwitch.previousSceneName);
        }

        if (Input.GetButtonDown("Reset Level"))
        {
            m_SceneSwitch.loadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }
}
