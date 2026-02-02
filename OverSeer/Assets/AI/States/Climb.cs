using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Climb : AIState
{
    // Start is called before the first frame update
    private float staggerTime = 0f;
    private Vector3 pos;
    private Vector3 offset = new Vector3(0, 1f, 0);
    private Vector3 surfaceDir;
    private string animationName = "climbHigh";


    public Climb(Vector3 pos, Vector3 surfaceDir)
    {
        this.pos = pos;
        this.surfaceDir = surfaceDir;
    }
    public override void Setup()
    {

        this.Animator.Play(animationName);
        this.ai.lockRoot.Lock();
        this.ai.noGravity = true;
        ai.transform.position += ((pos - offset) - ai.transform.position )* 2 * Time.deltaTime;
        ai.transform.rotation = Quaternion.LookRotation(-surfaceDir);
    }
    public override void act()
    {
        if(ai.transform.position.y < (pos - offset).y - 0.01f)
        {
            ai.transform.position += ((pos - offset) - ai.transform.position) * 2 * Time.deltaTime;
        }
      
        if (staggerTime < 0.05f)
        {
            this.ai.lockRoot.Lock();
            staggerTime += Time.deltaTime;
            return;
        }

        if (hasEnded)
        {
            this.ai.lockRoot.Lock();
            return;
        }

        if (Animator.IsInTransition(0))
        {
            this.ai.lockRoot.Lock();
            return;
        }
        else
        {
            this.ai.lockRoot.MoveWithAnim();
        }
        if (Animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.9f && !Animator.IsInTransition(0) && Animator.GetCurrentAnimatorStateInfo(0).IsName(animationName))
        {
            hasEnded = true;
            Debug.Log("climbed Exited");
        }


    }

    public override void Interupt()
    {
        hasEnded = true;
    }

    public override void Finish()
    {
        ai.transform.position += ai.transform.forward * 0.5f;
        this.ai.lockRoot.Lock();
        this.ai.lockRoot.PreCalc();
        hasEnded = true;
        this.ai.noGravity = false;
    }

    public override void Continue()
    {

    }
}

