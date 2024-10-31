using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ChaseTest : AIState
{
        public Vector3 pos;
        private Moving m;
      
        // Start is called before the first frame update
        public ChaseTest(Vector3 pos)
        {
            this.pos = pos;
        }
        public override void Setup()
        {
            m = new Moving(pos);
            ai.AddState(m);

        }
        public override void act()
        {
            m = new Moving(pos);
            ai.AddState(m);


            //if (!hasPath)
            //{
            //    Debug.Log("No path found");
            //  hasEnded = true;
            //return;
            //}



        }

        public override void Interupt()
        {
            
            //this.Animator.SetBool("Idle", false);
        }

        public override void Finish()
        {
            ai.canMove = false;
            //this.Animator.SetBool("Idle", false);
        }

        public override void Continue()
        {
            
            
            //this.Animator.SetBool("Idle", true);
        }
    }
