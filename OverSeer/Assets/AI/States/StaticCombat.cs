using UnityEngine;
using System.Collections.Generic;

public class StaticCombat : AIState
{
    private Vector3 holdPos;
    private Vector3 danger;

    private bool inCover;
    private bool crouched;

    private float crouchTimer = 0f;
    private float crouchCooldown;

    private float peekTimer = 0f;
    private float peekDelay = 1.5f;

    public StaticCombat(Vector3 holdPos, Vector3 danger)
    {
        this.holdPos = holdPos;
        this.danger = danger;
    }

    // -------------------------------------------------
    // SETUP
    // -------------------------------------------------
    public override void Setup()
    {
        ai.canMove = false;
        ai.lockRoot.MoveWithAnim();

        inCover = ai.IsInCover(danger);
        crouched = inCover;

        if (crouched)
            Animator.Play("IdleCrouch");
        else
            Animator.Play("JogMoveTree"); ;

        crouchCooldown = Random.Range(1.5f, 3.5f);
    }

    public override void Continue()
    {
        ai.canMove = false;
        ai.lockRoot.Lock();

        inCover = ai.IsInCover(danger);
        crouched = inCover;

        if (crouched)
            Animator.Play("IdleCrouch");
        else
            Animator.Play("JogMoveTree");

        crouchCooldown = Random.Range(1.5f, 3.5f);
    }

    // -------------------------------------------------
    // ACT
    // -------------------------------------------------
    public override void act()
    {
        AIbase enemies = ai.GetEnemies();
        
        bool enemyVisible = enemies != null;

        AIbase enemy = enemyVisible ? enemies : null;
        bool enemyLookingAtUs = enemyVisible && enemy.IsLookingAt(ai);

        // -------------------------------------------------
        // 1) SI EN COUVERT
        // -------------------------------------------------
        if (!ai.IsInCover(danger))
        {
           

            if (enemyVisible)
                Fire(enemy);
            if (enemyLookingAtUs && enemyVisible)
            {
                Vector3 newCover = FindBetterCoverLocal(
                    ai.transform.position,
                    danger
                );
                Debug.Log("try to get new cover");
                ai.AddState(new TacticalMove(newCover, danger, false,true));
                hasEnded = true;
                return;
            }

            return;
        }

        // -------------------------------------------------
        // 2) PAS EN COUVERT (debout)
        // -------------------------------------------------
        else
        {
            if (crouched)
            {
                crouchTimer += Time.deltaTime;
                if (crouchTimer > crouchCooldown)
                {
                    crouched = false;
                    Animator.Play("JogMoveTree");
                }
            }

            if (enemyVisible)
                Fire(enemy);

            if (enemyLookingAtUs)
            {
                crouchTimer = 0f;
                Animator.Play("IdleCrouch");
                return;
            }
        }

        // -------------------------------------------------
        // 3) PEEK LOGIC
        // -------------------------------------------------
        peekTimer += Time.deltaTime;
        if (peekTimer > peekDelay)
        {
            Vector3 peekPos = FindClosestPeek(
                ai.transform.position,
                danger
            );

            ai.AddState(new TacticalMove(peekPos, danger, false,true));
            hasEnded = true;
        }
    }

    // -------------------------------------------------
    // FIRE
    // -------------------------------------------------
    private void Fire(AIbase enemy)
    {

        Vector3 target = enemy.GetEyePosition();

        ai.LookTowards(target);
        ai.lockRoot.shoulderLook(target);
        danger = enemy.transform.position;
        if (ai.weapon[ai.WeaponSelect] == null) return;
        if (!ai.weapon[ai.WeaponSelect].CanShoot()) return;



        ai.weapon[ai.WeaponSelect].Shoot(
            (danger - ai.eyePosition.position).normalized,
                        ai.GetEyePosition()
        );
    }

    // -------------------------------------------------
    // INTERRUPT / FINISH
    // -------------------------------------------------
    public override void Interupt()
    {
        ai.canMove = false;
    }

    public override void Finish()
    {
        ai.canMove = false;
        hasEnded = true;
    }


    Vector3 FindClosestPeek(Vector3 origin, Vector3 danger, float radius = 7f)
    {
        Vector3 bestPeek = origin;
        float bestScore = float.MinValue;

        int ci, cj, ch;
        if (!NavGridGen.WorldToGrid(origin, out ci, out cj, out ch))
            return bestPeek;

        Vector3 dangerDir = danger - origin;
        dangerDir.y = 0f;
        dangerDir.Normalize();

        int range = (int)radius;

        for (int i = ci - range; i <= ci + range; i++)
        {
            for (int j = cj - range; j <= cj + range; j++)
            {
                if (!NavGridGen.IsValid(i, j, 0))
                    continue;

                NodeGrid node = NavGridGen.grid[i, j];

                for (int h = 0; h < node.heights.Length; h++)
                {
                    if (!node.isInsideGeometry[h])
                        continue;

                    Vector3 worldPos = NavGridGen.GridToWorld(i, j);

                    float dist = Vector3.Distance(origin, worldPos);
                    if (dist > radius)
                        continue;

                    // Check des 8 coins possibles
                    for (int c = 0; c < 8; c++)
                    {
                        if (!node.isCorner[c])
                            continue;

                        Vector2 off = NavGridGen.offsets[c];
                        Vector3 cornerDir = new Vector3(off.x, 0f, off.y).normalized;

                        float dot = Vector3.Dot(cornerDir, dangerDir);
                        if (dot < 0.3f)
                            continue;

                        float score = dot * 2f - dist * 0.5f;

                        if (score > bestScore)
                        {
                            bestScore = score;
                            bestPeek = worldPos;
                        }
                    }
                }
            }
        }

        return bestPeek;
    }



    Vector3 FindBetterCoverLocal(Vector3 origin, Vector3 danger, float radius = 10f)
    {
        Vector3 bestCover = origin;
        List<Vector3> l = new List<Vector3>();

        int ci, cj, ch;
        if (!NavGridGen.WorldToGrid(origin, out ci, out cj, out ch))
            return bestCover;
        Debug.Log("worldtogrid succes");
        Vector3 dangerDir = danger - origin;
        dangerDir.y = 0f;

        int dangerIndex = AStarTest.GetClosestDirectionIndex(dangerDir);
        if (dangerIndex < 0)
            return bestCover;
        Debug.Log("danger succes");
        int range = (int)radius;

        for (int i = ci - range; i <= ci + range; i++)
        {
            for (int j = cj - range; j <= cj + range; j++)
            {
                if (!NavGridGen.IsValid(i, j, 0))
                {
                    continue;
                }

                NodeGrid node = NavGridGen.grid[i, j];

                for (int h = 0; h < node.heights.Length; h++)
                {
                    if (node.isInsideGeometry[h])
                        continue;

                    bestCover = NavGridGen.GridToWorld(i, j);
                    if (!node.cover[h][dangerIndex])
                        continue;

                    Vector3 pos = NavGridGen.GridToWorld(i, j);

                    l.Add(pos);
                }
            }
        }
        if (l.Count == 0) return bestCover;
        return l[Random.Range(0,l.Count)];
    }


}
