using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : AIState
{
    public float MaxRange;
    public float cooldown;
    private float timer = 0;

    public Attack(float m, float c)
    {
        MaxRange = m;
        cooldown = c;
    }

    public override void Setup()
    {
        this.Animator.SetBool("Combat", true);
        
    }

    public override void Continue()
    {
        //this.Animator.SetBool("Combat", true);
    }
    public override void act()
    {
        //do nothing
        
        var e = GetClosestEnemy();
       if (e != null)
        {
            ai.LookTowards(e.GetEyePosition());
            AttackEnemy();
        }
        else
        {
            timer += Time.deltaTime;
        }

        if (timer > cooldown)
        {
            hasEnded = true;
        }
    }

    private AIbase GetClosestEnemy()
    {
        AIbase closest = null;
        float distance = float.MaxValue;
        foreach (AIbase a in ai.GetEnemies())
        {
            if (ai.IsSeing(a))
            {
                float d = Vector3.Distance(a.transform.position, ai.transform.position);
                if(d > MaxRange)
                {
                    continue;
                }
                if (d < distance)
                {
                    distance = d;
                    closest = a;
                }
            }
        }
        return closest;
    }   

    private void AttackEnemy()
    {
        Debug.Log("Attacking");
        //this.Animator.SetBool("Combat", true);
    }

    public override void Interupt()
    {
        this.Animator.SetBool("Combat", false);
        hasEnded = true;
    }

    public override void Finish()
    {
        this.Animator.SetBool("Combat", false);
    }

}
