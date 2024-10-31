using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Moving : AIState
{   public Vector3 pos;
    public Transform tran;
    public List<Vector3> path;
    public NavMeshPath nav = new NavMeshPath();
    private bool hasPath;
    private int currentCorner = 0;
    private bool isVec = false;
    private bool isTrans = false;
    // Start is called before the first frame update
    public Moving(Vector3 pos)
    {
        this.pos = pos;
        isVec = true;
    }
    public Moving(Transform t)
    {
        tran = t;
        this.pos = t.position;
        isTrans = true;
    }
    public override void Setup()
    {
  
        this.hasPath = NavMesh.CalculatePath(ai.transform.position,pos,NavMesh.AllAreas, nav) ;
    }
    public override void act()
    {
        if (isTrans)
        {
            pos = tran.position;
        }
       
        ai.canMove = true;
        //this.hasPath = NavMesh.CalculatePath(ai.transform.position, pos, NavMesh.AllAreas, nav);
        if (!hasPath)
        {
            Debug.Log("No path found");
            hasEnded = true;
            return;
        }

        if (Vector3.Distance(pos,ai.transform.position) <= 2.1f)
        {
            Debug.Log("Arrived at destination");
            this.hasEnded = true;
            return;
        }
        if (currentCorner >= nav.corners.Length)
        {
            this.hasEnded = true;
            return;
        }
        Vector3 aiPosition = ai.transform.position;
        Vector3 cornerPosition = nav.corners[currentCorner];

        // Ignore the y-axis by setting both y values to 0.
        aiPosition.y = 0;
        cornerPosition.y = 0;

        if (Vector3.Distance(aiPosition, cornerPosition) > 2f)
        {
            Debug.Log("Moving....");
            ai.SetMoveVector(nav.corners[currentCorner]);
            this.hasPath = NavMesh.CalculatePath(ai.transform.position, pos, NavMesh.AllAreas, nav);
            currentCorner = 0;

        }
        else
        {
            Debug.Log("Moving to next corner");
            if (isTrans)
            {
                
            }
            currentCorner++;
        }



    }

    public void UpdatePosition(Vector3 pos)
    {
        this.pos = pos;
    }

    public override void Interupt()
    {
        //Debug.Log(this.ai);
        ai.canMove = false;
        //this.Animator.SetBool("Idle", false);
    }

    public override void Finish()
    {
        ai.canMove = false;
        this.hasEnded = true;
        //this.Animator.SetBool("Idle", false);
    }

    public override void Continue()
    {
        ai.canMove = true;
        this.hasPath = NavMesh.CalculatePath(ai.transform.position, pos, NavMesh.AllAreas, nav);
        //this.Animator.SetBool("Idle", true);
    }
}
