using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;


public enum l_mtype
{
    LOCKANIM,
    MOVEWITHANIM,
}


public class LockRoot : MonoBehaviour
{
    private l_mtype type = l_mtype.LOCKANIM;
    private Vector3 l_offset;
    private Vector3 l_moffset;

    public Transform l_controller;
    public Transform l_model;
    public Transform l_metaRig;


    // Start is called before the first frame update
    void Start()
    {
        l_offset = l_model.position - l_controller.position;
        l_moffset = l_metaRig.position;

    }

    // Update is called once per frame

    public void Lock()
    {
        type = l_mtype.LOCKANIM;
    }

    public void MoveWithAnim()
    {
        l_moffset = l_metaRig.position;
        type = l_mtype.MOVEWITHANIM;
    }

    private void LateUpdate()
    {
        PreCalc();
    }

    public void PreCalc()
    {
        if (type == l_mtype.LOCKANIM)
        {
            var o = l_model.position - l_controller.position - l_offset;
            o.y = 0;
            l_metaRig.position = new Vector3(l_metaRig.position.x - o.x,l_metaRig.position.y, l_metaRig.position.z -o.z) ;
        }
        else if (type == l_mtype.MOVEWITHANIM)
        {
            var o = l_model.position - l_controller.position - l_offset;
            o.y = 0;
            var yoffset = l_metaRig.position.y - l_controller.position.y;
            l_controller.position = new Vector3(l_model.position.x - l_offset.x, l_model.position.y - l_offset.y - yoffset, l_model.position.z - l_offset.z);
            l_metaRig.position = new Vector3(l_metaRig.position.x - o.x, l_metaRig.position.y, l_metaRig.position.z - o.z);


        }
    }
}
