using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLedgeClimbState : State
{
    Vector3 myStartPos;
    Vector3 myDesiredPos;
    float mySpeedMultiplier;
    Vector3 myDiffernce;

    float myTime;

    public override void OnEnter()
    {
        myStateMachine.SetGroundedYVelocity();
        myStateMachine.SetVelocityXZ(0.0f, 0.0f);

        myStateMachine.GetCharacterController().enabled = false;
        myStartPos = myStateMachine.transform.position;
        myDesiredPos = myStateMachine.GetEdgeClimbPosition();
        mySpeedMultiplier = myStateMachine.GetEdgeClimbSpeed();

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

        myTime += Time.deltaTime * 2.0f;
        myTime = Mathf.Clamp01(myTime);

        Vector3 newPosition = myStartPos + myDiffernce * EaseOutQuad(myTime);

        myStateMachine.transform.position = newPosition;

        myStateMachine.SetDesiredCameraTilt(Mathf.Sin(myTime * Mathf.PI) * 8.0f);

        if (myTime == 1.0f)
        {
            myStateMachine.ChangeState(PlayerStateMachine.eStates.Idle);
        }
    }

    float EaseOutBack(float aValue)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1.0f;

        return 1.0f + c3 * Mathf.Pow(aValue - 1.0f, 3.0f) + c1 * Mathf.Pow(aValue - 1.0f, 2.0f);
    }

    float EaseOutQuart(float aValue)
    {
        return 1.0f - Mathf.Pow(1.0f - aValue, 4.0f);
    }

    float EaseOutQuad(float aValue)
    {
        return 1.0f - (1.0f - aValue) * (1.0f - aValue);
    }
}
