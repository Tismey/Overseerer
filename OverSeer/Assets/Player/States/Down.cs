using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Down : AIState
{

    public override void Setup()
    {
        this.Animator.SetBool("Down", true);
        this.ai.lockRoot.MoveWithAnim();

    }
    public override void act()
    {
        //do nothing
        this.Animator.SetBool("Down", true);
        if (Animator.IsInTransition(0))
        {
            this.ai.lockRoot.Lock();
        }
        else
        {
            this.ai.lockRoot.MoveWithAnim();
        }

    }

    public override void Interupt()
    {
        this.Animator.SetBool("Down", false);
        this.ai.lockRoot.Lock();
    }

    public override void Finish()
    {
        this.Animator.SetBool("Down", false);
    }

    public override void Continue()
    {
        this.ai.lockRoot.MoveWithAnim();
        this.Animator.SetBool("Down", true);
    }


}
