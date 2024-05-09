using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRunningState : State
{
    const float mySpeed = 7.5f;
    const float myBackwardSpeed = 5.0f;
    const float myAcceleration = 8.0f;

    public override void OnEnter()
    {
        
    }

    public override void OnExit()
    {
        myStateMachine.SetGroundedYVelocity();
    }

    public override void Tick()
    {
        myStateMachine.LookAround();

        ManageFlow();
        Move();
        Transitions();
    }

    void Move()
    {
        myStateMachine.GravityTick();

        Vector2 input = myStateMachine.GetInput();
        Vector3 force = myStateMachine.transform.forward * input.y + myStateMachine.transform.right * input.x;

        float speed = mySpeed;
        if (input.y < 0.1f)
        {
            speed = myBackwardSpeed;
        }

        Vector2 vel = new Vector2(myStateMachine.GetCurrentVelocity().x, myStateMachine.GetCurrentVelocity().z);
        float mul = 1.0f;
        if (Vector2.Dot(vel.normalized, input) < 0.0f)
        {
            mul += Mathf.Abs(Vector2.Dot(vel.normalized, input));
        }
        vel = vel + new Vector2(force.x * mySpeed, force.z * speed) * mul * myAcceleration * Time.deltaTime;
        vel = Vector2.ClampMagnitude(vel, mySpeed);
        myStateMachine.SetVelocityXZ(vel.x, vel.y);


        myStateMachine.GetPlayerAnimator().SetFloat("speed", vel.magnitude / mySpeed);
    }

    void Transitions()
    {
        Vector2 input = myStateMachine.GetInput();

        if (!myStateMachine.IsGrounded())
        {
            myStateMachine.ChangeState(PlayerStateMachine.eStates.CayoteFalling);
        }
        else if (myStateMachine.GroundIsSlippy())
        {
            myStateMachine.ChangeState(PlayerStateMachine.eStates.Slide);
        }
        else if (Input.GetButtonDown("Jump"))
        {
            if (input.y > 0.0f && myStateMachine.IsHeadingForObstacle())
            {
                myStateMachine.ChangeState(PlayerStateMachine.eStates.Vault);
            }
            else
            {
                myStateMachine.ChangeState(PlayerStateMachine.eStates.Jump);
            }
        }
        else if (input.magnitude < 0.2f)
        {
            myStateMachine.ChangeState(PlayerStateMachine.eStates.Idle);
        }
        else if (Input.GetButtonDown("Crouch") && input.y > 0.75f)
        {
            myStateMachine.ChangeState(PlayerStateMachine.eStates.Slide);
        }
    }

    void ManageFlow()
    {
        if (myStateMachine.GetFlowPercentage() < 0.33f)
        {
            myStateMachine.AddFlowPoint(10.0f * Time.deltaTime);
        }
        else
        {
            myStateMachine.RemoveFlowPoint(0.5f * Time.deltaTime);
        }
    }
}
