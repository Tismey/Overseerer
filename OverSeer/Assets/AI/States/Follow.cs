using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Follow : AIState
{
    private List<AIbase> sq;
    private SquadLeader leader;

    public Follow(List<AIbase> squad, SquadLeader leader)
    {
        this.sq = squad;
        this.leader = leader;
    }

    public override void Setup()
    {
        ai.canMove = true;
        Animator.Play("JogMoveTree");
        ((AiMouvement)ai.moveType).avoidObstacle = true;
        ai.lockRoot.Lock();
    }

    public override void act()
    {
        if (sq == null || ai == null)
            return;

        // Si on n'est plus dans la squad → on stoppe
        if (!sq.Contains(ai))
        {
            return;
        }

        int index = sq.IndexOf(ai);

        // Le leader n’a PAS de Follow
        if (index <= 0)
        {
            ai.canMove = false;
            return;
        }

        AIbase target = sq[index - 1];

        // Si celui qu'on suit est mort
        if (target == null)
        {
            for (int i = index - 2; i > 0; i--) { 
                if (sq[i] != null)
                {
                    target = sq[i];
                }
            }

            if(target == null)
            {
                ai.AddState(leader);
            }
        }

        Vector3 go = target.transform.position;
        ai.SetMoveVector(go);
        ai.LookTowards(go);

        var e = ai.GetEnemies();
        if(e != null)
        {
            ai.alert = true;
        }
    }

    public override void Interupt()
    {
        ai.canMove = false;
    }

    public override void Finish()
    {
        ai.canMove = false;
        hasEnded = true;
    }

    public override void Continue()
    {
        ai.canMove = true;
    }
}
