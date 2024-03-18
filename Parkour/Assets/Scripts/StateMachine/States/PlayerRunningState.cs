using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRunningState : State
{
    const float mySpeed = 7.5f;
    const float myBackwardSpeed = 5.0f;
    const float myAcceleration = 6.0f;

    float myActiveTime;

    public override void OnEnter()
    {
        myActiveTime = 0.0f;
    }

    public override void OnExit()
    {
        myStateMachine.SetGroundedYVelocity();
        myStateMachine.SetDesiredCameraTilt(0.0f);
        //myStateMachine.SetDesiredCameraHeight(2.0f);

        myStateMachine.GetPlayerAnimator().SetFloat("speed", 0.0f);
    }

    public override void Tick()
    {
        myStateMachine.LookAround();

        Move();
    }

    void Move()
    {
        //myStateMachine.SetDesiredCameraHeight(1.85f + Mathf.Sin(myActiveTime * 10.0f + 90.0f) * 0.15f);
        myStateMachine.GravityTick();


        myActiveTime += Time.deltaTime;
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


        myStateMachine.GetPlayerAnimator().SetFloat("speed", vel.magnitude);

        // Camera Tilt
        myStateMachine.SetDesiredCameraTilt(-Input.GetAxisRaw("Horizontal") * 1.5f);

        // Transitions
        Transitions();
    }

    void Transitions()
    {
        Vector2 input = myStateMachine.GetInput();

        GameObject obj;
        myStateMachine.EnemyIsInRange(out obj);

        if (!myStateMachine.IsGrounded())
        {
            myStateMachine.ChangeState(PlayerStateMachine.eStates.CayoteFalling);
        }
        else if (Input.GetButtonDown("Attack") && myStateMachine.CanKick())
        {
            myStateMachine.ChangeState(PlayerStateMachine.eStates.AirKick);
        }
        else if (Input.GetButtonDown("Jump"))
        {
            if (input.y > 0.0f && myStateMachine.IsHeadingForObstacle())
            {
                if (myStateMachine.GetCurrentVelocityXZ().magnitude < mySpeed * 2.0f)
                {
                    myStateMachine.ChangeState(PlayerStateMachine.eStates.StepClimb);
                }
                else
                {
                    myStateMachine.ChangeState(PlayerStateMachine.eStates.Vault);
                }
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
        else if (Input.GetButtonDown("Crouch"))
        {
            myStateMachine.ChangeState(PlayerStateMachine.eStates.Crouch);
        }
    }
}
