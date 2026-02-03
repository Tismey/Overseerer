using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public abstract class AIbase : MonoBehaviour
{   
    protected Vector3 m_Position;
    private bool b_think = false;
    public List<AIState> states = new List<AIState>();
    public Transform eyePosition;
    private Vector3 previousPosition;
    public bool isPlayer = false;
    public float turnSpeed;
    public mouvementscript moveType;
    public Animator Animator;
    public string teamName;
    public static List<AIbase> Population = new List<AIbase>();
    public List<AIbase> enemies = new List<AIbase>();
    public AIbase target;
    public float maxViewDistance = 30f;
    public float maxViewAngle;
    public LayerMask occlusionLayer;
    public Rigidbody rb;

    public bool canMove = false;
    public bool sprint = false;
    public bool crouch = false;
    public bool noGravity = false;

    public Transform righthand;
    public WeaponAbstract[] weapon;
    public int WeaponSelect = 0;
    public LockRoot lockRoot;

    public Healthcontroller health;

    private Rigidbody[] ragdollRigidbodies;
    public GameObject ragdollHolder;

    public float noise;
    public float noiseResetAmount = 5;

    public float perceptionTimer = 0.5f;

    public bool isleader;
    public bool alert = false;
    public bool isdead = false;

    private string animationStr;


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

        perceptionTimer = Random.Range(0.2f, 1.2f);

    }

    public void SetNoise(float amount)
    {
        noise = amount;
    }

    public float GetNoise()
    {
        return noise;
    }

    public static void RunAllAi()
    {
        foreach(AIbase ai in Population)
        {
            ai.AIthink();
        }

       /* foreach (AIbase ai in Population)
        {
            if(ai == null || ai.isdead)
            {
                Population.Remove(ai);
            }
        }*/
    }


    // Update is called once per frame

    protected void UpdateAnimator()
    {

        UpdateAnimation();
        Vector3 displacement = transform.position - previousPosition;
        displacement *= 10 ;
        Vector3 localDisplacement = transform.InverseTransformDirection(displacement);
        this.Animator.SetFloat("ZSpeed", localDisplacement.z);
        this.Animator.SetFloat("XSpeed", localDisplacement.x);
        
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
        if(states.Count - 1 < 0)return;
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

        /*  foreach(AIState stp in states)
          {
              if(st.GetType() == stp.GetType())
              {
                  RemoveState(stp);
                  break;
              }
          }*/
        if (states.Count > 0)
        {
            if (!states[states.Count - 1].init())
            {
                states[states.Count - 1].StartState(this);
            }
            states[states.Count - 1].Interupt();
            states[states.Count - 1].SetInterupted(true);

        }
        else
        {
            states.Add(new Idle());
            AddState(st);
        }
        
        states.Add(st);
        st.StartState(this);
    }

    public void RemoveState(AIState st)
    {

        st.Interupt();
        st.Finish();
        states.Remove(st);
    }

    public void OnDestroy()
    {
        Population.Remove(this);
    }

    public bool IsSeing(AIbase target)
    {
        if (target == null || target.transform == null)
            return false;

        Vector3 eyeA = eyePosition.position;
        Vector3 eyeB = target.eyePosition.position;

        // 1️⃣ Distance (cheap)
        Vector3 diff = eyeB - eyeA;
        if (diff.sqrMagnitude > maxViewDistance * maxViewDistance)
            return false;

        // 2️⃣ Grille : world → grid
        if (!NavGridGen.WorldToGrid(eyeA, out int iA, out int jA, out int hA))
            return false;

        if (!NavGridGen.WorldToGrid(eyeB, out int iB, out int jB, out int hB))
            return false;

        // 3️⃣ Visibilité pré-calculée (BITMASK)
        NodeGrid nodeA = NavGridGen.grid[iA, jA];

        if (nodeA.visibilityMask == null ||
            hA < 0 || hA >= nodeA.visibilityMask.Length)
            return false;
        int bit = NavGridGen.GetRelativeCellBit(iA, jA, iB, jB);
        int chunk = bit >> 6;
        int shift = bit & 63;
        if (nodeA.visibilityMask[hA] == null) return false;
        if (chunk >= nodeA.visibilityMask[hA].Length || chunk < 0) return false; //NULL REFERENCE?

        if ((nodeA.visibilityMask[hA][chunk] & (1UL << shift)) == 0)
            return false;


        // 4️⃣ Angle de vue (FOV)
        diff.y = 0f;
        if (diff.sqrMagnitude < 0.0001f)
            return true;

        float dot = Vector3.Dot(
            eyePosition.forward,
            diff.normalized
        );

        if (dot < Mathf.Cos(maxViewAngle * Mathf.Deg2Rad))
            return false;

        return true;
    }



    public AIbase GetEnemies()
    {
        if ((target != null && !target.isdead) && IsSeing(target) && !Physics.Linecast(GetEyePosition(), target.GetEyePosition(), occlusionLayer)) { alert = true; return target; }

        else if (perceptionTimer > 0)
        {
            perceptionTimer -= Time.deltaTime;
            alert = false;
            return null;
        }
        else
        {
            perceptionTimer = Random.Range(0.2f, 1.2f);
            if (target != null && enemies.Contains(target))
            {
                enemies.Remove(target);
            }
        }
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
        if(enemies.Count <= 0)
        {
            target = null;
            alert = false;
            return target;
        }
        target = enemies[Random.Range(0, enemies.Count - 1)];
        
        if (Physics.Linecast(GetEyePosition(), target.GetEyePosition(), occlusionLayer))
        {
            alert = false;
            return null;
        }
            
        alert = true;
        return target;
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

   public int GetStateDepth()
    {
        return states.Count;
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
            
            rbs.velocity = displacement/Time.deltaTime;
        }
        Physics.SyncTransforms();
        
        rb.velocity = displacement / Time.deltaTime;
    }


    public bool IsLookingAt(AIbase target, float maxAngle = 35f, float maxDistance = 25f)
    {
        if (target == null) return false;

        Vector3 dir = target.GetEyePosition() - GetEyePosition();
        float dist = dir.magnitude;

        //if (maxViewDistance < Vector3.Distance(eyePosition.position, target.eyePosition.position)) return false;

        dir.Normalize();

        float angle = Vector3.Angle(eyePosition.forward, dir);
        return angle <= maxAngle;
    }

    public bool IsInCover(Vector3 dangerPos)
    {
        int i, j, h;
        if (!NavGridGen.WorldToGrid(transform.position, out i, out j, out h))
            return false;

        NodeGrid node = NavGridGen.grid[i, j];

        Vector3 dir = dangerPos - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.0001f)
            return false;

        int dangerDir = AStarTest.GetClosestDirectionIndex(dir);
        if (dangerDir < 0)
            return false;

        return node.cover[h][dangerDir];
    }

    public void SetAnimationString(string str)
    {
        animationStr = str;
    }

    public void UpdateAnimation()
    {
        if (!Animator.GetCurrentAnimatorStateInfo(0).IsName(animationStr))
        {
            Animator.Play(animationStr);
        }
    }

    public bool HasPassedPointXZ(Vector3 point, float radius)
    {
        int i, j, h;
        int ip, jp, hp;
        var vect = NavGridGen.WorldToGrid(transform.position, out i, out j, out h);
        var vectp = NavGridGen.WorldToGrid(point, out ip, out jp, out hp);
        if (i == ip && j == jp) return true;
        return false;

    }






}
