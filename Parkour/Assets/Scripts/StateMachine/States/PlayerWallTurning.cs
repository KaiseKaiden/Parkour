using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallTurning : State
{
    float myActiveTime = 0.0f;

    Vector3 myStartOrientation;
    Vector3 myStartVelocity;

    bool myHasJumped;
    bool myHasTurned;

    public override void OnEnter()
    {
        myHasJumped = false;
        myHasTurned = false;
        myActiveTime = 0.0f;

        myStartOrientation = myStateMachine.transform.eulerAngles;
        myStartVelocity = myStateMachine.GetCurrentVelocity();

        myStateMachine.GetPlayerAnimator().SetTrigger("wallTurn");
    }

    public override void OnExit()
    {
        myStateMachine.AdjustLookRotation();
    }

    public override void Tick()
    {
        myStateMachine.ForwardLookAround();

        myActiveTime += Time.deltaTime * 2.0f;

        SlowingDown();

        if (Input.GetButtonDown("Jump"))
        {
            myHasJumped = true;
        }

        if (myHasJumped && myHasTurned)
        {
            myStateMachine.ChangeState(PlayerStateMachine.eStates.WallJump);
        }
    }

    void SlowingDown()
    {
        Vector3 vel = Vector3.zero + myStartVelocity * (1.0f - EaseOutCirc(Mathf.Clamp01(myActiveTime)));
        myStateMachine.SetVelocityXYZ(vel.x, vel.y, vel.z);
    }

    float EaseOutCirc(float aValue)
    {
        return Mathf.Sqrt(1.0f - Mathf.Pow(aValue - 1.0f, 2));
    }

    public override void AnimDone()
    {
        if (!myHasJumped)
        {
            myStateMachine.ChangeState(PlayerStateMachine.eStates.WallRunFall);
        }
    }

    public override bool AnimImpact()
    {
        myHasTurned = true;

        return true;
    }
}
