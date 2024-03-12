using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCrouchState : State
{
    const float mySpeed = 2.5f;
    const float myAcceleration = 2.0f;
    bool myIsCrouching;
    float myEasingValue;

    public override void OnEnter()
    {
        myIsCrouching = true;
        if (myStateMachine.GetHeight() == 2.0f)
        {
            myEasingValue = 0.0f;
        }
        else
        {
            myEasingValue = 1.0f;
        }
    }

    public override void OnExit()
    {
        myStateMachine.SetDesiredCameraHeight(2.0f);
        myStateMachine.SetHeight(2.0f);
    }

    public override void Tick()
    {
        myStateMachine.LookAround();

        Move();
    }

    void Move()
    {
        if (!Input.GetButton("Crouch") && myIsCrouching)
        {
            myIsCrouching = false;
            myEasingValue = 0.0f;
        }

        if (myIsCrouching)
        {
            myEasingValue += Time.deltaTime * 3.0f;
            myEasingValue = Mathf.Clamp01(myEasingValue);

            myStateMachine.SetCameraHeight(0.5f + myStateMachine.GetAnimationCurves().myCrouchingDownCurve.Evaluate(myEasingValue) * 1.5f);
            myStateMachine.SetHeight(0.5f);
        }
        else if (!Physics.Raycast(myStateMachine.transform.position + Vector3.one * 0.3f, Vector3.up, 1.7f, myStateMachine.GetWallLayerMask()))
        {
            myEasingValue += Time.deltaTime * 3.0f;
            myEasingValue = Mathf.Clamp01(myEasingValue);

            myStateMachine.SetCameraHeight(0.5f + myStateMachine.GetAnimationCurves().myStandingUpCurve.Evaluate(myEasingValue) * 1.5f);
            myStateMachine.SetHeight(2.0f);
        }

        // Apply Movement Force
        Vector2 input = myStateMachine.GetInput();
        Vector3 force = myStateMachine.transform.forward * input.y + myStateMachine.transform.right * input.x;

        myStateMachine.GravityTick();

        if (input.magnitude > 0.1f)
        {
            Vector2 vel = new Vector2(myStateMachine.GetCurrentVelocity().x, myStateMachine.GetCurrentVelocity().z);
            float mul = 1.0f;
            if (Vector2.Dot(vel.normalized, input) < 0.0f)
            {
                mul += Mathf.Abs(Vector2.Dot(vel.normalized, input));
            }
            vel = vel + new Vector2(force.x * mySpeed, force.z * mySpeed) * mul * myAcceleration * Time.deltaTime;
            vel = Vector2.ClampMagnitude(vel, mySpeed);

            myStateMachine.SetVelocityXZ(vel.x, vel.y);
        }
        else
        {
            Vector3 vel = Vector3.Lerp(myStateMachine.GetCurrentVelocity(), Vector3.zero, Time.deltaTime * 5.0f);
            myStateMachine.SetVelocityXZ(vel.x, vel.z);
        }

        // Camera Tilt
        myStateMachine.SetDesiredCameraTilt(-Input.GetAxisRaw("Horizontal") * 0.75f);

        // Transitions
        if (!myStateMachine.IsGrounded())
        {
            myStateMachine.ChangeState(PlayerStateMachine.eStates.Falling);
            myStateMachine.SetGroundedYVelocity();
        }
        else if (!myIsCrouching && myEasingValue == 1.0f)
        {
            myStateMachine.ChangeState(PlayerStateMachine.eStates.Idle);
        }
    }
}
