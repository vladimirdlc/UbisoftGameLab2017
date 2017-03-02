using UnityEngine.UI;
using UnityEngine;

public class Timer : MonoBehaviour
{
    public Text timer;
    private float spawnTime;

    // Use this for initialization
    void Start()
    {
        spawnTime = Time.time;
        timer = GameObject.Find("Timer").GetComponent<Text>();
    }

    void Update()
    {
        timer.text = Mathf.Floor(Time.time - spawnTime).ToString();
    }
}
