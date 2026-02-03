using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Idle : AIState
{
    HashSet<AIbase> players = new HashSet<AIbase>();
    public override void Setup()
    {

        ai.canMove = true;
        this.ai.lockRoot.Lock();
       

    }
    public override void act()
    {


        //do nothing
       

    }

    public override void Interupt()
    {
     
    }

    public override void Finish()
    {
       
    }

    public override void Continue()
    {

    }


}

