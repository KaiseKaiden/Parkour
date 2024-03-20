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
    }

    public override void OnExit()
    {
        myStateMachine.GetCharacterController().enabled = true;
        myStateMachine.AdjustLookRotation();
    }

    public override void Tick()
    {
        myStateMachine.ForwardLookAround();

        myTime += Time.deltaTime * 1.5f;
        myTime = Mathf.Clamp01(myTime);

        Vector3 newPosition = myStartPos + myDiffernce * EaseOutQuad(myTime);

        myStateMachine.transform.position = newPosition;

        if (myTime == 1.0f)
        {
            myStateMachine.ChangeState(PlayerStateMachine.eStates.Idle);
        }
    }

    float EaseOutQuad(float aValue)
    {
        return 1.0f - (1.0f - aValue) * (1.0f - aValue);
    }
}
