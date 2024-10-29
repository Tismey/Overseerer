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
    void FixedUpdate()
    {
        this.QuakeMovementFunc(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), Input.GetKey(KeyCode.LeftShift), Input.GetKeyDown(KeyCode.Space));
    }

    public override void MoveActor(Vector3 pos)
    {
        transform.position = pos;
    }

}
