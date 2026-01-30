using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class mouvementscript : MonoBehaviour
{
    public Rigidbody rb;
    public GameObject body;
    public float targetvel;
    public float sprintmofier;
    public float MAXSPEED = 30f;
    public float MAXACCEL = 3 ;
    public const float MAXAIR = 1f;
    public float MAXHEIGTH = 10f;
    private float RAYOFFSET = 1f; //for consitency when hitting the ground
    public float jumpHeight;
    public float decay = 20f;
    public float sprintp = 1.20f;
    private float sprintm;
    public float xmov;
    public float zmov;
    public bool noclip;

    public bool useRotation = true;

    Vector3 velocity;
    Vector3 wishdir;
    public LayerMask layermask;


    // Update is called once per frame
    public void QuakeMovementFunc(float x, float z,bool run, bool jump)
    {

      
            xmov = x;
            zmov = z;
            var f = useRotation ? transform.forward : Vector3.forward;
            var r = useRotation ? transform.right : Vector3.right;
            wishdir = f * zmov + r * xmov;
            wishdir.Normalize();

         // =========================
         // WALL SLIDING (CORE FIX)
         // =========================
         RaycastHit wallHit;
         if (Physics.Raycast(
             transform.position + Vector3.down * RAYOFFSET,
             wishdir,
             out wallHit,
             0.9f,
             layermask
         ) || Physics.Raycast(
             transform.position + Vector3.down * RAYOFFSET + Vector3.right,
             wishdir,
             out wallHit,
             0.9f,
             layermask)

         || Physics.Raycast(
             transform.position + Vector3.down * RAYOFFSET - Vector3.right,
             wishdir,
             out wallHit,
             0.9f,
             layermask))
         {
             // Si on pousse vers le mur
             if (Vector3.Dot(wishdir, wallHit.normal) < 0f)
             {
                 wishdir = Vector3.ProjectOnPlane(wishdir, wallHit.normal);

                 if (wishdir.sqrMagnitude > 0.001f)
                     wishdir.Normalize();
                 else
                     return; // plus rien de valide à faire
             }
         }



        if (noclip)
            {

                transform.position += wishdir * MAXSPEED * Time.deltaTime;
                return;
            }


            float currentspeed = Vector3.Dot(rb.velocity, wishdir); 
            if(run && zmov > 0.1f)
            {
                sprintm = sprintp;
            }
            else
            {
                sprintm = 1;
            }

           


        //On differencie bien le comportement dans les air de celui sur le sol
            RaycastHit hit;
            if (Physics.Raycast(transform.position + new Vector3(0,RAYOFFSET,0), -Vector3.up, out hit, Mathf.Infinity, layermask))
            {
               
                if (Mathf.Abs(hit.point.y - transform.position.y) < MAXHEIGTH + 0.5f)
                {
                    if (jump)
                    {
                        
                        rb.velocity += new Vector3(0, -rb.velocity.y + jumpHeight, 0);
                        return;
                    }
                    else {
                        rb.velocity = new Vector3(rb.velocity.x, 0,rb.velocity.z);
                    }

                    transform.position = new Vector3(transform.position.x, hit.point.y + MAXHEIGTH - 0.01f, transform.position.z) ;
                    rb.velocity += new Vector3(-rb.velocity.x/decay, 0, -rb.velocity.z/decay);
                    float addspeed = Mathf.Clamp(MAXSPEED - currentspeed, 0, MAXACCEL * Time.fixedDeltaTime);
                    rb.velocity += (addspeed * wishdir * sprintm);

                   

                }
                else
                {
                    float addspeed = Mathf.Clamp(MAXAIR - currentspeed, 0, MAXACCEL * Time.deltaTime);
                    rb.velocity += (addspeed * wishdir * sprintm) + new Vector3(0, -20.81f * Time.fixedDeltaTime, 0);
                }

            }
            else
            {
                float addspeed = Mathf.Clamp(MAXAIR - currentspeed, 0, MAXACCEL * Time.deltaTime);
                rb.velocity += (addspeed * wishdir * sprintm) + new Vector3(0, -20.81f * Time.fixedDeltaTime, 0);
            }
    }

    public float GetCurrentSpeed()
    {
        return rb.velocity.magnitude;
    }

    public bool IsGrounded()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -Vector3.up, out hit, Mathf.Infinity, layermask))
        {
            if (Mathf.Abs(hit.point.y - transform.position.y) < MAXHEIGTH + 0.5f)
            {
                return false;
            }
        }

        return true;
    }

    public void RotateActorTowards(Vector3 pos, float rotationSpeed)
    {
        Vector3 direction = pos - transform.position;
        direction.y = 0;  // Keep the direction on the horizontal plane.

        if (direction.sqrMagnitude > 0.001f) // Ensure there's a valid direction to rotate towards
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // Get the current and target rotations around the y-axis only.
            float currentYRotation = transform.rotation.eulerAngles.y;
            float targetYRotation = targetRotation.eulerAngles.y;

            // Create a rotation around the y-axis.
            Quaternion newRotation = Quaternion.Euler(0, Mathf.LerpAngle(currentYRotation, targetYRotation, rotationSpeed * Time.deltaTime), 0);

            // Apply the new rotation.
            transform.rotation = newRotation;
        }
    }


    public abstract void MoveActor(Vector3 pos);
   

}
