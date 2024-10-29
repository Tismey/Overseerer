using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Idle : AIState
{

    public override void Setup()
    {
        this.Animator.SetBool("Idle", true);
        //this.ai.lockRoot.MoveWithAnim();
        
    }
    public override void act()
    {
        //do nothing
        //this.Animator.SetBool("Idle",true);
        if (Animator.IsInTransition(0))
        {
            this.ai.lockRoot.Lock();
        }
        else
        {
            //this.ai.lockRoot.MoveWithAnim();
        }

        Debug.Log("Idle");
        if (ai.GetEnemies().Count > 0)
        {
            foreach (AIbase a in ai.GetEnemies())
            {
               if(ai.IsSeing(a))
                {
                   ai.AddState(new Attack(100f,10f));
               }
            }
        }
    }

    public override void Interupt()
    {
        this.Animator.SetBool("Idle", false);
        this.ai.lockRoot.Lock();
    }

    public override void Finish()
    {
        this.Animator.SetBool("Idle", false);
    }

    public override void Continue()
    {
        //this.ai.lockRoot.MoveWithAnim();
        this.Animator.SetBool("Idle", true);
    }


}

