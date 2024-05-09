using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHorizontalWallRunningState : State
{
    float myActiveTime;

    const float myRunningSpeed = 7.5f;
    const float myJumpForce = 6.5f;
    Vector3 myVelocity;
    Vector3 myDeciredAngle;

    bool myIsRight;

    float myTimeAfterLeavingEdge;

    public override void OnEnter()
    {
        myActiveTime = 0.0f;
        myStateMachine.AddFlowPoint(10.0f);

        myStateMachine.SetSpeedLinesActive(true);
        myTimeAfterLeavingEdge = 0.0f;

        RaycastHit hit;
        if (myStateMachine.RaycastForLeft(out hit))
        {
            // Move To The Wall
            Vector3 desired = (hit.point + hit.normal * myStateMachine.GetCharacterController().radius) - myStateMachine.transform.position;
            myStateMachine.GetCharacterController().Move(desired);


            myDeciredAngle = Vector3.Cross(hit.normal, Vector3.up);

            myVelocity = myDeciredAngle * myRunningSpeed;
            myVelocity.y = 1.0f;
            myStateMachine.SetVelocityXYZ(myVelocity.x, myVelocity.y, myVelocity.z);

            myIsRight = false;

            myStateMachine.GetPlayerAnimator().SetTrigger("wallRunLeft");

            return;
        }

        if (myStateMachine.RaycastForRight(out hit))
        {
            // Move To The Wall
            Vector3 desired = (hit.point + hit.normal * myStateMachine.GetCharacterController().radius) - myStateMachine.transform.position;
            myStateMachine.GetCharacterController().Move(desired);


            myDeciredAngle = -Vector3.Cross(hit.normal, Vector3.up);

            myVelocity = myDeciredAngle * myRunningSpeed;
            myVelocity.y = 1.0f;
            myStateMachine.SetVelocityXYZ(myVelocity.x, myVelocity.y, myVelocity.z);

            myIsRight = true;

            myStateMachine.GetPlayerAnimator().SetTrigger("wallRunRight");

            return;
        }
    }

    public override void OnExit()
    {
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
        myActiveTime += Time.deltaTime;
        //myStateMachine.RemoveFlowPoint(Time.deltaTime * myActiveTime * 10.0f);

        myVelocity.y -= 5.0f * Time.deltaTime;
        myStateMachine.SetVelocityXYZ(myVelocity.x, myVelocity.y, myVelocity.z);

        RaycastHit hit;
        if (myStateMachine.GroundIsSlippy())
        {
            myStateMachine.ChangeState(PlayerStateMachine.eStates.Slide);
        }
        else if (myStateMachine.GetEdgeHit())
        {
            myStateMachine.ChangeState(PlayerStateMachine.eStates.LedgeClimb);
        }
        else if (Physics.Raycast(myStateMachine.transform.position + Vector3.up, new Vector3(myDeciredAngle.x, 0.0f, myDeciredAngle.z), 1.0f, myStateMachine.GetWallLayerMask()))
        {
            myStateMachine.ChangeState(PlayerStateMachine.eStates.Falling);
        }
        else if (Input.GetButtonDown("Jump"))
        {
            if (myIsRight)
            {
                myStateMachine.ChangeState(PlayerStateMachine.eStates.WallJumpLeft);
            }
            else
            {
                myStateMachine.ChangeState(PlayerStateMachine.eStates.WallJumpRight);
            }
        }
        else if (myStateMachine.GetCharacterController().velocity.y < -5.0f)
        {
            myStateMachine.ChangeState(PlayerStateMachine.eStates.Falling);
        }
        else if (Input.GetButton("Crouch"))
        {
            myStateMachine.ChangeState(PlayerStateMachine.eStates.Falling);
        }
        else if (myIsRight && !myStateMachine.RaycastForRight(out hit))
        {
            myTimeAfterLeavingEdge += Time.deltaTime;
            if (myTimeAfterLeavingEdge > 0.2f)
            {
                myStateMachine.ChangeState(PlayerStateMachine.eStates.Falling);
            }
        }
        else if (!myIsRight && !myStateMachine.RaycastForLeft(out hit))
        {
            myTimeAfterLeavingEdge += Time.deltaTime;
            if (myTimeAfterLeavingEdge > 0.2f)
            {
                myStateMachine.ChangeState(PlayerStateMachine.eStates.Falling);
            }
        }
        else if (myStateMachine.IsGrounded())
        {
            myStateMachine.ChangeState(PlayerStateMachine.eStates.Running);
        }
    }
}

