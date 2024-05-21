using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AIState
{
    private bool isInit = false;
    private bool hasEnded = false;
    public float StateLengh;
    public float StateTimer = 0f;
    public List<string> AnimationNames;
    public List<AIState> transitions;
    public Animator Animator;
    
    public AIState(float lengh, List<string> st)
    {
        StateLengh = lengh;
        AnimationNames = st;
    }

    public abstract void StartState(AIbase ai);

    public abstract void act();

    public abstract void Interupt();

    public bool Ended()
    {
        return hasEnded;
    }

    public bool init()
    {
        return isInit;
    }
}
