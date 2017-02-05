using UnityEngine;
using System.Collections;

public class ExpandWindow : MonoBehaviour
{

    private static ExpandWindow _currentWindow;
    private RectTransform rectTransform;

    public RectTransform body;
    public RectTransform title;
    public float speed = 2f;


    public ExpandWindow CurrentWindow
    {
        get { return _currentWindow; }
        set { _currentWindow = value; }
    }

    public void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void Start()
    {
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, title.rect.size.y);
    }

    public void SetCurrentWindow(ExpandWindow window)
    {
        if (_currentWindow != null)
            _currentWindow.Collapse();
        if (_currentWindow == this)
        {
            _currentWindow.Collapse();
            _currentWindow = null;
        }
        else
        {
            _currentWindow = window;
            _currentWindow.Expand();
        }
    }


    public void Expand()
    {
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, body.sizeDelta.y);
    }

    public void Collapse()
    {
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, title.rect.size.y);
    }



}
