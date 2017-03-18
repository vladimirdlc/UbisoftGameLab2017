 using UnityEngine;
 using UnityEngine.UI;
 using System.Collections;
 
 public class FadeImageInOut : MonoBehaviour {
 
     public Image m_targetImage;

     private bool m_FadingIn;
 
	 void Start(){ 
	    Color c = m_targetImage.color;
        c.a = 0;
        m_targetImage.color = c; 
	    //yield WaitForSeconds(2);
	    StartCoroutine(FadeIn());	        
	 } 

	 IEnumerator FadeIn() {
		m_FadingIn=true;

	 	while (m_targetImage.color.a < 1f){
	    	Color c = m_targetImage.color;
        	c.a += 0.1f * Time.deltaTime * 2f;
        	m_targetImage.color = c;   
	    }  	        
	    m_FadingIn = false;
	    StartCoroutine(FadeOut());
	    yield return new WaitForSeconds(0.1f);
	 }

	 IEnumerator FadeOut(){
	 	while(m_FadingIn)
	 		yield return new WaitForSeconds(0.1f);

	 	  while (m_targetImage.color.a < 1f){
	    	Color c = m_targetImage.color;
        	c.a += 0.1f * Time.deltaTime * 2f;
        	m_targetImage.color = c;   
	    }  	 
	    StartCoroutine(FadeIn());  
	 }

	 // void FadeIn(){
	 //    while (m_targetImage.color.a < 1f){
	 //    	Color c = m_targetImage.color;
  //       	c.a += 0.1f * Time.deltaTime * 2f;
  //       	m_targetImage.color = c;   
	 //    }  	        
	 //    FadeOut();
	 // }
	        
	 // void FadeOut(){
	 //     while (m_targetImage.color.a > 0f){
	 //    	Color c = m_targetImage.color;
  //       	c.a -= 0.1f * Time.deltaTime * 2f;
  //       	m_targetImage.color = c;     
	 //     }    
	 //     FadeIn();
	 // }
 }