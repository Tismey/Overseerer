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
    private float l_ground = 0.3f;

    public Transform l_controller;
    public Transform l_model;
    public Transform l_metaRig;


    // Start is called before the first frame update
    void Start()
    {
        l_offset = l_model.position - l_controller.position;
        l_moffset = l_metaRig.position;
        var o = l_model.position - l_controller.position;
        l_metaRig.position = new Vector3(l_metaRig.position.x - o.x, l_metaRig.position.y - o.y, l_metaRig.position.z - o.z);
    }

    /*private void Awake()
    {
        l_offset = l_model.position - l_controller.position;
        l_moffset = l_metaRig.position;
        var o = l_model.position - l_controller.position;
        l_metaRig.position = new Vector3(l_metaRig.position.x - o.x, l_metaRig.position.y - o.y, l_metaRig.position.z - o.z);
    }*/

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
            var o = l_model.position - l_controller.position;
           // o.y = 0;
            l_metaRig.position = new Vector3(l_metaRig.position.x - o.x,l_metaRig.position.y - o.y - l_ground, l_metaRig.position.z - o.z) ;
        }
        else if (type == l_mtype.MOVEWITHANIM)
        {
            var o = l_model.position - l_controller.position;
            var l = l_controller.position.y;
            //o.y = 0;
            l_controller.position = new Vector3(l_controller.position.x + o.x, l_controller.position.y + o.y + l_ground, l_controller.position.z + o.z);
            l_metaRig.position = new Vector3(l_metaRig.position.x - o.x, l_metaRig.position.y - o.y - l_ground, l_metaRig.position.z - o.z);
            



        }
    }
}
