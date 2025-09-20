using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Idle : AIState
{
    HashSet<AIbase> players = new HashSet<AIbase>();
    public override void Setup()
    {
        this.Animator.SetBool("Idle", true);
        this.ai.lockRoot.MoveWithAnim();
        var l = AIbase.Population;
        foreach (AIbase ais in l)
        {

            if (ais.isPlayer)
            {
                players.Add(ais);
            }
        }

    }
    public override void act()
    {


        //do nothing
        this.Animator.SetBool("Idle",true);
        if (Animator.IsInTransition(0))
        {
            this.ai.lockRoot.Lock();
        }
        else
        {
            this.ai.lockRoot.MoveWithAnim();
        }

        foreach (AIbase ais in players)
        {
            if(Vector3.Distance(ai.transform.position,ais.transform.position) < 150)
            {
                //Debug.Log("player position = " + ais.transform.position + "|| ai position = " + ai.transform.position);
                //Debug.Log(" ai distance = " + Vector3.Distance(ai.transform.position, ais.transform.position));
                if(ais.GetNoise() > Vector3.Distance(ai.transform.position, ais.transform.position))
                {
                    ai.AddState(new ChaseTest(ais.transform));
                }
                return;
            }
        }
        Debug.Log("Too far, Destroy");
        AIbase.Population.Remove(ai);
        GameObject.Destroy(ai.gameObject);
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
        this.ai.lockRoot.MoveWithAnim();
        this.Animator.SetBool("Idle", true);
    }


}

