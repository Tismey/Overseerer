using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorInteract : Iteractebable
{
    public GameObject door;
    private Vector3 closeDoorPos;
    private Vector3 openDoorPos;
    public bool isOpen = false;

    public override void Interact(AIbase ai)
    {
        isOpen = true;
        canInteract = false;
        Debug.Log("Door opened");
    }

    public override void InteractUpdateBehavior()
    {

        if (!isOpen)
        {
            transform.position = new Vector3(closeDoorPos.x,Mathf.Lerp(closeDoorPos.y, openDoorPos.y, Time.deltaTime),closeDoorPos.z);
            //transform.position = closeDoorPos;
        }
        else
        {
            transform.position = new Vector3(closeDoorPos.x, Mathf.Lerp(openDoorPos.y, closeDoorPos.y, Time.deltaTime), closeDoorPos.z);
            //transform.position = openDoorPos;

        }
    }


    // Start is called before the first frame update
    void Start()
    {
        door = this.gameObject;
        closeDoorPos = door.transform.position;
        openDoorPos = new Vector3(door.transform.position.x, door.transform.position.y + 10, door.transform.position.z);
        setInteract(true);
    }

}
