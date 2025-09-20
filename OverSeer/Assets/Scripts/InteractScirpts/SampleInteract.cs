using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleInteract : Iteractebable
{
    public CureNodeInteract.NodeType color;
    // Start is called before the first frame update
    public override void Interact(AIbase ai)
    {
        var pl = (PlayerState)ai;
        pl.NodesInPossesion[(int)color - 1] = true;
        setInteract(false);
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
