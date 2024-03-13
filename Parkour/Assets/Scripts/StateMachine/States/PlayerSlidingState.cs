using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSlidingState : State
{
    float myMaxSlideSpeed = 10.0f;
    float mySlopeMultiplier = 1.001f;

    float myEasingValue;
    bool myCantSlide;

    public override void OnEnter()
    {
        myCantSlide = false;
        myEasingValue = 0.0f;

        myStateMachine.SetGroundedYVelocity();

        RaycastHit hit;
        if (Physics.Raycast(myStateMachine.transform.position + Vector3.up, Vector3.down, out hit, 2.0f, myStateMachine.GetWallLayerMask()))
        {
            if (Vector3.Dot(myStateMachine.transform.forward, hit.normal) < 0.0f)
            {
                myCantSlide = true;
                myStateMachine.ChangeState(PlayerStateMachine.eStates.Idle);
                return;
            }
        }

        myStateMachine.SetDesiredCameraTilt(5.0f);
        myStateMachine.SetDesiredFOV(100.0f);

        myStateMachine.SetHeight(0.5f);

        myStateMachine.SetSpeedLinesActive(true);


        myStateMachine.GetPlayerAnimator().SetTrigger("slide");
    }

    public override void OnExit()
    {
        myStateMachine.SetBodyRotationX(0.0f);

        myStateMachine.SetDesiredCameraTilt(0.0f);
        myStateMachine.SetDesiredFOV(90.0f);

        myStateMachine.SetSpeedLinesActive(false);
        myStateMachine.AdjustLookRotation();

        // This Stops A Animation Bug From Happening
        if (!myCantSlide && myStateMachine.IsGrounded() && !Input.GetButton("Jump"))
        {
            myStateMachine.GetPlayerAnimator().SetTrigger("slidingStop");
        }
    }

    public override void Tick()
    {
        myStateMachine.ForwardLookAround();

        AdjustHeight();
        Slide();
    }

    void Slide()
    {
        Vector3 velocity = myStateMachine.GetCurrentVelocity();

        Vector3 controlledVelocity = new Vector3();
        bool slidingDown = false;
        RaycastHit hit;
        if (Physics.Raycast(myStateMachine.transform.position + Vector3.up, Vector3.down, out hit, 2.0f, myStateMachine.GetWallLayerMask()))
        {
            float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
            //myStateMachine.GetBodyTransform().localEulerAngles = new Vector3(slopeAngle, 0.0f, 0.0f);
            myStateMachine.SetBodyRotationX(slopeAngle);

            if (slopeAngle > 0.0f)
            {
                Vector3 slopeDirection = Vector3.Cross(Vector3.Cross(Vector3.up, hit.normal), hit.normal).normalized;
                velocity += slopeDirection * mySlopeMultiplier * slopeAngle * Time.deltaTime;

                slidingDown = true;

                // Ground The Player
                myStateMachine.GetCharacterController().enabled = false;
                myStateMachine.transform.position = hit.point;
                myStateMachine.GetCharacterController().enabled = true;

                // Set Correct Angle
                Quaternion rotation = Quaternion.LookRotation(new Vector3(slopeDirection.x, 0.0f, slopeDirection.z));
                myStateMachine.transform.rotation = Quaternion.Lerp(myStateMachine.transform.rotation, rotation, 5.0f * Time.deltaTime);

                // Apply Sliding Movement
                Vector2 input = myStateMachine.GetInput();
                controlledVelocity = Vector3.Cross(Vector3.up, hit.normal) * input.x * myMaxSlideSpeed * 2.0f * Time.deltaTime;

                // Apply Camera Shake
                myStateMachine.SetScreenShakeIntensity(0.025f);
            }
        }

        velocity = Vector3.ClampMagnitude(velocity, myMaxSlideSpeed);
        velocity += controlledVelocity;

        myStateMachine.SetVelocityXYZ(velocity.x, velocity.y, velocity.z);
        myStateMachine.GravityTick();

        // Transitions
        if (!myStateMachine.IsGrounded())
        {
            //myStateMachine.SetDesiredCameraHeight(2.0f);
            myStateMachine.SetHeight(2.0f);
            myStateMachine.ChangeState(PlayerStateMachine.eStates.Falling);
            myStateMachine.SetGroundedYVelocity();
        }
        else if (Input.GetButton("Jump") && slidingDown)
        {
            //myStateMachine.SetDesiredCameraHeight(2.0f);
            myStateMachine.SetHeight(2.0f);
            myStateMachine.ChangeState(PlayerStateMachine.eStates.SlopeJump);
        }
    }

    void AdjustHeight()
    {
        Vector3 velocity = myStateMachine.GetCurrentVelocity();
        velocity.y = 0.0f;

        RaycastHit hit;
        Physics.Raycast(myStateMachine.transform.position + Vector3.up, Vector3.down, out hit, 2.0f, myStateMachine.GetWallLayerMask());

        if (Input.GetButton("Crouch") && (velocity.magnitude < 2.0f && Vector3.Dot(hit.normal, Vector3.up) == 1.0f))
        {
            myStateMachine.ChangeState(PlayerStateMachine.eStates.Crouch);
        }
        else if (Input.GetButtonUp("Crouch") || (velocity.magnitude < 2.0f && Vector3.Dot(hit.normal, Vector3.up) == 1.0f))
        {
            myStateMachine.SetSpeedLinesActive(false);

            myStateMachine.SetDesiredCameraTilt(0.0f);
            myStateMachine.SetDesiredFOV(90.0f);

            if (Physics.Raycast(myStateMachine.transform.position + Vector3.one * 0.3f, Vector3.up, 1.7f, myStateMachine.GetWallLayerMask()))
            {
                myStateMachine.SetHeight(0.5f);
                myStateMachine.ChangeState(PlayerStateMachine.eStates.Crouch);
            }
            else
            {
                myStateMachine.ChangeState(PlayerStateMachine.eStates.Idle);
                myStateMachine.SetHeight(2.0f);
                //myStateMachine.SetDesiredCameraHeight(2.0f);
            }
        }
        else
        {
            myEasingValue += Time.deltaTime * 2.0f;
            myEasingValue = Mathf.Clamp01(myEasingValue);

            myStateMachine.SetHeight(0.5f);
            myStateMachine.SetCameraHeight(0.5f + myStateMachine.GetAnimationCurves().myCrouchingDownCurve.Evaluate(myEasingValue) * 1.5f);
        }
    }
}