using System.Collections.Generic;
using UnityEngine;

public class MovingAndClimb : AIState
{
    private Vector3 targetPos;
    private Transform targetTransform;

    private bool followTransform = false;

    private List<Vector3> path = null;
    private int currentIndex = 0;

    private readonly float climbCheckDistance = 2f;
    private readonly float climbHeightMax = 2.5f;

    // Constructors
    public MovingAndClimb(Vector3 pos)
    {
        this.targetPos = pos;
        followTransform = false;
    }

    public MovingAndClimb(Transform target)
    {
        this.targetTransform = target;
        this.targetPos = target.position;
        followTransform = true;
    }

    // -------------------------------------------------------------
    // Setup : compute initial path
    // -------------------------------------------------------------
    public override void Setup()
    {
        ComputePath();
        ((AiMouvement)ai.moveType).avoidObstacle = false;
        this.Animator.Play("JogMoveTree");
        this.ai.lockRoot.Lock();
    }

    private void ComputePath()
    {
        Vector3 start = ai.transform.position;

        if (followTransform)
            targetPos = targetTransform.position;

        path = AStarPathFinder.FindPath(start, targetPos);

        currentIndex = 0;

        if (path == null || path.Count == 0)
        {
            Debug.LogWarning("A* returned no path.");
            hasEnded = true;
        }
    }

    // -------------------------------------------------------------
    // ACT : follow path + climbing
    // -------------------------------------------------------------
    public override void act()
    {
        ai.canMove = true;

        // If target moved, recompute path
        if (followTransform)
        {
            Vector3 newPos = targetTransform.position;
            if ((newPos - targetPos).sqrMagnitude > 0.5f)
            {
                targetPos = newPos;
                ComputePath();
            }
        }

        if (path == null || currentIndex >= path.Count)
        {
            hasEnded = true;
            return;
        }

        // -----------------------------------------
        // 1) Check if we reached the final destination
        // -----------------------------------------
        Vector3 currentGoal = path[currentIndex];
        currentGoal.y = ai.transform.position.y; // ignore y for ground movement

        float distFlat = Vector3.Distance(
            new Vector3(ai.transform.position.x, 0, ai.transform.position.z),
            new Vector3(currentGoal.x, 0, currentGoal.z)
        );

        if (currentIndex == path.Count - 1 && distFlat < 0.6f)
        {
            hasEnded = true;
            return;
        }

        // -----------------------------------------
        // 2) Climb detection
        // -----------------------------------------
        if (CheckForClimb())
            return; // climbing took over → stop moving here

        // -----------------------------------------
        // 3) Move along the path
        // -----------------------------------------
        if (distFlat > 0.4f)
        {
            ai.SetMoveVector(path[currentIndex]);
            ai.LookTowards(path[currentIndex]);
        }
        else
        {
            currentIndex++;
        }
    }


    // -------------------------------------------------------------
    // CLIMB CHECK
    // -------------------------------------------------------------
    private bool CheckForClimb()
    {
        RaycastHit hit;

        Vector3 pos = ai.transform.position;

        // forward check
        if (Physics.Raycast(pos, ai.transform.forward, out hit, climbCheckDistance))
        {
            if (hit.collider.gameObject.layer != 6)
                return false;

            // check if above is free (climb possible)
            if (!Physics.Raycast(pos + Vector3.up * climbHeightMax, ai.transform.forward, climbCheckDistance))
            {
                // find exact step height
                for (float h = climbHeightMax; h >= 0; h -= 0.1f)
                {
                    if (Physics.Raycast(pos + new Vector3(0, h, 0), ai.transform.forward, out hit, climbCheckDistance))
                    {
                        if (hit.collider.gameObject.layer != 6)
                            continue;

                        Vector3 normal = hit.normal;
                        normal.y = 0;

                        ai.AddState(new Climb(hit.point, normal));
                        return true;
                    }
                }
            }
        }

        return false;
    }

    // -------------------------------------------------------------
    // Update external target position
    // -------------------------------------------------------------
    public void UpdatePosition(Vector3 newPos)
    {
        targetPos = newPos;
        followTransform = false;
        ComputePath();
    }

    // -------------------------------------------------------------
    // State machine callbacks
    // -------------------------------------------------------------
    public override void Interupt()
    {
        ai.canMove = false;
        ((AiMouvement)ai.moveType).avoidObstacle = true;
    }

    public override void Finish()
    {
        ai.canMove = false;
        hasEnded = true;
        ((AiMouvement)ai.moveType).avoidObstacle = true;
    }

    public override void Continue()
    {
        Setup();
        ((AiMouvement)ai.moveType).avoidObstacle = false;
    }
}
