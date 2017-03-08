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
    private float midpoint;
    private List<Text> secondsOnScreen;
    private int latestSecond;

    void Start()
    {
        latestSecond = intervalCount;
        midpoint = Screen.width / 2;
        secondsOnScreen = new List<Text>();
        var timeLine = GameObject.FindGameObjectWithTag("TimeLine");
        transform = GetComponent<RectTransform>();
        widthBetweenSeconds = Screen.width / (float)10;
        for (int i = 0; i < intervalCount + 1; ++i)
        {
            var temp = Instantiate(second);
            temp.transform.parent = timeLine.transform;
            temp.text = i.ToString();
            temp.rectTransform.anchoredPosition = new Vector2(i * widthBetweenSeconds, 0);
            secondsOnScreen.Add(temp);
        }
    }
    void Update()
    {
        if (transform.anchoredPosition.x < midpoint)
            transform.anchoredPosition += Vector2.right * Time.deltaTime * 500;
        else
        {
            foreach (var second in secondsOnScreen)
            {
                second.rectTransform.anchoredPosition += Vector2.left * 0.5f;
                var charOrigin = second.rectTransform.anchoredPosition.x;
                //In pixels
                var charWidth = second.font.characterInfo[0].advance;

                if (charOrigin + charWidth + widthBetweenSeconds < 0)
                {
                    second.rectTransform.anchoredPosition = Vector2.right * Screen.width;
                    second.text = (latestSecond++ + 1).ToString();
                }
            }
        }

    }
}
