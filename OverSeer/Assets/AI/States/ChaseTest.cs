using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ChaseTest : AIState
{
        public Transform pos;
        private Moving m;
      
        // Start is called before the first frame update
        public ChaseTest(Transform pos)
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


           if(Vector3.Distance(ai.transform.position, pos.position) > 5f)
           {
                m = new Moving(pos);
                ai.AddState(m);
                return;
           }
           else
           {
                ai.AddState(new Attack(6f,1f));
           }


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
