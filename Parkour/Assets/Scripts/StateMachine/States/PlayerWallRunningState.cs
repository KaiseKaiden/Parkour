using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallRunningState : State
{
    const float myClimbingSpeed = 7.5f;
    Vector3 myUpDirection;
    Vector3 myVelocity;
    Vector3 myDeciredAngle;

    float myActiveTime;

    public override void OnEnter()
    {
        RaycastHit hit;
        if (myStateMachine.RaycastForward(out hit))
        {
            // Move To The Wall
            Vector3 desired = (hit.point + hit.normal * myStateMachine.GetCharacterController().radius) - myStateMachine.transform.position;
            myStateMachine.GetCharacterController().Move(desired);

            Vector3 leftDir = Vector3.Cross(hit.normal, Vector3.up);

            myDeciredAngle = -hit.normal;
            myUpDirection = -Vector3.Cross(leftDir, myDeciredAngle);

            myStateMachine.SetDesiredAngle(-myDeciredAngle); // Set This For When/If You Wall Jump Later

            myVelocity = myUpDirection * myClimbingSpeed;
            myStateMachine.SetVelocityXYZ(myVelocity.x, myVelocity.y, myVelocity.z);
        }

        myActiveTime = 0.0f;

        myStateMachine.SetSpeedLinesActive(true);
    }

    public override void OnExit()
    {
        myStateMachine.SetDesiredCameraTilt(0.0f);
        myStateMachine.AdjustLookRotation();
        myStateMachine.SetSpeedLinesActive(false);
    }

    public override void Tick()
    {
        myStateMachine.ForwardLookAround();

        // Set Correct Angle
        Quaternion rotation = Quaternion.LookRotation(new Vector3(myDeciredAngle.x, 0.0f, myDeciredAngle.z));
        myStateMachine.transform.rotation = Quaternion.Lerp(myStateMachine.transform.rotation, rotation, 5.0f * Time.deltaTime);

        Move();
    }

    void Move()
    {
        myVelocity.y -= 10.0f * Time.deltaTime;
        myStateMachine.SetVelocityXYZ(myVelocity.x, myVelocity.y, myVelocity.z);

        myActiveTime += Time.deltaTime * 10.0f;
        myStateMachine.SetDesiredCameraTilt(Mathf.Sin(myActiveTime) * 10.0f);

        if (myStateMachine.GetCharacterController().velocity.y < 0.0f)
        {
            myStateMachine.ChangeState(PlayerStateMachine.eStates.Falling);
        }
        else if (myStateMachine.GetEdgeHit())
        {
            myStateMachine.ChangeState(PlayerStateMachine.eStates.LedgeClimb);
        }
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            myStateMachine.ChangeState(PlayerStateMachine.eStates.WallTurn);
        }
    }
}
