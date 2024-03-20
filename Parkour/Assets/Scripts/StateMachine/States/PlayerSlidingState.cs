using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSlidingState : State
{
    float myMaxSlideSpeed = 10.0f;
    float mySlopeMultiplier = 1.001f;

    bool myCantSlide;

    public override void OnEnter()
    {
        myCantSlide = false;

        myStateMachine.SetGroundedYVelocity();

        RaycastHit hit;
        if (Physics.SphereCast(myStateMachine.transform.position + Vector3.up * 1.5f, myStateMachine.GetCharacterController().radius, Vector3.down, out hit, 2.0f, myStateMachine.GetWallLayerMask()))
        {
            if (hit.transform.gameObject.layer != myStateMachine.GetSlippyLayerMask())
            {
                if (Vector3.Dot(myStateMachine.transform.forward, hit.normal) < 0.0f)
                {
                    myCantSlide = true;
                    myStateMachine.ChangeState(PlayerStateMachine.eStates.Idle);
                    Debug.Log("STOP");
                    return;
                }
            }

            // Ground The Player
            Vector3 start = myStateMachine.transform.position + Vector3.up * 1.5f;
            Vector3 end = hit.point;
            Vector3 newPos = start + Vector3.down * (myStateMachine.SphereCastStartToMiddleDistance(start, end) + myStateMachine.GetCharacterController().radius);

            myStateMachine.GetCharacterController().enabled = false;
            myStateMachine.transform.position = newPos;
            myStateMachine.GetCharacterController().enabled = true;
        }

        myStateMachine.SetDesiredFOV(100.0f);
        myStateMachine.SetSpeedLinesActive(true);

        myStateMachine.SetHeight(0.5f);
        myStateMachine.GetPlayerAnimator().SetTrigger("slide");
    }

    public override void OnExit()
    {
        myStateMachine.SetBodyRotationX(0.0f);
        myStateMachine.SetHeight(2.0f);
        myStateMachine.SetDesiredFOV(90.0f);

        myStateMachine.SetSpeedLinesActive(false);
        myStateMachine.AdjustLookRotation();

        // This Stops A Animation Bug From Happening
        if (!myCantSlide && myStateMachine.IsGrounded() && !Input.GetButton("Jump"))
        {
            myStateMachine.GetPlayerAnimator().SetTrigger("slidingStop");
            myStateMachine.SetGroundedYVelocity();
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
        if (Physics.SphereCast(myStateMachine.transform.position + Vector3.up * 1.5f, myStateMachine.GetCharacterController().radius, Vector3.down, out hit, 2.0f, myStateMachine.GetWallLayerMask()))
        {
            float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
            myStateMachine.SetBodyRotationX(slopeAngle);

            if (slopeAngle > 1.0f && (Vector3.Dot(Vector3.up, hit.normal) > 0.2f))
            {
                Vector3 slopeDirection = Vector3.Cross(Vector3.Cross(Vector3.up, hit.normal), hit.normal).normalized;
                velocity += slopeDirection * mySlopeMultiplier * slopeAngle * Time.deltaTime;

                slidingDown = true;

                // Ground The Player
                Vector3 start = myStateMachine.transform.position + Vector3.up * 1.5f;
                Vector3 end = hit.point;
                Vector3 newPos = start + Vector3.down * (myStateMachine.SphereCastStartToMiddleDistance(start, end) + myStateMachine.GetCharacterController().radius);

                myStateMachine.GetCharacterController().enabled = false;
                myStateMachine.transform.position = newPos;
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

        if (!slidingDown)
        {
            velocity = Vector3.Lerp(myStateMachine.GetCurrentVelocity(), Vector3.zero, Time.deltaTime * 0.5f);
        }

        myStateMachine.SetVelocityXYZ(velocity.x, velocity.y, velocity.z);

        // Transitions
        if (!myStateMachine.IsGrounded() && !myStateMachine.GroundIsSlippy())
        {
            myStateMachine.ChangeState(PlayerStateMachine.eStates.Falling);
            myStateMachine.SetGroundedYVelocity();
        }
        else if (Input.GetButton("Jump") && slidingDown)
        {
            myStateMachine.ChangeState(PlayerStateMachine.eStates.SlopeJump);
        }
    }

    void AdjustHeight()
    {
        Vector3 velocity = myStateMachine.GetCurrentVelocity();

        RaycastHit hit;
        Physics.SphereCast(myStateMachine.transform.position + Vector3.up, myStateMachine.GetCharacterController().radius, Vector3.down, out hit, 2.0f, myStateMachine.GetWallLayerMask());

        if ((!Input.GetButton("Crouch") && !myStateMachine.GroundIsSlippy() && velocity.magnitude < 6.0f) || 
            (velocity.magnitude < 4.0f && Vector3.Dot(hit.normal, Vector3.up) == 1.0f && !myStateMachine.GroundIsSlippy()) || 
            (myStateMachine.RaycastSlideForward(out hit) && !myStateMachine.GroundIsSlippy()))
        {
            myStateMachine.SetSpeedLinesActive(false);
            myStateMachine.SetDesiredFOV(90.0f);

            myStateMachine.ChangeState(PlayerStateMachine.eStates.Idle);
            myStateMachine.SetHeight(2.0f);
            Debug.Log("<color=red>STOP2</color>");
        }
    }
}