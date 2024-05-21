using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class mouvementscript : MonoBehaviour
{
    public Rigidbody rb;
    public GameObject body;
    public Transform groundcheck;
    public Camera cam;
    public float targetvel;
    public float sprintmofier;
    public const float MAXSPEED = 30;
    public const float MAXACCEL = 3 * MAXSPEED;
    public const float MAXAIR = 1;
    public const float MAXHEIGTH = 2f;
    public float jumpHeight;
    public float decay;
    public float sprintp = 1.20f;
    private float sprintm;
    public Text txt;
    public float xmov;
    public float zmov;
    public bool noclip;

    Vector3 velocity;
    Vector3 wishdir;
    public LayerMask layermask;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {

      
            xmov = Input.GetAxis("Horizontal");
            zmov = Input.GetAxis("Vertical");
      
            wishdir = transform.forward * zmov + transform.right * xmov;
            wishdir.Normalize();
            if (noclip)
            {

                transform.position += wishdir * MAXSPEED * Time.deltaTime;
                return;
            }
            float currentspeed = Vector3.Dot(rb.velocity, wishdir); 
            if(Input.GetKey(KeyCode.LeftShift) && zmov > 0.1f)
            {
                sprintm = sprintp;
            }
            else
            {
                sprintm = 1;
            }

           


        //On differencie bien le comportement dans les air de celui sur le sol
            RaycastHit hit;
            if (Physics.Raycast(transform.position, -Vector3.up, out hit, Mathf.Infinity, layermask))
            {
                if (Mathf.Abs(hit.point.y - transform.position.y) < MAXHEIGTH)
                {
                    transform.position = new Vector3(transform.position.x,transform.position.y + (MAXHEIGTH - Mathf.Abs(hit.point.y - transform.position.y)),transform.position.z);
                    rb.velocity += new Vector3(-rb.velocity.x/decay, 0, -rb.velocity.z/decay);
                    float addspeed = Mathf.Clamp(MAXSPEED - currentspeed, 0, MAXACCEL * Time.fixedDeltaTime);
                    rb.velocity += (addspeed * wishdir * sprintm);

                    if (Input.GetKey(KeyCode.Space))
                    {

                        rb.velocity += new Vector3(0, -rb.velocity.y + jumpHeight, 0);
                    }

                }
                else
                {
                    float addspeed = Mathf.Clamp(MAXAIR - currentspeed, 0, MAXACCEL * Time.deltaTime);
                    rb.velocity += (addspeed * wishdir * sprintm) + new Vector3(0, -9.81f * Time.fixedDeltaTime, 0);
                }

            }
           
            // cam.transform.Rotate(cam.transform.forward, rb.velocity.x*20);





           //txt.text = "vel :" + (int)((Mathf.Sqrt(rb.velocity.x * rb.velocity.x 
             //                  + rb.velocity.z * rb.velocity.z)) * 10) + "curspeed : " +( Physics.CheckSphere(groundcheck.position, 0.1f) ? "true" : "false");
        
    }
}
