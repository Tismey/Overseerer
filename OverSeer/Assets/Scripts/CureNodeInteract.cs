using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CureNodeInteract : Iteractebable
{
    public enum NodeType
    {
        NONE = 0,
        RED = 1,
        YELLOW = 2,
        BLUE = 3,
        GREEN = 4
    }

    private int counter = 0;

    private NodeType currentType = NodeType.NONE ;

    public NodeType getCurtype()
    {
        return currentType;
    }
    public override void Interact(AIbase ai)
    {
        var player = (PlayerState)ai;
        if(player.NodesInPossesion[counter])
        {
            if(counter + 1 == 1)
            {
                currentType = NodeType.RED;
            }
            if (counter + 1 == 2)
            {
                currentType = NodeType.YELLOW;
            }
            if (counter + 1 == 3)
            {
                currentType = NodeType.BLUE;
            }
            if (counter + 1 == 4)
            {
                currentType = NodeType.GREEN;
            }
        }
        else
        {
            //do something
        }

        counter = ++counter % 4;

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
