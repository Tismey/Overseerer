using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestShoulderAimAtPlayerState : AIState
{
    private Transform target;

    public TestShoulderAimAtPlayerState(Transform target)
    {
        this.target = target;
    }

    public override void Setup()
    {
        this.Animator.Play("JogMoveTree");
        this.ai.lockRoot.Lock();

        // Enable shoulder manipulation
        this.ai.lockRoot.EnableShoulder();

        Debug.Log("TestShoulderAimAtPlayerState: Setup complete.");
    }

    public override void act()
    {


        if (target == null)
            return;

        // Horizontal rotation handled by your AI function
        ai.LookTowards(target.position);


        // ----- COMPUTE VERTICAL SHOULDER ANGLE -----

        Vector3 toTarget = target.position - ai.transform.position;

        // We only need vertical angle
        float verticalAngle = Mathf.Atan2(
            toTarget.y,
            new Vector2(toTarget.x, toTarget.z).magnitude
        ) * Mathf.Rad2Deg;

        // Clamp for realism if needed (optional)
        verticalAngle = Mathf.Clamp(verticalAngle, -60f, 60f);

        // Apply shoulder angle
        this.ai.lockRoot.SetShoulderAngle(verticalAngle);
    }

    public override void Interupt()
    {

        this.ai.lockRoot.DisableShoulder();
        this.ai.lockRoot.Lock();
    }

    public override void Finish()
    {
        this.ai.lockRoot.DisableShoulder();
    }

    public override void Continue()
    {
        this.ai.lockRoot.EnableShoulder();
    }
}
