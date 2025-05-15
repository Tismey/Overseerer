using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


    public class MovingAndClimb : AIState
    {
        public Vector3 pos;
        public Transform tran;
        private Vector3 lead;
        public List<Vector3> path;
        public NavMeshPath nav = new NavMeshPath();
        private bool hasPath;
        private int currentCorner = 0;
        private bool isVec = false;
        private bool isTrans = false;
        private Vector3 maxClimb = new Vector3(0,10,0);
        // Start is called before the first frame update
        public MovingAndClimb(Vector3 pos)
        {
            this.pos = pos;
            isVec = true;
        }
        public MovingAndClimb(Transform t)
        {
            tran = t;
            this.pos = t.position;
            isTrans = true;
        }
        public override void Setup()
        {

            this.hasPath = NavMesh.CalculatePath(ai.transform.position, pos, NavMesh.AllAreas, nav);
        }
        public override void act()
        {
            if (isTrans)
            {
                lead = tran.position - pos;
                pos = tran.position;
                ai.LookTowards(pos);
            }

            ai.canMove = true;
        //this.hasPath = NavMesh.CalculatePath(ai.transform.position, pos, NavMesh.AllAreas, nav);
            RaycastHit hit;
            if (Physics.Raycast(ai.transform.position, ai.transform.forward, out hit, 2f))
            {
                if (hit.collider.gameObject.layer != 6)
                {

                    return;
                }
                if (!Physics.Raycast(ai.transform.position + maxClimb, ai.transform.forward, out hit, 2f))
                {
                    
                     for (float i = 0; i < maxClimb.y; i += 0.1f)
                     {
                         if (Physics.Raycast(ai.transform.position + new Vector3(0, maxClimb.y - i, 0), ai.transform.forward, out hit, 2f))
                         {
                              if (hit.collider.gameObject.layer != 6)
                              {

                                  continue;
                              }
                              hit.normal = new Vector3(hit.normal.x, 0, hit.normal.z);
                              ai.AddState(new Climb(hit.point,hit.normal));
                              return;
                         }
                     }
                    
                   
                }
            }
        if (!hasPath || nav.status == NavMeshPathStatus.PathPartial)
            {
                Debug.Log("No path found");
                ai.SetMoveVector(pos);
                this.hasPath = NavMesh.CalculatePath(ai.transform.position, pos + (lead * Vector3.Distance(pos, ai.transform.position) * 2), NavMesh.AllAreas, nav);
            return;
            }

            if (Vector3.Distance(pos, ai.transform.position) <= 3.1f)
            {
                
                this.hasEnded = true;
                return;
            }
            if (currentCorner >= nav.corners.Length)
            {
                currentCorner = 0;
                return;
            }
            Vector3 aiPosition = ai.transform.position;
            Vector3 cornerPosition = nav.corners[currentCorner];

            // Ignore the y-axis by setting both y values to 0.
            aiPosition.y = 0;
            cornerPosition.y = 0;

            if (Vector3.Distance(aiPosition, cornerPosition) > 2f)
            {
               
                ai.SetMoveVector(nav.corners[currentCorner]);
                this.hasPath = NavMesh.CalculatePath(ai.transform.position, pos + (lead * Vector3.Distance(pos, ai.transform.position) * 2), NavMesh.AllAreas, nav);
                currentCorner = 0;


            }
            else
            {
              
                if (isTrans)
                {

                }
                currentCorner++;
            }
           



        }

        public void UpdatePosition(Vector3 pos)
        {
            this.pos = pos;
        }

        public override void Interupt()
        {
            //Debug.Log(this.ai);
            ai.canMove = false;
            //this.Animator.SetBool("Idle", false);
        }

        public override void Finish()
        {
            ai.canMove = false;
            this.hasEnded = true;
            //this.Animator.SetBool("Idle", false);
        }

        public override void Continue()
        {
            ai.canMove = true;
            this.hasPath = NavMesh.CalculatePath(ai.transform.position, pos, NavMesh.AllAreas, nav);
            //this.Animator.SetBool("Idle", true);
        }
    }


