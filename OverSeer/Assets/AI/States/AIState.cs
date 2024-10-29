using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AIState
{
    private bool isInit = false;
    protected bool hasEnded = false;
    protected bool hasInterupted = false;
    public float StateLengh;
    public float StateTimer = 0f;
    public List<string> AnimationNames;
    public List<AIState> transitions;
    public AIbase ai;
    public Animator Animator;
    
    public AIState(){}

    public void StartState(AIbase ai)
    {
        this.ai = ai;
        this.Animator = ai.Animator;
        isInit = true;
        Setup();
    }

    public abstract void act();

    public abstract void Interupt();

    public abstract void Setup();

    public bool Ended()
    {
        return hasEnded;
    }
    
    public abstract void Continue();

    public bool WasInterupted()
    {
        return hasInterupted;
    }
    public abstract void Finish();

    public void SetInterupted(bool interupted)
    {
        hasInterupted = interupted;
    }




    public bool init()
    {
        return isInit;
    }


}
