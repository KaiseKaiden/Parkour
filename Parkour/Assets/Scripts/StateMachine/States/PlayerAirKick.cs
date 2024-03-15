using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAirKick : State
{
    const float myKickForce = 7.5f;
    const float myMaxKickForce = 12.0f;

    float myHitFreezeTime;
    bool myHasHit;

    public override void OnEnter()
    {
        // Forward Force
        float currentFallSpeed = myStateMachine.GetCurrentVelocity().y;
        Vector3 forwardForce = myStateMachine.GetCurrentVelocityXZ() + myStateMachine.GetCameraTransform().forward * myKickForce;
        Vector3.ClampMagnitude(forwardForce, myMaxKickForce);
        myStateMachine.SetVelocityXYZ(forwardForce.x, currentFallSpeed, forwardForce.z);
        myStateMachine.GetPlayerAnimator().SetTrigger("kick");

        myHitFreezeTime = 0.0f;
        myHasHit = false;

        myStateMachine.SetSpeedLinesActive(true);
    }

    public override void OnExit()
    {
        myStateMachine.AdjustLookRotation();

        myStateMachine.SetSpeedLinesActive(false);
    }

    public override void Tick()
    {
        myStateMachine.GravityTick();
        myStateMachine.ForwardLookAround();

        myHitFreezeTime -= Time.unscaledDeltaTime;
        if (myHitFreezeTime < 0.0f)
        {
            Time.timeScale = 1.0f;
        }
        
        if (myHasHit)
        {
            if (Input.GetButtonDown("Jump"))
            {
                Time.timeScale = 1.0f;
                myStateMachine.ChangeState(PlayerStateMachine.eStates.KickBoost);
            }
        }

        // Transition If Wall Is Hit
        RaycastHit hit;
        if (myStateMachine.RaycastSlideForward(out hit))
        {
            float yVelocity = myStateMachine.GetCurrentVelocity().y;
            if (yVelocity > -0.5f)
            {
                yVelocity = -0.5f;
            }

            myStateMachine.SetVelocityXYZ(0.0f, yVelocity, 0.0f);
            myStateMachine.ChangeState(PlayerStateMachine.eStates.Falling);
        }
    }

    public override void AttackDone()
    {
        if (myStateMachine.GetCurrentState() == PlayerStateMachine.eStates.AirKick)
        {
            if (myStateMachine.IsGrounded())
            {
                myStateMachine.ChangeState(PlayerStateMachine.eStates.Idle);
            }
            else
            {
                myStateMachine.ChangeState(PlayerStateMachine.eStates.Falling);
            }
        }
    }

    public override void AttackHit()
    {
        if (myStateMachine.GetCurrentState() == PlayerStateMachine.eStates.AirKick)
        {
            Debug.Log("Hit");
            Time.timeScale = 0.0f;
            myHitFreezeTime = 0.2f;
            myHasHit = true;

            myStateMachine.SetScreenShakeIntensity(0.05f);
        }
    }
}
