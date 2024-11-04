using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stumble : AIState
{
    // Start is called before the first frame update
    private float staggerTime = 0f;
    public override void Setup()
    {
        
        this.Animator.SetTrigger("Stagger");
        this.Animator.SetFloat("StaggerBlend",(float)Random.Range(0,3));
        this.ai.lockRoot.Lock();
        this.ai.noGravity = true;
        Debug.Log("Stagger");
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
        if (Animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.9f && !Animator.IsInTransition(0) && Animator.GetCurrentAnimatorStateInfo(0).IsName("StaggerTree"))
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
