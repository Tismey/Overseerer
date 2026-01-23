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
        ai.lockRoot.Lock();

        inCover = ai.IsInCover(danger);
        crouched = inCover;

        if (crouched)
            Animator.Play("CrouchIdle");
        else
            Animator.Play("JogIdle");

        crouchCooldown = Random.Range(1.5f, 3.5f);
    }

    public override void Continue()
    {
        ai.canMove = false;
        ai.lockRoot.Lock();

        inCover = ai.IsInCover(danger);
        crouched = inCover;

        if (crouched)
            Animator.Play("CrouchIdle");
        else
            Animator.Play("JogIdle");

        crouchCooldown = Random.Range(1.5f, 3.5f);
    }

    // -------------------------------------------------
    // ACT
    // -------------------------------------------------
    public override void act()
    {
        List<AIbase> enemies = ai.GetEnemies();
        bool enemyVisible = enemies.Count > 0;

        AIbase enemy = enemyVisible ? enemies[0] : null;
        bool enemyLookingAtUs = enemyVisible && enemy.IsLookingAt(ai);

        // -------------------------------------------------
        // 1) SI EN COUVERT
        // -------------------------------------------------
        if (inCover)
        {
            if (!crouched)
            {
                crouched = true;
                Animator.Play("CrouchIdle");
            }

            if (enemyVisible)
                Fire(enemy);

            if (enemyLookingAtUs)
            {
                Vector3 newCover = FindBetterCoverLocal(
                    ai.transform.position,
                    danger
                );

                ai.AddState(new TacticalMove(newCover, danger, false));
                hasEnded = true;
                return;
            }

            return;
        }

        // -------------------------------------------------
        // 2) PAS EN COUVERT (debout)
        // -------------------------------------------------
        if (!inCover)
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
                crouched = true;
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

            ai.AddState(new TacticalMove(peekPos, danger, false));
            hasEnded = true;
        }
    }

    // -------------------------------------------------
    // FIRE
    // -------------------------------------------------
    private void Fire(AIbase enemy)
    {
        if (ai.weapon[ai.WeaponSelect] == null) return;
        if (!ai.weapon[ai.WeaponSelect].CanShoot()) return;

        Vector3 target = enemy.GetEyePosition();

        ai.LookTowards(target);
        ai.lockRoot.shoulderLook(target);

        ai.weapon[ai.WeaponSelect].Shoot(
            ai.eyePosition.forward,
            target
        );

        danger = enemy.transform.position - ai.transform.position;
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
        return Vector3.zero;
    }

    Vector3 FindBetterCoverLocal(Vector3 origin, Vector3 danger, float radius = 7f)
    {
        Vector3 bestCover = origin;

        int ci, cj, ch;
        if (!NavGridGen.WorldToGrid(origin, out ci, out cj, out ch))
            return bestCover;

        Vector3 dangerDir = danger - origin;
        dangerDir.y = 0f;

        int dangerIndex = AStarTest.GetClosestDirectionIndex(dangerDir);
        if (dangerIndex < 0)
            return bestCover;

        float bestScore = float.MinValue;
        int range = (int)radius;

        for (int i = ci - range; i <= ci + range; i++)
        {
            for (int j = cj - range; j <= cj + range; j++)
            {
                if (!NavGridGen.IsValid(i, j))
                    continue;

                NodeGrid node = NavGridGen.grid[i, j];
                if (!node.isInsideGeometry[ch] || !node.cover[ch][dangerIndex])
                    continue;

                Vector3 pos = NavGridGen.GridToWorld(i, j);

                float dist = Vector3.Distance(origin, pos);
                if (dist > radius)
                    continue;

                // scoring simple mais efficace
                float score = -dist;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestCover = pos;
                }
            }
        }

        return bestCover;
    }

}
