using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestShoulderRotateState : AIState
{
    private float angle = 0f;

    public override void Setup()
    {
        // Optional: set an animation if you want
        this.Animator.Play("JogMoveTree");

        // Allow animation movement like your Idle
        this.ai.lockRoot.Lock();

        // Enable shoulder system
        this.ai.lockRoot.EnableShoulder();

        Debug.Log("TestShoulderRotateState: Setup complete.");
    }

    public override void act()
    {
        // Maintain Idle animation (your pattern)


        // Increment angle from 0 to 360
        angle += Time.deltaTime * 60f;
        if (angle >= 360f)
            angle -= 360f;

        // Apply angle to shoulder
        this.ai.lockRoot.SetShoulderAngle(angle);
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
