using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class HumanoidLocoAnim : AIbase
{
    
    public float rotationSpeed;
    public string team;
    public Transform test;

    public Squad squad;
    public override void LookTowards(Vector3 v)
    {
        moveType.RotateActorTowards(v,rotationSpeed);
    }

    new void Start()
    {
        base.Start();
        this.team = teamName;
    }

    public void FixedUpdate()
    {
        UpdateAnimator();
    }



    public override void AIthink()
    {
        this.PlayState();
        if (canMove)
        {
            moveType.MoveActor(m_Position,sprint,crouch);
        }
        else
        {
            if (!noGravity)
            {
                moveType.MoveActor(transform.position,sprint,crouch);
                if (!rb.detectCollisions) ;
                     //rb.detectCollisions = true;

            }
            else
            {

                if (rb.detectCollisions) ;
                    //rb.detectCollisions = false;
                rb.velocity = Vector3.zero;
            }


        }

        //this.moveType.RotateActorTowards(test.position, 10f);
        

    }

  



}
