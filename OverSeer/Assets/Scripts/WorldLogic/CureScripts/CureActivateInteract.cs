using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CureActivateInteract : Iteractebable
{


    public CureCrafterManager Ccm;

    public override void Interact(AIbase ai)
    {
        if (Ccm.CreateCure())
        {
            setInteract(false);
        }
    }

    public void Reset()
    {
        setInteract(true);
    }
    public override void InteractUpdateBehavior()
    {

    }


    // Start is called before the first frame update
    void Start()
    {
        setInteract(true);
    }
}
