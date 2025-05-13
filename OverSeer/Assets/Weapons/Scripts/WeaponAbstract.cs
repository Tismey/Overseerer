using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeaponAbstract : MonoBehaviour
{
    public string weaponName;
    public Transform muzzle;
    public Transform handle;
    public AIbase owner;
    public int maxAmmo;
    public int currentAmmo;
    public int maxClip;
    public int currentClip;

    public float reloadTime;
    public float fireRate;
    public float damage;
    public float acuracy;

    public string[] teamInteract;

    private float reloadTimer = 0f;
    private float fireTimer = 0f;
    private float bufferPickUp = 2f;
    private float bufferTimer = 0f;
    private bool pickedUp = false;


    public void Start()
    {
        if(owner == null)
        {
           pickedUp = false;
        }
        currentAmmo = maxAmmo - maxClip;
        currentClip = maxClip;
    }

    public void FixedUpdate()
    {

        if (!CanShoot())
        {
            Reload();
        }
        if (pickedUp)
        {
             transform.rotation = owner.transform.rotation;
             transform.position = owner.righthand.position;
             var offset = owner.righthand.position - handle.position;
             transform.position += offset;

        }
        else
        {
            if (bufferTimer < bufferPickUp)
            {
                bufferTimer += Time.deltaTime;
                return;
            }
            transform.rotation = Quaternion.identity;
            transform.position += Vector3.zero;
            foreach (AIbase a in AIbase.Population)
            {
                foreach(string t in teamInteract)
                {
                    
                    if (a.teamName == t)
                    {
                        if(Vector3.Distance(a.transform.position, transform.position) < 5)
                        {
                            Debug.Log("Picking up weapon");
                            pickedUp = true;
                            owner = a;
                            if (owner.weapon != null && owner.weapon != this)
                            {
                                owner.weapon.PutDown();
                            }
                            owner.weapon = this;
                            break;
                        }
                    }
                }
            }
        }

        fireTimer += Time.deltaTime;
    }

    public void Shoot(Vector3 dir,Vector3 pos)
    {   
        if (fireTimer < fireRate)
        {
            return;
        }
        
        if (!CanShoot())
        {
            Reload();
            return;
        }
        fireTimer = 0f;
        ShotBehavior(dir,pos);
        currentClip--;
    }
    public abstract void ShotBehavior(Vector3 dir,Vector3 pos);

    public abstract void ReloadBehavior();
    public void Reload()
    {
        if(reloadTimer < reloadTime)
        {
            ReloadBehavior();
            reloadTimer += Time.deltaTime;
            return;
        }
        currentClip = maxClip > currentAmmo ? currentAmmo : maxClip;
        currentAmmo -= currentClip;
        reloadTimer = 0f;
    }

    public bool CanShoot()
    {
        return currentClip > 0;
    }

    public void PutDown()
    {
        owner.weapon = null;
        owner = null;
        pickedUp = false;
        bufferTimer = 0f;
    }

}
