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
    public bool interactInput;
    public int switchInput;
    public bool dropInput;
    private InputAction shootAction;
    private InputAction shoveAction;
    private InputAction jumpAction;
    private InputAction interactAction;
    private InputAction switchUp;
    private InputAction switchDown;
    private InputAction DropAction;
    // Start is called before the first frame update
    void Awake()
    {
        shootAction = GetComponent<PlayerInput>().actions["Shoot"];
        shoveAction = GetComponent<PlayerInput>().actions["Shove"];
        jumpAction = GetComponent<PlayerInput>().actions["Jump"];
        interactAction = GetComponent<PlayerInput>().actions["Interact"];
        switchUp = GetComponent<PlayerInput>().actions["ChangeWeaponUP"];
        switchDown = GetComponent<PlayerInput>().actions["ChangeWeaponDown"];
        DropAction = GetComponent<PlayerInput>().actions["Drop"];
    }

    private void Update()
    {
        ShootInput = shootAction.ReadValue<float>() > 0.9f;
        ShoveInput = shoveAction.ReadValue<float>() > 0.1f;
        interactInput = interactAction.ReadValue<float>() > 0.1f;
        dropInput = DropAction.ReadValue<float>() > 0.9f;
        if (switchDown.ReadValue<float>() > 0f)
        {
            switchInput = 1;
        }
        else if (switchUp.ReadValue<float>() > 0f)
        {
            switchInput = -1;
        }
        else
        {
            switchInput = 0;
        }
        if (!JumpInput)
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
