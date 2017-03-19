using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinematicHandler : MonoBehaviour {

    public float speedLookAtPuppy;
    public float speedLookAtObjective;
    public float speedLookAtLevel;
    public float timeUntilAnimationTransition;

    Animator anim;
    private float timer;
    private int count = 2;

    private void Start()
    {
        anim = GetComponent<Animator>();
        anim.SetFloat("speedLookAtPuppy", speedLookAtPuppy);
        anim.SetFloat("speedLookAtObjective", speedLookAtObjective);
        anim.SetFloat("speedLookAtLevel", speedLookAtObjective);
        timer = timeUntilAnimationTransition;
    }

    private void Update()
    {
        if (count <= 0)
            return;

        timer -= Time.deltaTime;
        if(timer <= 0)
        {
            anim.SetTrigger("next");
            count--;
            timer = timeUntilAnimationTransition;
        }

    }

    public void onCinematicFinish()
    {
        GameState.Instance.onCinematicFinish();
        this.enabled = false;
    }
}
