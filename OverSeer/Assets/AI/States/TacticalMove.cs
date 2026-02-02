using System.Collections.Generic;
using UnityEngine;


public enum ACTIONTYPE
{
    MOVEFAST,
    CAUTIOUS,
}
public class TacticalMove : AIState
{
    private Vector3 pos;
    private Vector3 danger;

    private bool sprint;

    private bool forceMove = false;

    private List<Vector3> path = null;
    private int currentIndex = 0;

    private readonly float climbCheckDistance = 2f;
    private readonly float climbHeightMax = 2.5f;

    private Vector3 cover;

    // Constructors
   

    public TacticalMove(Vector3 pos, Vector3 danger,bool ac, bool forceMove)
    {
        this.pos = pos;
        this.danger = danger;
        this.sprint = ac;
        this.forceMove = forceMove;
    }

    // -------------------------------------------------------------
    // Setup : compute initial path
    // -------------------------------------------------------------
    public override void Setup()
    {
        if (!sprint)
            this.Animator.Play("JogMoveTree");
        else
            this.Animator.Play("Sprint");
        ComputePath();
        ((AiMouvement)ai.moveType).avoidObstacle = false;
      
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

        ai.crouch = false;
        ai.sprint = sprint;
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

        if (currentIndex == path.Count - 1 && ai.HasPassedPointXZ(path[currentIndex], 0.7f))
        {
            hasEnded = true;
            return;
        }

        // -----------------------------------------
        // 2) Climb detection
        // -----------------------------------------
        //if (CheckForClimb())
        //  return; // climbing took over → stop moving here*/

        // -----------------------------------------
        // 3) Move along the path
        // -----------------------------------------

        var enemy = ai.GetEnemies();
        if (enemy !=  null)
        {
            danger = enemy.transform.position;
            if (!forceMove)
            {
                ai.AddState(new StaticCombat(ai.transform.position, danger));
                return;
            }
                
            if(!sprint)
            {
                ai.LookTowards(danger);
                ai.lockRoot.shoulderLook(danger);
                if (ai.weapon[ai.WeaponSelect] != null)
                    if (Random.Range(0, 9) == 0)
                    {
                        ai.weapon[ai.WeaponSelect].Shoot(
                       (danger - ai.eyePosition.position).normalized,
                                   ai.GetEyePosition()
                   );
                    }

                if (ai.weapon[ai.WeaponSelect] != null && !ai.weapon[ai.WeaponSelect].CanShoot())
                {
                    sprint = true;
                    this.Animator.Play("Sprint");
                }
            }
        }

        if (!ai.HasPassedPointXZ(path[currentIndex],0.7f) )
        {
            if (sprint || !ai.alert)
                ai.LookTowards(path[currentIndex]);
            else
                ai.LookTowards(danger);

            ai.SetMoveVector(path[currentIndex]);
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
        pos = newPos;
        ComputePath();
    }

    // -------------------------------------------------------------
    // State machine callbacks
    // -------------------------------------------------------------
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
        Setup();

    }


  
}
