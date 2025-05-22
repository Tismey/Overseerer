using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Iteractebable : MonoBehaviour
{
    protected bool canInteract = false;
    public string interactLabel;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        InteractUpdateBehavior();
    }

    public abstract void Interact(AIbase ai);

    public abstract void InteractUpdateBehavior();

    public void setInteract(bool canInteract)
    {
        this.canInteract = canInteract;
    }
    public bool getInteractable()
    {
        return canInteract;
    }

    public string getInteractLabel()
    {
        return interactLabel + " (press E)";
    }
}
