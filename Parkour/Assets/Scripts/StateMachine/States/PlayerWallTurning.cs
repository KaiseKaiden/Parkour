using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallTurning : State
{
    float myActiveTime = 0.0f;

    Vector3 myStartOrientation;
    Vector3 myStartVelocity;

    public override void OnEnter()
    {
        myActiveTime = 0.0f;

        myStartOrientation = myStateMachine.transform.eulerAngles;
        myStartVelocity = myStateMachine.GetCurrentVelocity();

        myStateMachine.SetDesiredCameraTilt(-20.0f);
    }

    public override void OnExit()
    {
        myStateMachine.SetDesiredCameraTilt(0.0f);
        myStateMachine.AdjustLookRotation();
    }

    public override void Tick()
    {
        myStateMachine.ForwardLookAround();

        myActiveTime += Time.deltaTime;

        Turning();
        SlowingDown();

        if (myActiveTime < 0.6f)
        {
            if (Input.GetButtonDown("Jump"))
            {
                myStateMachine.ChangeState(PlayerStateMachine.eStates.WallJump);
            }
        }
        else
        {
            myStateMachine.ChangeState(PlayerStateMachine.eStates.WallRunFall);
        }
    }

    void Turning()
    {
        myStateMachine.SetDesiredCameraTilt(-10.0f - (10.0f * (1.0f - EaseOutCirc(Mathf.Clamp01(myActiveTime * 2.0f)))));

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
