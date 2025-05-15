using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Die : AIState
{
    // Start is called before the first frame update
    private float staggerTime = 0f;
    public override void Setup()
    {
        ai.SetRagdollState(true);
        ai.ragdollHolder.transform.parent = null;
        ai.Animator.enabled = false;
        AIbase.Population.Remove(ai);
        GameObject.Destroy(ai.gameObject);
        return;
    }
    public override void act()
    {


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
