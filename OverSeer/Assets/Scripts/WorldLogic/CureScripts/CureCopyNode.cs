using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CureCopyNode : MonoBehaviour
{

    public CureNodeInteract copy;
    public WordLogic wl;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(wl.objectiveComplete)
            GetComponent<MeshRenderer>().material = copy.GetComponent<MeshRenderer>().material;
    }
}
