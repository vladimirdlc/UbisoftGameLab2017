using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeLine : MonoBehaviour
{
    //hide the usual transform since it doesn't make since for UI
    new RectTransform transform;

    public float widthBetweenSeconds;
    public int intervalCount = 10;
    public Text second;
    void Start()
    {
        var timeLine = GameObject.FindGameObjectWithTag("TimeLine");
        transform = GetComponent<RectTransform>();
        widthBetweenSeconds = Screen.width / 10;
        for (int i = 0; i < intervalCount; ++i)
        {
            var temp = Instantiate(second);
            temp.transform.parent = timeLine.transform;
        }
    }
    void LateUpdate()
    {
        transform.anchoredPosition += Vector2.right * Time.deltaTime * 100;
    }
}
