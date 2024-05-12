using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSlidingState : State
{
    float myMaxSlideSpeed = 10.0f;
    float mySlopeMultiplier = 1.001f;
    float myStartSpeed;
    const float mySpeedBoost = 5.0f;
    const float myLengthMarginal = 5.0f;

    bool myCantSlide;

    const float myBaseSlideLength = 7.5f;
    float mySlideLength;
    Vector3 myDesiredPosition;

    bool myIsOnSlope;
    bool myShouldRecalculate;

    public override void OnEnter()
    {
        mySlideLength = myBaseSlideLength;
        myStartSpeed = myStateMachine.GetCurrentVelocityXZ().magnitude + mySpeedBoost;

        myCantSlide = false;
        myIsOnSlope = false;
        myShouldRecalculate = false;

        myStateMachine.SetGroundedYVelocity();

        RaycastHit hit;
        if (Physics.SphereCast(myStateMachine.transform.position + Vector3.up * 1.5f, myStateMachine.GetCharacterController().radius, Vector3.down, out hit, 2.0f, myStateMachine.GetWallLayerMask()))
        {
            if (hit.transform.gameObject.layer != myStateMachine.GetSlippyLayerMask())
            {
                if (Vector3.Dot(myStateMachine.transform.forward, hit.normal) < 0.0f)
                {
                    myCantSlide = true;
                    if (myStateMachine.GetPreviusState() == PlayerStateMachine.eStates.Falling)
                    {
                        myStateMachine.ChangeState(PlayerStateMachine.eStates.IdleLand);
                    }
                    else
                    {
                        myStateMachine.ChangeState(PlayerStateMachine.eStates.Idle);
                    }

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

        CalculateDesiredPosition();
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

        Slide();
        Transitions();

        myStateMachine.slidePos = myDesiredPosition;
    }

    void Slide()
    {
        myIsOnSlope = false;
        float flowMultiplier = 1.0f;

        Vector3 velocity = myStateMachine.GetCurrentVelocity();

        Vector3 controlledVelocity = new Vector3();
        RaycastHit hit;
        if (Physics.SphereCast(myStateMachine.transform.position + Vector3.up * 1.5f, myStateMachine.GetCharacterController().radius, Vector3.down, out hit, 2.5f, myStateMachine.GetWallLayerMask()))
        {
            RaycastHit hit2;
            Physics.Raycast(hit.point + Vector3.up, Vector3.down, out hit2, 1.5f, myStateMachine.GetWallLayerMask());
            float slopeAngle = Vector3.Angle(hit2.normal, Vector3.up);
            myStateMachine.SetBodyRotationX(slopeAngle);

            if (slopeAngle > 1.0f)
            {
                myIsOnSlope = true;
                myShouldRecalculate = true;

                flowMultiplier = 1.0f + (slopeAngle / 40.0f);

                Vector3 slopeDirection = Vector3.Cross(Vector3.Cross(Vector3.up, hit.normal), hit.normal).normalized;
                velocity += slopeDirection * mySlopeMultiplier * slopeAngle * Time.deltaTime;

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
                controlledVelocity = Vector3.Cross(Vector3.up, hit2.normal) * input.x * myMaxSlideSpeed * 2.0f * Time.deltaTime; // This might cause some issues

                // Apply Camera Shake
                myStateMachine.SetScreenShakeIntensity(0.025f);
            }
        }

        if (!myIsOnSlope && myShouldRecalculate)
        {
            CalculateDesiredPosition();
            myShouldRecalculate = false;
        }

        Vector3 directionToPoint = (myDesiredPosition - myStateMachine.transform.position).normalized;
        if (!myIsOnSlope)
        {
            velocity = directionToPoint * myStartSpeed;
        }

        velocity = Vector3.ClampMagnitude(velocity, myMaxSlideSpeed);
        velocity += controlledVelocity;

        myStateMachine.SetVelocityXYZ(velocity.x, velocity.y, velocity.z);

        myStateMachine.AddFlowPoint(flowMultiplier * (velocity.magnitude / myMaxSlideSpeed) * 3.0f * Time.deltaTime);
    }

    void Transitions()
    {
        RaycastHit hit;
        if (Physics.SphereCast(myStateMachine.transform.position + Vector3.up, myStateMachine.GetCharacterController().radius, Vector3.down, out hit, 2.0f, myStateMachine.GetWallLayerMask()))
        {
            RaycastHit hit2;

            Vector3 directionToPoint = (myDesiredPosition - myStateMachine.transform.position).normalized;
            if ((Vector3.Dot(myStateMachine.transform.forward, directionToPoint) <= 0.5f && !myIsOnSlope) ||
                (!Input.GetButton("Crouch") && !myStateMachine.GroundIsSlippy() /*&& TIMER?*/) ||
                (myStateMachine.RaycastSlideForward(out hit2) && !myStateMachine.GroundIsSlippy()) ||
                Vector3.Dot(myStateMachine.transform.forward, hit.normal) < 0.0f)
            {
                myStateMachine.SetSpeedLinesActive(false);
                myStateMachine.SetDesiredFOV(90.0f);

                myStateMachine.ChangeState(PlayerStateMachine.eStates.Idle);
                myStateMachine.SetHeight(2.0f);
            }
        }
        else if (!myStateMachine.IsGrounded() &&
            !myStateMachine.GroundIsSlippy() &&
            !Physics.Raycast(myStateMachine.transform.position + Vector3.up * 1.5f, Vector3.down, 2.0f, myStateMachine.GetWallLayerMask()))
        {
            myStateMachine.ChangeState(PlayerStateMachine.eStates.Falling);
            myStateMachine.SetGroundedYVelocity();
        }
        else if (Input.GetButton("Jump") && myIsOnSlope)
        {
            myStateMachine.ChangeState(PlayerStateMachine.eStates.SlopeJump);
        }
    }

    void CalculateDesiredPosition()
    {
        // Calculate Desired Position
        RaycastHit hit;

        if (Physics.Raycast(myStateMachine.transform.position + Vector3.up * 0.5f, myStateMachine.transform.forward, out hit, mySlideLength, myStateMachine.GetWallLayerMask(), QueryTriggerInteraction.Ignore))
        {
            if (Physics.Raycast(hit.point - myStateMachine.transform.forward * 0.5f, Vector3.down, out hit, 2.0f, myStateMachine.GetWallLayerMask(), QueryTriggerInteraction.Ignore))
            {
                myDesiredPosition = hit.point; // Get Position If Sliding Into Object
            }
        }
        else
        {
            if (Physics.Raycast(myStateMachine.transform.position + Vector3.up * 0.5f + myStateMachine.transform.forward * mySlideLength, Vector3.down, out hit, 2.0f, myStateMachine.GetWallLayerMask(), QueryTriggerInteraction.Ignore))
            {
                if (Physics.CheckCapsule(hit.point + Vector3.up * 0.5f, hit.point + Vector3.up * 2.0f, myStateMachine.GetCharacterController().radius, myStateMachine.GetWallLayerMask(), QueryTriggerInteraction.Ignore)) // Check If Its Gonna Stop Under An Object
                {

                    if (Physics.Raycast(hit.point + Vector3.up * 0.5f + myStateMachine.transform.forward * myLengthMarginal, Vector3.up, 1.5f, myStateMachine.GetWallLayerMask(), QueryTriggerInteraction.Ignore))
                    {
                        // Place Player Outside Of Slide
                        if (Physics.Raycast(myStateMachine.transform.position + Vector3.up * 2.0f, myStateMachine.transform.forward, out hit, Mathf.Infinity, myStateMachine.GetWallLayerMask(), QueryTriggerInteraction.Ignore))
                        {
                            if (Physics.Raycast(hit.point - myStateMachine.transform.forward * 0.5f, Vector3.down, out hit, Mathf.Infinity, myStateMachine.GetWallLayerMask(), QueryTriggerInteraction.Ignore))
                            {
                                myDesiredPosition = hit.point;
                            }
                        }
                    }
                    else
                    {
                        myDesiredPosition = hit.point + myStateMachine.transform.forward * myLengthMarginal;
                    }
                }
                else
                {
                    myDesiredPosition = hit.point; // Get Position On Ground
                }
            }
            else
            {
                myDesiredPosition = myStateMachine.transform.position + myStateMachine.transform.forward * mySlideLength; // Get Position Out In Air
            }
        }

        myDesiredPosition.y = myStateMachine.transform.position.y;
    }

    /*public override void OnEnter()
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
                    if (myStateMachine.GetPreviusState() == PlayerStateMachine.eStates.Falling)
                    {
                        myStateMachine.ChangeState(PlayerStateMachine.eStates.IdleLand);
                    }
                    else
                    {
                        myStateMachine.ChangeState(PlayerStateMachine.eStates.Idle);
                    }

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
        float flowMultiplier = 1.0f;

        Vector3 velocity = myStateMachine.GetCurrentVelocity();

        Vector3 controlledVelocity = new Vector3();
        bool slidingDown = false;
        RaycastHit hit;
        if (Physics.SphereCast(myStateMachine.transform.position + Vector3.up * 1.5f, myStateMachine.GetCharacterController().radius, Vector3.down, out hit, 2.5f, myStateMachine.GetWallLayerMask()))
        {
            RaycastHit hit2;
            Physics.Raycast(hit.point + Vector3.up, Vector3.down, out hit2, 1.5f, myStateMachine.GetWallLayerMask());
            float slopeAngle = Vector3.Angle(hit2.normal, Vector3.up);
            myStateMachine.SetBodyRotationX(slopeAngle);

            if (slopeAngle > 1.0f)
            {
                flowMultiplier = 1.0f + (slopeAngle / 40.0f);

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
                controlledVelocity = Vector3.Cross(Vector3.up, hit2.normal) * input.x * myMaxSlideSpeed * 2.0f * Time.deltaTime; // This might cause some issues

                // Apply Camera Shake
                myStateMachine.SetScreenShakeIntensity(0.025f);
            }
        }

        velocity = Vector3.ClampMagnitude(velocity, myMaxSlideSpeed);
        velocity += controlledVelocity;

        if (!slidingDown && !Physics.SphereCast(myStateMachine.transform.position + Vector3.up * 0.6f, 0.5f, Vector3.up, out hit, 0.8f, myStateMachine.GetWallLayerMask()))
        {
            velocity = Vector3.Lerp(myStateMachine.GetCurrentVelocity(), Vector3.zero, Time.deltaTime * 0.5f);
        }

        myStateMachine.SetVelocityXYZ(velocity.x, velocity.y, velocity.z);

        myStateMachine.AddFlowPoint(flowMultiplier * (velocity.magnitude / myMaxSlideSpeed) * 3.0f * Time.deltaTime);

        // Transitions
        if (!myStateMachine.IsGrounded() && 
            !myStateMachine.GroundIsSlippy() &&
            !Physics.Raycast(myStateMachine.transform.position + Vector3.up * 1.5f, Vector3.down, 2.0f, myStateMachine.GetWallLayerMask()))
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
        }
    }*/
}