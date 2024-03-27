using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State
{
    protected PlayerStateMachine myStateMachine;

    public void Init(PlayerStateMachine aStateMachine)
    {
        myStateMachine = aStateMachine;
    }

    public virtual void OnEnter()
    {

    }

    public virtual void OnExit()
    {

    }

    public virtual void Tick()
    {

    }

    public virtual void AnimDone()
    {

    }

    public virtual bool AnimImpact()
    {
        return false;
    }
}