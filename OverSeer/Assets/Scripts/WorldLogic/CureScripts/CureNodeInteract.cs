using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CureNodeInteract : Iteractebable
{
    public enum NodeType
    {
        NONE = 0,
        RED = 1,
        BLUE = 2,
        GREEN = 3
    }

    public Material[] Material1  = new Material[3];

    public Material defaultMat;

    private int counter = 0;

    private NodeType currentType = NodeType.NONE ;

    public void Reset()
    {
        GetComponent<MeshRenderer>().material = defaultMat;
        setInteract(true);
    }

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
                GetComponent<MeshRenderer>().material = Material1[0];
            }
            if (counter + 1 == 2)
            {
                currentType = NodeType.BLUE;
                GetComponent<MeshRenderer>().material = Material1[1];
            }
            if (counter + 1 == 3)
            {
                currentType = NodeType.GREEN;
                GetComponent<MeshRenderer>().material = Material1[2];
            }
            
        }
        else
        {
            //do something
        }

        counter = ++counter % 3;

        

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
