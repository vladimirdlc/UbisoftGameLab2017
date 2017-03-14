using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class SceneSwitch : MonoBehaviour
{
    public string sceneName;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" || other.tag== "PlayerGround")
        {
            Cursor.visible = false;
            SceneManager.LoadScene(sceneName);
        }
    }
}