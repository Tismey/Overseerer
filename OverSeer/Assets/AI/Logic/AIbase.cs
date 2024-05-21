using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.UI.Image;

public abstract class AIbase : MonoBehaviour
{   
    private Vector3 m_Position;
    private bool b_think = false;
    private List<AIState> states;
    private NavMeshAgent nav;
    private Transform eyePosition;
   


    public Animator animator;
    public string teamName;
    public static List<AIbase> Population;
    public float maxViewDistance;
    public float maxViewAngle;
    public LayerMask occlusionLayer;

    // Start is called before the first frame update
    void Awake()
    {
        Population.Add(this);
    }

    

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetMoveVector(Vector3 v)
    {
        m_Position = v;
    }

    public abstract void LookTowards();

    public abstract void AIthink();

    public void PlayState()
    {
        //init state if not done yet
        if (!states[states.Count].init())
        {
            states[states.Count].StartState(this);
        }
        //do stuff...
        states[states.Count].act();

        //remove the state if it execution finished
        if (states[states.Count].Ended())
        {
            states.Remove(states[states.Count]);
        }
    }

    public void AddState(AIState st)
    {
        states[states.Count].Interupt();
        states.Add(st);
    }

    public bool IsSeing(AIbase target)
    {
        if(Physics.Linecast(eyePosition.position, target.eyePosition.position, occlusionLayer)){
            return false;
        }
        Vector2 direction = new Vector2(target.eyePosition.position.x, target.eyePosition.position.z) - new Vector2(eyePosition.position.x, eyePosition.position.z);
        if (Vector2.Angle(new Vector2(eyePosition.forward.x, eyePosition.forward.z), direction) > maxViewAngle) {
            return false;        
        }
        return true;

    }
}
