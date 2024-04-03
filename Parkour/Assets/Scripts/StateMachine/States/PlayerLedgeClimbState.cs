using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLedgeClimbState : State
{
    Vector3 myStartPos;
    Vector3 myDesiredPos;
    Vector3 myDiffernce;

    float myTime;

    public override void OnEnter()
    {
        myStateMachine.SetGroundedYVelocity();
        myStateMachine.SetVelocityXZ(0.0f, 0.0f);

        myStateMachine.GetCharacterController().enabled = false;
        myStartPos = myStateMachine.transform.position;
        myDesiredPos = myStateMachine.GetEdgeClimbPosition();

        myDiffernce = myDesiredPos - myStartPos;
        myTime = 0.0f;

        myStateMachine.GetPlayerAnimator().SetTrigger("verticalClimb");
        myStateMachine.GetPlayerAnimator().ResetTrigger("verticalWallRun");
        myStateMachine.SetVelocityXYZ(0.0f, -0.5f, 0.0f);
    }

    public override void OnExit()
    {
        myStateMachine.GetPlayerAnimator().SetFloat("speed", 0.0f);
        myStateMachine.GetCharacterController().enabled = true;
        myStateMachine.AdjustLookRotation();
    }

    public override void Tick()
    {
        myStateMachine.ForwardLookAround();

        if (myStateMachine.GetPlayerAnimator().GetCurrentAnimatorStateInfo(0).IsName("PlayerWallClimb"))
        {
            myTime = myStateMachine.GetPlayerAnimator().GetCurrentAnimatorStateInfo(0).normalizedTime;
        }
        else
        {
            myTime = myStateMachine.GetPlayerAnimator().GetCurrentAnimatorStateInfo(1).normalizedTime;
        }

        Vector3 newPosition = myStartPos + myDiffernce * EaseOutQuad(myTime);
        myStateMachine.transform.position = newPosition;
    }

    float EaseOutQuad(float aValue)
    {
        return 1.0f - (1.0f - aValue) * (1.0f - aValue);
    }

    public override void AnimDone()
    {
        myStateMachine.ChangeState(PlayerStateMachine.eStates.Idle);
    }
}
