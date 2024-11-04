using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Climb : AIState
{
    // Start is called before the first frame update
    private float staggerTime = 0f;
    private Vector3 pos;


    public Climb(Vector3 pos)
    {
        this.pos = pos;
    }
    public override void Setup()
    {

        this.Animator.SetTrigger("Climb");
        this.ai.lockRoot.Lock();
        this.ai.noGravity = true;
        Debug.Log("Climb");
        ai.transform.position = pos;
    }
    public override void act()
    {
        if (staggerTime < 0.05f)
        {
            this.ai.lockRoot.Lock();
            staggerTime += Time.deltaTime;
            return;
        }

        if (hasEnded)
        {
            this.ai.lockRoot.Lock();
            return;
        }

        if (Animator.IsInTransition(0))
        {
            this.ai.lockRoot.Lock();
            return;
        }
        else
        {
            this.ai.lockRoot.MoveWithAnim();
        }
        if (Animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.9f && !Animator.IsInTransition(0) && Animator.GetCurrentAnimatorStateInfo(0).IsName("climbHigh"))
        {
            hasEnded = true;
        }


    }

    public override void Interupt()
    {
        this.Animator.ResetTrigger("Climb");
        hasEnded = true;
    }

    public override void Finish()
    {
        ai.transform.position += ai.transform.forward * 1f;
        this.Animator.ResetTrigger("Climb");
        this.ai.lockRoot.Lock();
        this.ai.lockRoot.PreCalc();
        hasEnded = true;
        this.ai.noGravity = false;
    }

    public override void Continue()
    {

    }
}

