using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallTurning : State
{
    float myActiveTime = 0.0f;

    Vector3 myStartOrientation;
    Vector3 myStartVelocity;

    bool myHasJumped;

    public override void OnEnter()
    {
        myHasJumped = false;
        myActiveTime = 0.0f;

        myStartOrientation = myStateMachine.transform.eulerAngles;
        myStartVelocity = myStateMachine.GetCurrentVelocity();
    }

    public override void OnExit()
    {
        myStateMachine.AdjustLookRotation();
    }

    public override void Tick()
    {
        myStateMachine.ForwardLookAround();

        myActiveTime += Time.deltaTime * 2.0f;

        Turning();
        SlowingDown();

        if (myActiveTime < 1.0f)
        {
            if (Input.GetButtonDown("Jump"))
            {
                myHasJumped = true;
            }
        }
        else
        {
            if (myHasJumped)
            {
                myStateMachine.ChangeState(PlayerStateMachine.eStates.WallJump);
            }
            else
            {
                myStateMachine.ChangeState(PlayerStateMachine.eStates.WallRunFall);
            }
        }
    }

    void Turning()
    {
        //myStateMachine.SetDesiredCameraTilt(-10.0f - (10.0f * (1.0f - EaseOutCirc(Mathf.Clamp01(myActiveTime * 2.0f)))));

        myStateMachine.transform.eulerAngles = new Vector3(0.0f, myStartOrientation.y + 180.0f * EaseOutCirc(Mathf.Clamp01(myActiveTime * 2.0f)), 0.0f);
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
}
