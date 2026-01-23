using UnityEngine;

public class Follow : AIState
{
    private Squad sq;

    public Follow(Squad squad)
    {
        this.sq = squad;
    }

    public override void Setup()
    {
        ai.canMove = true;
        Animator.Play("JogMoveTree");
        ai.lockRoot.Lock();
    }

    public override void act()
    {
        if (sq == null || ai == null)
            return;

        // Si on n'est plus dans la squad → on stoppe
        if (!sq.soldiers.Contains(ai))
        {
            ai.canMove = false;
            return;
        }

        int index = sq.soldiers.IndexOf(ai);

        // Le leader n’a PAS de Follow
        if (index <= 0)
        {
            ai.canMove = false;
            return;
        }

        AIbase target = sq.soldiers[index - 1];

        // Si celui qu'on suit est mort
        if (target == null)
        {
            ai.canMove = false;
            return;
        }

        Vector3 go = target.transform.position;
        ai.SetMoveVector(go);
        ai.LookTowards(go - ai.transform.position);
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
