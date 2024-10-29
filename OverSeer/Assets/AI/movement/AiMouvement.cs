using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AiMouvement : mouvementscript
{

    public LayerMask obstacleLayer;
    public LayerMask other;
    public override void MoveActor(Vector3 pos)
    {
        if(Vector3.Distance(transform.position, pos) < 0.1f)
        {
            this.QuakeMovementFunc(0, 0, false, false);
            return;
        }
       
        var angToTar = Vector3.SignedAngle( pos - transform.position, transform.forward, transform.up);
        var x = 0;
        var z = 0;
        if (angToTar < 70 && angToTar > -70)
        {
            z = 1;
        }
        else if (angToTar > 110 || angToTar < -110)
        {
            z = -1;
        }

        if (angToTar > 20 && angToTar < 160)
        {
            x = 1;
        }
        else if (angToTar < -20 && angToTar > -160)
        {
            x = -1;
        }
        var t = obstacleAvoidance();
        x += (int)t.x;


        x = Mathf.Clamp(x, -1, 1);
        z = Mathf.Clamp(z, -1, 1);
        

        this.QuakeMovementFunc(-x, z,false,false);
        this.RotateActorTowards(pos, 5f);
    }

    public Vector2 obstacleAvoidance()
    {

        Vector2 ret = new Vector2(0, 0);
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward + transform.right, out hit, 2f))
        {
            if (hit.collider.gameObject.layer == obstacleLayer || hit.collider.gameObject.layer == other )
            {
                ret.x = -1;
            }
        }
        if (Physics.Raycast(transform.position, transform.forward - transform.right, out hit, 2f))
        {
            if (hit.collider.gameObject.layer == obstacleLayer || hit.collider.gameObject.layer == other)
            {
                ret.x = 1;
            }
        }

        return ret;
    }
}
