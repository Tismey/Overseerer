using System.Collections;
using System.Collections.Generic;
using UnityEditor.Presets;
using UnityEngine;

public class Ak47Script : WeaponAbstract
{
    public GameObject bullet;
    public LayerMask hitlayer;
    public override void ShotBehavior(Vector3 dir)
    {
        RaycastHit hit;

        if (Physics.Linecast(muzzle.position, owner.transform.position, out hit, hitlayer))
        {
            if (hit.collider.gameObject.GetComponent<AIbase>() != null)
            {
                AIbase.Population.Remove(hit.collider.gameObject.GetComponent<AIbase>());
                hit.collider.gameObject.GetComponent<AIbase>().AddState(new Die());
                
            }
        }
        var b  = Instantiate(bullet, muzzle.position - owner.transform.forward *5, muzzle.rotation);
        b.GetComponent<Rigidbody>().AddForce(dir * 10000f);
    }

    public override void ReloadBehavior()
    {
        //nothing add aniiamtion
    }
}
