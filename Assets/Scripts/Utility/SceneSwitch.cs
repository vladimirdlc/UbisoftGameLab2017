using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BoxCollider))]
public class SceneSwitch : MonoBehaviour
{
    public string sceneName;
    public bool requirePuppy = false;

    private BoxCollider boundingCollider;
    private Collider puppyCollider;

    private void Start()
    {
        boundingCollider = GetComponent<BoxCollider>();

        if (requirePuppy)
        {
            puppyCollider = GameObject.FindGameObjectWithTag("Puppy").GetComponent<Collider>();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player" || other.tag == "PlayerGround")
        {
            if (requirePuppy)
            {
                if (!boundingCollider.bounds.Intersects(puppyCollider.bounds))
                {
                    //TriggerMessage();
                    return;
                }
            }

            Debug.Break();

            Cursor.visible = false;
            SceneManager.LoadScene(sceneName);
        }
    }
}