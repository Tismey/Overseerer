using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PlayerInControl : AIState
{
    public Camera cam;

    public override void Setup()
    {
        this.Animator.SetBool("Moving", true);
        this.ai.lockRoot.Lock();
        this.ai.canMove = true;
        cam = Camera.main;
        Cursor.lockState = CursorLockMode.None;

    }
    public override void act()
    {
        //do nothing
        //this.Animator.SetBool("Idle",true);
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Vector3.Distance(cam.transform.position, ai.transform.position);  // distance entre la caméra et l'objet
        Vector3 worldPosition = cam.ScreenToWorldPoint(mousePos);
        ai.LookTowards(worldPosition);

    }

    public override void Interupt()
    {
        this.Animator.SetBool("Moving", false);
        this.ai.lockRoot.Lock();
    }

    public override void Finish()
    {
        this.Animator.SetBool("Moving", false);
    }

    public override void Continue()
    {
        this.ai.lockRoot.MoveWithAnim();
        this.Animator.SetBool("Moving", true);
    }


}

