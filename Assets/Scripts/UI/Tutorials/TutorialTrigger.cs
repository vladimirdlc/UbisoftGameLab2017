using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TutorialTrigger : MonoBehaviour {

    [SerializeField]
    public TutorialCanvas.PlayerType m_PlayerType;
    [SerializeField]
    public int m_TutorialIndex;

    private Canvas m_Canvas;

	// Use this for initialization
	void Start () {
		if(m_PlayerType == TutorialCanvas.PlayerType.Dog)
        {
            m_Canvas = GameObject.FindGameObjectWithTag("Tutorial Canvas Ground Player").GetComponent<Canvas>();
        }
        else
        {
            m_Canvas = GameObject.FindGameObjectWithTag("Tutorial Canvas Overseer").GetComponent<Canvas>();
        }
	}

    private void OnTriggerEnter(Collider other)
    {
        if((other.tag == "PlayerGround" || other.tag == "Player"))
        {
            m_Canvas.GetComponent<TutorialCanvas>().TiggerTutorial(m_TutorialIndex);
        }
    }
}
