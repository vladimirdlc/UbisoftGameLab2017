using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class DogCloneCharacter : AnimatedDog
{
    [SerializeField]
    float m_AnimSpeedMultiplier = 1f;

    Rigidbody m_Rigidbody;

    #region Added Variables
    // Additions to the original Ethan character script
    public float m_MoveSpeed;
    #endregion

    public override void Start()
    {
        // Reference animator components to match the player
        walkAnimSpeed = GameObject.FindGameObjectWithTag("Player").GetComponent<DogFP>().walkAnimSpeed;
        walkingTailWagAnimSpeed = GameObject.FindGameObjectWithTag("Player").GetComponent<DogFP>().walkingTailWagAnimSpeed;
        tiltLeftAnimSpeed = GameObject.FindGameObjectWithTag("Player").GetComponent<DogFP>().tiltLeftAnimSpeed;
        tiltRightAnimSpeed = GameObject.FindGameObjectWithTag("Player").GetComponent<DogFP>().tiltRightAnimSpeed;

        base.Start();

        m_Rigidbody = GetComponent<Rigidbody>();

        m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
    }

    public void Move(Vector3 move, bool crouch)
    {
        move *= m_MoveSpeed;

        if (Time.deltaTime > 0)
        {
            Vector3 v = move;

            // we preserve the existing y part of the current velocity.
            v.y = m_Rigidbody.velocity.y;
            m_Rigidbody.velocity = v;
        }

        // send input and other state parameters to the animator
        UpdateAnimator(move);
    }

    void UpdateAnimator(Vector3 move)
    {
        // Do not include y component of velocity for the animator
        Vector3 tempVector = m_Rigidbody.velocity;
        tempVector.y = 0;

        // update the animator parameters
        m_Animator.SetFloat("walkingSpeedMultiplier", walkAnimSpeed);
        m_Animator.SetFloat("walkingSpeed", tempVector.magnitude);

        // Increase the overall animator speed if requiresd
       m_Animator.speed = m_AnimSpeedMultiplier;
    }
}

