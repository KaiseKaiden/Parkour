using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallRunningFallingState : State
{
    const float myAcceleration = 15.0f;
    const float myMaxSpeed = 7.5f;

    Vector2 myCurrentXZForce;

    //bool myCanWallRun;

    public override void OnEnter()
    {
        myCurrentXZForce.x = myStateMachine.GetCurrentVelocityXZ().x;
        myCurrentXZForce.y = myStateMachine.GetCurrentVelocityXZ().z;

        //myCanWallRun = true;
        //if (myStateMachine.WallRunnLeftTransition() || myStateMachine.WallRunnRightTransition())
        //{
        //    myCanWallRun = false;
        //}

        myStateMachine.GetPlayerAnimator().SetTrigger("fall");
    }

    public override void OnExit()
    {
        myStateMachine.SetSpeedLinesActive(false);

        myStateMachine.GetPlayerAnimator().ResetTrigger("fall");
    }

    public override void Tick()
    {
        myStateMachine.LookAround();

        Movement();
        Fall();
    }

    void Fall()
    {
        myStateMachine.GravityTick();

        if (myStateMachine.GetCurrentVelocity().y < -10.0f)
        {
            myStateMachine.SetSpeedLinesActive(true);
        }

        // Transitions
        if (myStateMachine.IsGrounded())
        {
            if (myStateMachine.GetCurrentVelocity().y < -10.0f)
            {
                if (Input.GetButton("Crouch"))
                {
                    myStateMachine.ChangeState(PlayerStateMachine.eStates.Roll);
                }
                else
                {
                    myStateMachine.ChangeState(PlayerStateMachine.eStates.HardLand);
                }
            }
            else
            {
                if (Input.GetButton("Crouch"))
                {
                    myStateMachine.ChangeState(PlayerStateMachine.eStates.Slide);
                }
                else
                {
                    myStateMachine.ChangeState(PlayerStateMachine.eStates.IdleLand);
                }
            }
        }
        else
        {
            if (Input.GetButtonDown("Attack"))
            {
                myStateMachine.ChangeState(PlayerStateMachine.eStates.AirKick);
            }
            else if (myStateMachine.GetCurrentVelocity().y > -10.0f && !Input.GetButton("Crouch"))
            {
                RaycastHit hit;
                if (myStateMachine.GetEdgeHit() && Vector3.Dot(myStateMachine.transform.forward, myStateMachine.GetCurrentVelocityXZ()) > 0)
                {
                    myStateMachine.ChangeState(PlayerStateMachine.eStates.LedgeClimb);
                }
            }
        }
    }

    void Movement()
    {
        Vector2 input = myStateMachine.GetInput();
        Vector3 force = myStateMachine.transform.forward * input.y + myStateMachine.transform.right * input.x;

        myCurrentXZForce += new Vector2(force.x, force.z) * myAcceleration * Time.deltaTime;
        myCurrentXZForce = Vector2.ClampMagnitude(myCurrentXZForce, myMaxSpeed);

        myStateMachine.SetVelocityXZ(myCurrentXZForce.x, myCurrentXZForce.y);
    }
}