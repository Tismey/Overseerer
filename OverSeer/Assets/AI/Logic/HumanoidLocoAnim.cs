using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class HumanoidLocoAnim : AIbase
{
    
    public float rotationSpeed;
    public string team;
    public Transform test;
    public override void LookTowards(Vector3 v)
    {
        moveType.RotateActorTowards(v,rotationSpeed);
    }

    new void Start()
    {
        base.Start();
        this.team = teamName;
        //this.AddState(new Idle());
    }



    public override void AIthink()
    {
        this.PlayState();
        if (canMove)
        {
            this.Animator.SetBool("Moving", true);
            this.Animator.SetBool("Airborne", moveType.IsGrounded()) ;
            moveType.MoveActor(m_Position);
            UpdateAnimator();
        }
        else
        {
            if (!noGravity)
            {
                moveType.MoveActor(transform.position);
                if (!rb.detectCollisions) ;
                     //rb.detectCollisions = true;

            }
            else
            {

                if (rb.detectCollisions) ;
                    //rb.detectCollisions = false;
                rb.velocity = Vector3.zero;
            }

            this.Animator.SetBool("Moving", false);

        }
        
        //this.moveType.RotateActorTowards(test.position, 10f);

    }

  



}
