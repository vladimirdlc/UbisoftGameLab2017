using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BoxCollider))]
public class SceneSwitch : MonoBehaviour
{
    public string sceneName;
    public bool requirePuppy = false;

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

    private void OnTriggerStay(Collider other)
    {
        Debug.Log(boundingCollider.bounds.Contains(puppyTransform.position));

        if (other.tag == "Player" || other.tag == "PlayerGround")
        {
            if (requirePuppy)
            {
                if (!boundingCollider.bounds.Contains(puppyTransform.position))
                {
                    //TriggerMessage();
                    return;
                }
            }

            //Debug.Break();

            Cursor.visible = false;
            SceneManager.LoadScene(sceneName);
        }
    }
}