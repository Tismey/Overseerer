using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AiMouvement : mouvementscript
{

    public LayerMask obstacleLayer;
    public LayerMask other;
    public bool avoidObstacle = true;
    private AIbase ai;
    float x = 0;
    float z = 0;

    private void Start()
    {
        ai = GetComponent<AIbase>();
    }
    public override void MoveActor(Vector3 pos,bool sprint, bool crouch)
    {

        float dist = Vector3.Distance(transform.position, pos);
        if (dist < 0.1f)
        {
            this.QuakeMovementFunc(0, 0, sprint, false,crouch);
            return;
        }
        var toTarget = (pos - transform.position);

        Vector3 dir = toTarget / dist; // normalisation safe

        var angToTar = Vector3.SignedAngle(transform.forward,dir, Vector3.up);

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
            

        //x *= dist;
        //z *= dist;
        x = Mathf.Clamp(x, -1, 1);
        z = Mathf.Clamp(z, -1, 1);


        Vector3 desired = new Vector3(x,0,z).normalized;
        Vector3 avoidance = obstacleAvoidance();

        RaycastHit hit;
        Vector3 finalDir = desired;
        if (!ObstacleInFront(out hit)) avoidance = desired;
            finalDir = Vector3.Lerp(desired, avoidance, 0.7f);



        this.QuakeMovementFunc(finalDir.x, finalDir.z,ai.sprint,false,ai.crouch);
        //this.RotateActorTowards(pos,2f);
    }

    public Vector3 obstacleAvoidance()
    {
        Vector3 n = Vector3.one;
        RaycastHit hit;
        if (ObstacleInFront(out hit))
             n = ComputeAvoidance(hit);
        return n;
    }

    bool IsObstacle(int layer)
    {
        return ((1 << layer) & obstacleLayer) != 0
            || ((1 << layer) & other) != 0;
    }

    bool ObstacleInFront(out RaycastHit hit)
    {
        /* Vector3 center = transform.position + transform.forward * 1.5f;
         Vector3 halfExtents = new Vector3(0.6f, 1f, 1.5f);

         return Physics.BoxCast(
             center,
             halfExtents,
             transform.forward,
             out hit,
             transform.rotation,
             0f,
             other
         );*/
        hit = new RaycastHit();
        return false;
    }


    Vector3 ComputeAvoidance(RaycastHit hit)
    {
        Vector3 obstacleNormal = hit.normal;

        // Projeter le mouvement hors de l’obstacle
        Vector3 avoidDir = Vector3.ProjectOnPlane(transform.forward, obstacleNormal);

        return avoidDir.normalized;
    }



}
