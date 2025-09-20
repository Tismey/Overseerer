using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BedLogic : Iteractebable
{

    public WordLogic wordLogic;
    public override void Interact(AIbase ai)
    {
        wordLogic.Reset();
        setInteract(false);
    }

    public override void InteractUpdateBehavior()
    {

    }


    // Start is called before the first frame update
    void Start()
    {
        setInteract(false);
    }
}
