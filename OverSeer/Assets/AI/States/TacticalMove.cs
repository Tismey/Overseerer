using System.Collections.Generic;
using UnityEngine;

public class TacticalMove : AIState
{
    private Vector3 pos;
    private Vector3 danger;

    private bool sprint = false;

    private List<Vector3> path = null;
    private int currentIndex = 0;

    private readonly float climbCheckDistance = 2f;
    private readonly float climbHeightMax = 2.5f;

    // Constructors
   

    public TacticalMove(Vector3 pos, Vector3 danger,bool sprint)
    {
        this.pos = pos;
        this.danger = danger;
        this.sprint = sprint;
    }

    // -------------------------------------------------------------
    // Setup : compute initial path
    // -------------------------------------------------------------
    public override void Setup()
    {
        ComputePath();
        ((AiMouvement)ai.moveType).avoidObstacle = false;
        if(sprint) this.Animator.Play("Sprint");
        else this.Animator.Play("JogMoveTree");


        this.ai.lockRoot.Lock();

    }

    private void ComputePath()
    {
        Vector3 start = ai.transform.position;


        path = AStarPathFinder.FindPath(start, pos);

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

        // If target moved, recompute pat

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
            if(!sprint) ai.LookTowards(danger);
            else ai.LookTowards(path[currentIndex]);
        }
        else
        {
            currentIndex++;
        }

        //check For Player player detection

        if (sprint) return;
        var enemies = ai.GetEnemies();
        if(enemies.Count > 0 && ai.weapon[ai.WeaponSelect] != null)
        {
            //routine de combat
            danger = enemies[0].GetEyePosition();
            ai.LookTowards(danger);
            ai.lockRoot.shoulderLook(danger);
            ai.weapon[ai.WeaponSelect].Shoot(ai.eyePosition.forward, ai.GetEyePosition());
            if (!ai.weapon[ai.WeaponSelect].CanShoot())
            {
                sprint = true;
                Setup();
            }
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
