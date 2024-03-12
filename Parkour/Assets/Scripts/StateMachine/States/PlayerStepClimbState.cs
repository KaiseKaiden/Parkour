using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStepClimbState : State
{
    Vector3 startPos;
    Vector3 desiredPos;
    Vector3 differnce;
    Vector3 myDeciredAngle;

    float myTime;

    public override void OnEnter()
    {
        myStateMachine.SetGroundedYVelocity();

        myStateMachine.GetCharacterController().enabled = false;
        startPos = myStateMachine.transform.position;
        desiredPos = myStateMachine.GetObstacleVaultPosition();

        differnce = desiredPos - startPos;
        myTime = 0.0f;

        myDeciredAngle = differnce;
        myDeciredAngle.y = 0.0f;
        myDeciredAngle.Normalize();

        myStateMachine.SetVelocityXZ(0.0f, 0.0f);

        myStateMachine.SetDesiredCameraTilt(20.0f);
        Debug.Log("Step Climb");
    }

    public override void OnExit()
    {
        myStateMachine.GetCharacterController().enabled = true;
        myStateMachine.AdjustLookRotation();
        myStateMachine.SetDesiredCameraTilt(0.0f);
    }

    public override void Tick()
    {
        myStateMachine.ForwardLookAround();

        Quaternion rotation = Quaternion.LookRotation(new Vector3(myDeciredAngle.x, 0.0f, myDeciredAngle.z));
        myStateMachine.transform.rotation = Quaternion.Lerp(myStateMachine.transform.rotation, rotation, 5.0f * Time.deltaTime);

        myTime += Time.deltaTime * 2.0f;
        myTime = Mathf.Clamp01(myTime);

        Vector3 newPosition = startPos + differnce * EaseOutQuad(myTime);
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
