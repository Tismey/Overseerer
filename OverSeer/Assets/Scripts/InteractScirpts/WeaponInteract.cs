using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponInteract : Iteractebable
{



    private WeaponAbstract weapon;

    public override void Interact(AIbase ai)
    {
        weapon.WeaponPickUp(ai);
        setInteract(false);
    }

    public override void InteractUpdateBehavior()
    {
       if(weapon.owner == null) { setInteract(true); }
    }


    // Start is called before the first frame update
    void Start()
    {
        weapon = this.GetComponent<WeaponAbstract>();
        setInteract(true);
    }
}
