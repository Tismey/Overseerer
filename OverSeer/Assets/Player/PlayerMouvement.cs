using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;  


public class PlayerMouvement : mouvementscript
{

    private string playerId;
    private Vector2 movementInput;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame

    public override void MoveActor(Vector3 pos)
    {
       
        this.QuakeMovementFunc(movementInput.x, movementInput.y, Input.GetKey(KeyCode.LeftShift), Input.GetKeyDown(KeyCode.Space));
    }

    public void OnMouvement(InputValue value)
    {
        movementInput = value.Get<Vector2>();
        
    }

    // This method is called for the "Sprint" action

}
