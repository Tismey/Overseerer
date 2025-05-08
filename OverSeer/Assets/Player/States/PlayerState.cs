using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
public class PlayerState : AIbase
{

    public float rotationSpeed;
    public string team;
    public override void LookTowards(Vector3 v)
    {
        moveType.RotateActorTowards(v, rotationSpeed);
    }

    new void Start()
    {
        base.Start();
        this.team = teamName;
        this.AddState(new PlayerInControl());
    }

    private void Awake()
    {
        isPlayer = true;
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
                //moveType.MoveActor(transform.position);
                rb.velocity = Vector3.zero;

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
