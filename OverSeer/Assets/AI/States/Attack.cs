using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : AIState
{
    public float MaxRange;
    public float cooldown;
    private float timer = 0;
    private float damage = 20;
    private bool attacking = false;

    public Attack(float m, float c)
    {
        MaxRange = m;
        cooldown = c;
    }

    public override void Setup()
    {
        //this.Animator.SetBool("Combat", true);
        
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
       
        timer += Time.deltaTime;
       

        if (timer > cooldown)
        {
            hasEnded = true;
        }
    }

    private AIbase GetClosestEnemy()
    {
       
         
        return ai.GetEnemies();
    }   

    private void AttackEnemy()
    {
        this.Animator.SetBool("Combat", true);
        if (timer > cooldown/2 && !attacking)
        {
            RaycastHit hit;
            if(Physics.Raycast(ai.GetEyePosition(), ai.transform.forward, out hit, MaxRange))
            {
                if (hit.collider.gameObject.GetComponent<AIbase>() != null)
                {
                    var a = hit.collider.gameObject.GetComponent<AIbase>();
                    if(a.teamName == "Player")
                    {
                        attacking = true;
                        a.health.ApplyDamage(damage);
                       
                    }

                }
            }
        }
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
