using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AiMouvement : mouvementscript
{

    public LayerMask obstacleLayer;
    public LayerMask other;
    public bool avoidObstacle = true;
    float x = 0;
    float z = 0;
    public override void MoveActor(Vector3 pos)
    {

        float dist = Vector3.Distance(transform.position, pos);
        if (dist < 0.1f)
        {
            this.QuakeMovementFunc(0, 0, false, false);
            return;
        }
       
        var angToTar = Vector3.SignedAngle( pos - transform.position, transform.forward, transform.up);

        /* if (angToTar < 70 && angToTar > -70)
         {
             z = 1f;
         }
         else if (angToTar > 110 || angToTar < -110)
         {
             z = -1f;
         }
         else{ z = 0; }

         if (angToTar > 20 && angToTar < 160)
         {
             x = 1f;
         }
         else if (angToTar < -20 && angToTar > -160)
         {
             x = -1f;
         }
        else { x = 0; }*/
        float sin = Mathf.Sin(angToTar * Mathf.Deg2Rad);
        float cos = Mathf.Cos(angToTar * Mathf.Deg2Rad);

        const float dead = 0.4f;

        x = Mathf.Abs(sin) < dead ? 0f : Mathf.Sign(sin);
        z = Mathf.Abs(cos) < dead ? 0f : Mathf.Sign(cos);



        var t = obstacleAvoidance();
        if (avoidObstacle)
        {
            x += t.x;
            z += t.y;
        }
            

        x *= dist;
        z *= dist;
        x = Mathf.Clamp(x, -1, 1);
        z = Mathf.Clamp(z, -1, 1);
        

        this.QuakeMovementFunc(-x, z,false,false);
        //this.RotateActorTowards(pos,2f);
    }

    public Vector2 obstacleAvoidance()
    {

        Vector2 ret = new Vector2(0, 0);
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward + transform.right, out hit, 4f))
        {
            //Debug.DrawRay(transform.position, (transform.forward - transform.right) * 4f, Color.red, 1f);
            //Debug.DrawRay(transform.position, (transform.forward + transform.right) * 4f, Color.green, 1f);
            
            if (((1 << hit.collider.gameObject.layer) & obstacleLayer) != 0 || ((1 << hit.collider.gameObject.layer) & other) != 0)
            {
                ret.x = 2;
                ret.y = -1;
       
            }
            
        }
        if (Physics.Raycast(transform.position, transform.forward - transform.right, out hit, 4f))
        {
            if (((1 << hit.collider.gameObject.layer) & obstacleLayer) != 0 || ((1 << hit.collider.gameObject.layer) & other) != 0)
            {
                ret.x = -2;
                ret.y = -1;

            }
            
        }

        if (Physics.Raycast(transform.position, transform.right, out hit, 2f))
        {
            //Debug.DrawRay(transform.position, (transform.forward - transform.right) * 4f, Color.red, 1f);
            //Debug.DrawRay(transform.position, (transform.forward + transform.right) * 4f, Color.green, 1f);

            if (((1 << hit.collider.gameObject.layer) & obstacleLayer) != 0 || ((1 << hit.collider.gameObject.layer) & other) != 0)
            {
                ret.x = 2;
 

            }

        }
        if (Physics.Raycast(transform.position, -transform.right, out hit, 2f))
        {
            if (((1 << hit.collider.gameObject.layer) & obstacleLayer) != 0 || ((1 << hit.collider.gameObject.layer) & other) != 0)
            {
                ret.x = -2;
            
            }

        }

        return ret;
    }
}
