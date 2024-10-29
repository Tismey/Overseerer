using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanoidLocoAnim : AIbase
{
    public WeaponAbstract weapon;
    public float rotationSpeed;
    public string team;
    public Transform test;
    public override void LookTowards(Vector3 v)
    {
        moveType.RotateActorTowards(v,rotationSpeed);
    }

    void Start()
    {
        this.team = teamName;
        this.AddState(new Idle());
    }



    public override void AIthink()
    {
        this.PlayState();
        //this.moveType.RotateActorTowards(test.position, 10f);

    }

  



}
