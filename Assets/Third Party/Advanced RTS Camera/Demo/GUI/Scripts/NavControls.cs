using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class NavControls : MonoBehaviour
{

    public Animator currentWindow;
    public Button selectedButton;

    private Button _currentButton;
    private Button CurrentButton
    {
        get { return _currentButton; }
        set
        {
            // Reset the current button if it exists
            if (_currentButton != null)
            {
                _currentButton.GetComponent<Image>().color = Color.white;
                _currentButton.GetComponentInChildren<Text>().color = new Color(0.196f, 0.196f, 0.196f);
            }
            // Set the new current button
            if (_currentButton != value)
            {
                _currentButton = value;
                _currentButton.gameObject.GetComponent<Image>().color = new Color(0.749f, 0.458f, 0.192f);
                _currentButton.GetComponentInChildren<Text>().color = Color.white;
            }
            else
                _currentButton = null;
        }
    }
    public Button[] buttons;

    // Use this for initialization
    void Start()
    {
        foreach (Button b in buttons)
        {
            Button bRef = b;
            b.onClick.AddListener(() =>
            {
                CurrentButton = bRef;
            });
        }

        if (currentWindow != null)
            currentWindow.SetBool("isActive", true);

        if (selectedButton != null)
            CurrentButton = selectedButton;
    }

    public void SetActive(Animator anim)
    {
        if (currentWindow != null)
            currentWindow.SetBool("isActive", false);

        if (currentWindow != anim)
        {
            anim.SetBool("isActive", true);
            currentWindow = anim;
        }
        else
            currentWindow = null;
    }

}
