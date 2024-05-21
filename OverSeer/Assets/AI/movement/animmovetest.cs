using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class animmovetest : MonoBehaviour
{
    public Transform objectTransform;
    public Transform feet1;
    public Transform feet2;
    public Transform maximumHeight;
    public Animator anim;
    private Vector3 anchor;
    private bool feetdown = true;
    private Vector3 lastDir;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.T))
        {
            anim.SetBool("walkf", true);
        }
        else
        {
            anim.SetBool("walkf", false);
        }

        if (Input.GetKey(KeyCode.Y))
        {
            anim.SetBool("walkb", true);
        }
        else
        {
            anim.SetBool("walkb", false);
        }
        moveWithfeet();
       // objectTransform.Rotate(new Vector3(0, 0.1f, 0));
    }
    void moveWithfeet()
    {
        if(feet1.position.y < feet2.position.y)
        {
            if(feet1.position.y > maximumHeight.position.y)
            {
                objectTransform.position -= lastDir;
            }
            if(feetdown != true)
            {
                feetdown = true;
                anchor = feet1.position;
                Debug.Log("newAncher1");
                return;
            }
            var xoffset = feet1.position.x - anchor.x;
            var zoffset = feet1.position.z - anchor.z;
            Debug.Log(new Vector3(xoffset, 0, zoffset));
            lastDir = new Vector3(xoffset, 0, zoffset);
            objectTransform.position -= lastDir;

        }
        else
        {
            if (feet2.position.y > maximumHeight.position.y)
            {
                objectTransform.position -= lastDir;
            }
            if (feetdown != false)
            {
                feetdown = false;
                anchor = feet2.position;
                Debug.Log("newAncher2");
                return;
            }
            var xoffset = feet2.position.x - anchor.x;
            var zoffset = feet2.position.z - anchor.z;
            Debug.Log(new Vector3(xoffset, 0, zoffset));
            lastDir = new Vector3(xoffset, 0, zoffset);
            objectTransform.position -= lastDir;
        }
    }
}
