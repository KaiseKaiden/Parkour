using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEdgeClimbHippState : State
{
    Vector3 startPos;
    Vector3 desiredPos;
    Vector3 differnce;

    float myTime;

    public override void OnEnter()
    {
        myStateMachine.SetGroundedYVelocity();


        myStateMachine.GetCharacterController().enabled = false;
        startPos = myStateMachine.transform.position;
        desiredPos = myStateMachine.GetEdgeClimbPosition();

        differnce = desiredPos - startPos;
        myTime = 0.0f;
    }

    public override void OnExit()
    {
        myStateMachine.GetCharacterController().enabled = true;
    }

    public override void Tick()
    {
        myStateMachine.ForwardLookAround();

        myTime += Time.deltaTime;
        myTime = Mathf.Clamp01(myTime);

        Vector3 newPosition = startPos + differnce * EaseOutQuart(myTime);
        newPosition.y = startPos.y + differnce.y * EaseOutBack(myTime);

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
}
