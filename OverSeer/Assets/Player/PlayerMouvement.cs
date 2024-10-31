using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMouvement : mouvementscript
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame

    public override void MoveActor(Vector3 pos)
    {
        this.QuakeMovementFunc(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), Input.GetKey(KeyCode.LeftShift), Input.GetKeyDown(KeyCode.Space));
    }

}
