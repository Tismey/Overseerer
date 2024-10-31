using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stumble : AIState
{
    // Start is called before the first frame update
    public override void Setup()
    {
        
        this.Animator.SetTrigger("Stagger");
        this.ai.noGravity = true;

    }
    public override void act()
    {

        if (Animator.IsInTransition(0))
        {
            this.ai.lockRoot.Lock();
        }
        else
        {
            this.ai.lockRoot.MoveWithAnim();
        }
        if (Animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !Animator.IsInTransition(0))
        {
            hasEnded = true;
        }


    }

    public override void Interupt()
    {
        this.Animator.ResetTrigger("Stagger");
        hasEnded = true;
    }

    public override void Finish()
    {
        this.Animator.ResetTrigger("Stagger");
        this.ai.lockRoot.Lock();
        this.ai.lockRoot.PreCalc();
        hasEnded = true;
        this.ai.noGravity = false;
    }

    public override void Continue()
    {
        
    }
}
