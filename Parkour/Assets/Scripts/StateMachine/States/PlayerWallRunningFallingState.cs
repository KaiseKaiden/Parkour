using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallRunningFallingState : State
{
    float myActiveTime;

    const float myAcceleration = 15.0f;
    const float myMaxSpeed = 7.5f;

    Vector2 myCurrentXZForce;

    public override void OnEnter()
    {
        myActiveTime = 0.0f;

        myCurrentXZForce.x = myStateMachine.GetCurrentVelocityXZ().x;
        myCurrentXZForce.y = myStateMachine.GetCurrentVelocityXZ().z;

        myStateMachine.GetPlayerAnimator().SetTrigger("fall");

        myActiveTime = 1.0f;
        myStateMachine.RemoveFlowPoint(10.0f);
    }

    public override void OnExit()
    {
        myStateMachine.SetSpeedLinesActive(false);

        myStateMachine.GetPlayerAnimator().ResetTrigger("fall");
    }

    public override void Tick()
    {
        myStateMachine.LookAround();

        myActiveTime += Time.deltaTime;
        myStateMachine.RemoveFlowPoint(Time.deltaTime * Mathf.Clamp01(myActiveTime) * 50.0f);

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
            if (myStateMachine.GroundIsSlippy())
            {
                myStateMachine.ChangeState(PlayerStateMachine.eStates.Slide);
            }
            else if (myStateMachine.GetCurrentVelocity().y < -10.0f)
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
            if (myStateMachine.GetCurrentVelocity().y > -10.0f && !Input.GetButton("Crouch"))
            {
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
