using UnityEngine;


//Send Messages for the Animal Script

public class StateBehavior : StateMachineBehaviour
{
    [Header("Send Messages |Enter:true|Exit:false|")]
    public string State;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SendMessage(State, true, SendMessageOptions.DontRequireReceiver);
    }
    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SendMessage(State, false, SendMessageOptions.DontRequireReceiver);
    }
}
