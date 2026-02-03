using System.Collections;
using System.Collections.Generic;
using UnityEditor.Presets;
using UnityEngine;
using UnityEngine.Rendering;

public class Ak47Script : WeaponAbstract
{
    public GameObject bullet;
    public LayerMask hitlayer;


    new void Start()
    {
        base.Start();
        
    }
    public override void ShotBehavior(Vector3 dir, Vector3 pos)
    {
        RaycastHit hit;

        if (Physics.Linecast(pos,pos + (dir * 10) , out hit, hitlayer))
        {
            if (hit.collider.gameObject.GetComponent<AIbase>() != null)
            {
                if(owner != hit.collider.gameObject.GetComponent<AIbase>())
                {
                    hit.collider.gameObject.GetComponent<AIbase>().health.ApplyDamage(damage);
                    hit.collider.gameObject.GetComponent<AIbase>().rb.AddForce(transform.forward * (damage / 10), ForceMode.VelocityChange);
                }
               
            }
        }
        else
        {
            var b = Instantiate(bullet, muzzle.position, muzzle.rotation);
            b.GetComponent<Rigidbody>().AddForce(dir * 10000f);
        }
        

    }

    public override void ReloadBehavior()
    {
        //nothing add aniiamtion
    }
}
