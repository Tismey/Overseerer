using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public abstract class AIbase : MonoBehaviour
{   
    protected Vector3 m_Position;
    private bool b_think = false;
    private List<AIState> states = new List<AIState>();
    public Transform eyePosition;
    private Vector3 previousPosition;
    public bool isPlayer = false;
    public float turnSpeed;
    public mouvementscript moveType;
    public Animator Animator;
    public string teamName;
    public static List<AIbase> Population = new List<AIbase>();
    public float maxViewDistance;
    public float maxViewAngle;
    public LayerMask occlusionLayer;
    public Rigidbody rb;
    public bool canMove = false;
    public bool noGravity = false;

    public Transform righthand;
    public WeaponAbstract[] weapon;
    public int WeaponSelect = 0;
    public LockRoot lockRoot;

    public Healthcontroller health;

    private Rigidbody[] ragdollRigidbodies;
    public GameObject ragdollHolder;


    // Start is called before the first frame update
    void Awake()
    {
        
        
        
        // Désactive tous les Rigidbody au début
        
    }

    public void Start()
    {
        weapon = new WeaponAbstract[9];
        moveType = gameObject.GetComponent<mouvementscript>();
        health = gameObject.GetComponent<Healthcontroller>();

        previousPosition = transform.position;
        Population.Add(this);
        Debug.Log("Population size : " + Population.Count);
        if(eyePosition == null)
        {
            eyePosition = transform;
        }
        if (ragdollHolder != null)
        {
            ragdollRigidbodies = ragdollHolder.GetComponentsInChildren<Rigidbody>(true);
            Debug.Log("Ragdoll rigidbodies : " + ragdollRigidbodies.Length);
            SetRagdollState(false);
        }

        for(int i = 0; i < 9; i++)
        {
            weapon[i] = null;
            Debug.Log("set " + i + "to null");
        }


    }



    // Update is called once per frame
    void FixedUpdate()
    {   
        AIthink();
        
    }

    protected void UpdateAnimator()
    {
        Vector3 displacement = transform.position - previousPosition;
        displacement *= 10;
        Vector3 localDisplacement = transform.InverseTransformDirection(displacement);
        this.Animator.SetFloat("ZSpeed", localDisplacement.z);
        this.Animator.SetFloat("XSpeed", -localDisplacement.x);
        
        previousPosition = transform.position;
    }


    public void SetMoveVector(Vector3 v)
    {
        m_Position = v;
    }

    public abstract void LookTowards(Vector3 v);

    public abstract void AIthink();

    public void PlayState()
    {
        //init state if not done yet
        if(states.Count < 0)return;
        if (!states[states.Count - 1].init())
        {
            
            states[states.Count - 1].StartState(this);
            return;
        }

        if(states[states.Count - 1].WasInterupted())
        {
            
            states[states.Count - 1].SetInterupted(false);
            states[states.Count - 1].Continue();
        }
        //do stuff...
        states[states.Count - 1].act();

        //remove the state if it execution finished
        if (states[states.Count - 1].Ended())
        {
            states[states.Count - 1].Finish();
            states.Remove(states[states.Count - 1]);
        }
    }

    public void AddState(AIState st)
    {
        if(states.Count > 0)
        {
            if (!states[states.Count - 1].init())
            {
                states[states.Count - 1].StartState(this);
            }
            states[states.Count - 1].Interupt();
            states[states.Count - 1].SetInterupted(true);

        }
        
        states.Add(st);
        st.StartState(this);
    }

    public void RemoveState(AIState st)
    {
       
       
        st.Interupt();
        states.Remove(st);
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

    public List<AIbase> GetEnemies()
    {
        List<AIbase> enemies = new List<AIbase>();
        foreach (AIbase ai in Population)
        {
            if (ai != this && ai.teamName != teamName)
            {
                if (IsSeing(ai))
                {
                    enemies.Add(ai);
                }
            }
        }
        return enemies;
    }

    public List<AIbase> GetFriends()
    {
        List<AIbase> friends = new List<AIbase>();
        foreach (AIbase ai in Population)
        {
            if (ai != this && ai.teamName == teamName)
            {
                if (IsSeing(ai))
                {
                    friends.Add(ai);
                }
            }
        }
        return friends;
    }   

    public Vector3 GetEyePosition()
    {
        return eyePosition.position;
    }

    public void SetRagdollState(bool state)
    {
        Vector3 displacement = transform.position - previousPosition;
        foreach (Rigidbody rbs in ragdollRigidbodies)
        {
            rbs.isKinematic = !state;
            rbs.detectCollisions = state;
            var bone = Animator.transform.Find(rb.name);
            if (bone != null)
            {
                rbs.position = bone.position;
                rbs.rotation = bone.rotation;
            }
            
            rbs.velocity = displacement*2;
        }
        Physics.SyncTransforms();
        
        rb.velocity = displacement*2;
    }
}
