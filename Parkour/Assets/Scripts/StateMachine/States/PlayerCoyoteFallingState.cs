using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCoyoteFallingState : State
{
    const float myAcceleration = 15.0f;
    const float myMaxSpeed = 7.5f;

    Vector2 myCurrentXZForce;

    bool myCanWallRun;

    float myActiveTime;

    public override void OnEnter()
    {
        myCurrentXZForce.x = myStateMachine.GetCurrentVelocityXZ().x;
        myCurrentXZForce.y = myStateMachine.GetCurrentVelocityXZ().z;

        myCanWallRun = true;
        if (myStateMachine.WallRunnLeftTransition() || myStateMachine.WallRunnRightTransition())
        {
            myCanWallRun = false;
        }

        myActiveTime = 0.0f;
        myStateMachine.GetPlayerAnimator().SetTrigger("fall");
    }

    public override void OnExit()
    {

    }

    public override void Tick()
    {
        myStateMachine.LookAround();

        Movement();
        Fall();
    }

    void Fall()
    {
        myActiveTime += Time.deltaTime;
        myStateMachine.GravityTick();

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
            if (Input.GetButton("Jump") && myActiveTime < 0.5f)
            {
                myStateMachine.ChangeState(PlayerStateMachine.eStates.Jump);
            }
            else if (myActiveTime > 0.3f)
            {
                myStateMachine.ChangeState(PlayerStateMachine.eStates.Falling);
            }
            else if (myStateMachine.GetCurrentVelocity().y > -15.0f && !Input.GetButton("Crouch"))
            {
                RaycastHit hit;
                if (myStateMachine.WallRunnLeftTransition() && !Input.GetButton("Crouch") && myCanWallRun)
                {
                    // Wallrun Left
                    myStateMachine.ChangeState(PlayerStateMachine.eStates.WallRunH);
                }
                else if (myStateMachine.WallRunnRightTransition() && !Input.GetButton("Crouch") && myCanWallRun)
                {
                    // Wallrun Right
                    myStateMachine.ChangeState(PlayerStateMachine.eStates.WallRunH);
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
