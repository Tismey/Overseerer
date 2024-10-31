using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState : AIbase
{
    public WeaponAbstract weapon;
    public float rotationSpeed;
    public string team;
    public override void LookTowards(Vector3 v)
    {
        moveType.RotateActorTowards(v, rotationSpeed);
    }

    void Start()
    {
        this.team = teamName;
        this.AddState(new PlayerInControl());
    }



    public override void AIthink()
    {
        this.PlayState();
        if (canMove)
        {
            this.Animator.SetBool("Moving", true);
            moveType.MoveActor(m_Position);
            UpdateAnimator();
        }
        else
        {
            if (!noGravity)
            {
                moveType.MoveActor(transform.position);

            }
            else
            {
                rb.velocity = Vector3.zero;
            }

            this.Animator.SetBool("Moving", false);

        }
        //this.moveType.RotateActorTowards(test.position, 10f);

    }
}
