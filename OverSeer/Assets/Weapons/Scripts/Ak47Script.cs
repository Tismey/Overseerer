using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ak47Script : WeaponAbstract
{
    public GameObject bullet;

    public override void ShotBehavior(Vector3 dir)
    {
        var b  = Instantiate(bullet, muzzle.position - owner.transform.forward *5, muzzle.rotation);
        b.GetComponent<Rigidbody>().AddForce(dir * 10000f);
    }

    public override void ReloadBehavior()
    {
        //nothing add aniiamtion
    }
}
