using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;  


public class PlayerMouvement : mouvementscript
{

    private string playerId;
    private Vector2 movementInput;
    public Vector2 viewInput;
    public bool ShootInput;
    public bool ShoveInput;
    public bool JumpInput;
    private InputAction shootAction;
    private InputAction shoveAction;
    private InputAction jumpAction;
    // Start is called before the first frame update
    void Awake()
    {
        shootAction = GetComponent<PlayerInput>().actions["Shoot"];
        shoveAction = GetComponent<PlayerInput>().actions["Shove"];
        jumpAction = GetComponent<PlayerInput>().actions["Jump"];
    }

    private void Update()
    {
        ShootInput = shootAction.ReadValue<float>() > 0.9f;
        ShoveInput = shoveAction.ReadValue<float>() > 0.1f;
        if(!JumpInput)
            JumpInput = jumpAction.ReadValue<float>() > 0.1f;
    }

    // Update is called once per frame

    public override void MoveActor(Vector3 pos)
    {
       
        this.QuakeMovementFunc(movementInput.x, movementInput.y, Input.GetKey(KeyCode.LeftShift), JumpInput);
        JumpInput = false;
    }

    public void OnMouvement(InputValue value)
    {
        movementInput = value.Get<Vector2>();
        
    }

    public void OnView(InputValue value)
    {
        viewInput = value.Get<Vector2>();

    }

    // This method is called for the "Shoot" action

    // This method is called for the "Shove" action

    // This method is called for the "Sprint" action
}
