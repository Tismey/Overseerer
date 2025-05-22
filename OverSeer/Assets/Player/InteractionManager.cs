using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class InteractionManager : MonoBehaviour
{

    private AIbase ai;
    private bool canInteract = true;
    public LayerMask interactableLayer;
    public Iteractebable interactebale;
    // Start is called before the first frame update
    void Start()
    {
        ai = GetComponent<AIbase>();
    }

    // Update is called once per frame
    void Update()
    {
     
    
    }

    public bool CanInteractWith(out Iteractebable inter)
    {
        if(canInteract)
        {
            RaycastHit hit;
            if (Physics.Raycast(ai.eyePosition.position, ai.eyePosition.forward, out hit, 10f,interactableLayer))
            {
                Iteractebable interactebale = hit.collider.GetComponent<Iteractebable>();

                if(interactebale != null)
                {
                    if (interactebale.getInteractable())
                    {
                        Debug.Log("Interactable found");
                        inter = interactebale;
                        return true;
                    }
                }
            }
        }
        inter = null;
        return false;
    }

    public void setInteract(bool canInteract)
    {
        this.canInteract = canInteract;
    }

    public Iteractebable getInteractable()
    {
        return interactebale;
    }
}
