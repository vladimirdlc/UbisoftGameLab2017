using UnityEngine;
using System.Collections;

public class tvScreen : MonoBehaviour {
    public Texture[] textures;
    public float changeInterval = 0.1F;
    public Renderer rend;
    public static bool doProject;
    int index = 0;
    int delay = 4;

    void Start() {
        rend = GetComponent<Renderer>();
        doProject = true;

    }
    
    void Update() {
    	if(doProject) {
        	if (textures.Length == 0)
            	return;
        	
        	rend.material.SetColor("_TintColor", Color.gray);

        	//int index = Mathf.FloorToInt(Time.time / changeInterval);
        	//index = index % textures.Length-1;
        	//int index = Mathf.RoundToInt(Random.Range(0f,textures.Length));
        	
        	delay--;
        	if(delay == 0) {
        		index++;
        		if(index > 18)
        			index = 0;
        	delay = 4;
        	}
        	rend.material.mainTexture = textures[index];
    	}
    	else
    		rend.material.mainTexture = textures[0];
    }
}
