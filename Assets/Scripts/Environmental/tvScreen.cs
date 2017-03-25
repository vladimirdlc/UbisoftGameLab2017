using UnityEngine;
using System.Collections;

public class tvScreen : MonoBehaviour {
    public Texture[] textureArray;
    private int i;
    private float timePassed;
    public float secondsForEachImage;
    private Renderer rend;
    
    void Start() {
        rend = GetComponent<Renderer>();
        i = 0;
        timePassed = 0;
    }

    void Update() {
        rend.material.SetColor("_TintColor", Color.gray);   
        timePassed += Time.deltaTime;
        if(timePassed > secondsForEachImage) {
            timePassed = 0;
            rend.material.mainTexture = textureArray[i];
            i++;
            if(i >= textureArray.Length-1)
                i=0;
        }
    }
}
