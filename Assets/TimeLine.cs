using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeLine : MonoBehaviour
{
    //hide the usual transform since it doesn't make since for UI
    new RectTransform transform;

    public float widthBetweenSeconds;
    //10 intervals should mean 11 positions, starting at 0s
    public int intervalCount = 10;
    public Text second;

    void Start()
    {
        var timeLine = GameObject.FindGameObjectWithTag("TimeLine");
        transform = GetComponent<RectTransform>();
        widthBetweenSeconds = Screen.width / (float)10;
        for (int i = 0; i < intervalCount + 1; ++i)
        {
            var temp = Instantiate(second);
            temp.transform.parent = timeLine.transform;
            temp.text = i.ToString();
            temp.rectTransform.anchoredPosition = new Vector2(i * widthBetweenSeconds, 0);
        }
    }
    void LateUpdate()
    {
        transform.anchoredPosition += Vector2.right * Time.deltaTime * 100;
    }
}
