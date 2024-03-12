using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVaultState : State
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
        desiredPos = startPos + (myStateMachine.GetObstacleVaultPosition() - startPos) * 2.0f;

        differnce = desiredPos - startPos;
        myTime = 0.0f;

        myDeciredAngle = differnce;
        myDeciredAngle.y = 0.0f;
        myDeciredAngle.Normalize();
        Debug.Log("VAULT!");
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

        myTime += Time.deltaTime * 1.5f;
        myTime = Mathf.Clamp01(myTime);

        Vector3 newPosition = startPos + differnce * EaseOutSine(myTime);
        newPosition.y = startPos.y + Mathf.Sin(myTime * Mathf.PI) * ((myStateMachine.GetObstacleVaultPosition().y - startPos.y));
        myStateMachine.transform.position = newPosition;

        myStateMachine.SetCameraHeight(0.5f + (Mathf.Cos(myTime * Mathf.PI * 2.0f) + 1.0f) * 0.5f * 1.5f);
        myStateMachine.SetDesiredCameraTilt(10.0f * Mathf.Sin(myTime * Mathf.PI));

        if (myTime == 1.0f)
        {
            myStateMachine.ChangeState(PlayerStateMachine.eStates.Running);
        }
    }

    float EaseOutBack(float aValue)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1.0f;

        return 1.0f + c3 * Mathf.Pow(aValue - 1.0f, 3.0f) + c1 * Mathf.Pow(aValue - 1.0f, 2.0f);
    }

    float EaseOutSine(float aValue)
    {
        return Mathf.Sin((aValue * Mathf.PI) * 0.5f);
    }
}
