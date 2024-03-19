using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAirKick : State
{
    const float myKickForce = 4.5f;
    const float myMaxKickForce = 12.0f;

    float myHitFreezeTime;
    bool myHasHit;
    bool myHasJumped;

    GameObject myEnemyTarget;
    Vector3 myStartPosition;
    Vector3 myDeciredAngle;

    float myElapsedTime;

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
        myHasJumped = false;

        myStateMachine.SetSpeedLinesActive(true);

        // Find Target
        myEnemyTarget = null;
        if (myStateMachine.EnemyIsInRange(out myEnemyTarget))
        {
            myStartPosition = myStateMachine.transform.position;
            myElapsedTime = 0.0f;

            myDeciredAngle = (myEnemyTarget.transform.position - myStateMachine.transform.position).normalized;
        }
    }

    public override void OnExit()
    {
        myStateMachine.AdjustLookRotation();

        myStateMachine.SetSpeedLinesActive(false);
    }

    public override void Tick()
    {
        myStateMachine.ForwardLookAround();

        if (myEnemyTarget != null)
        {
            // Set Decired Angle
            Quaternion rotation = Quaternion.LookRotation(new Vector3(myDeciredAngle.x, 0.0f, myDeciredAngle.z));
            myStateMachine.transform.rotation = Quaternion.Lerp(myStateMachine.transform.rotation, rotation, 5.0f * Time.deltaTime);

            myElapsedTime += Time.deltaTime * 2.0f;

            Vector3 direction = (myEnemyTarget.transform.position - myStartPosition);

            myStateMachine.GetCharacterController().enabled = false;
            myStateMachine.transform.position = myStartPosition + direction * EaseInSine(Mathf.Clamp01(myElapsedTime));
            myStateMachine.GetCharacterController().enabled = true;
        }
        else
        {
            myStateMachine.GravityTick();

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

        // Hit Freeze 
        myHitFreezeTime -= Time.unscaledDeltaTime;
        if (myHitFreezeTime < 0.0f)
        {
            Time.timeScale = 1.0f;

            if (myHasJumped)
            {
                myStateMachine.ChangeState(PlayerStateMachine.eStates.KickBoost);
                myHasHit = false;
                myHasJumped = false;
            }
        }

        // Jump Boost
        if (myHasHit)
        {
            if (Input.GetButton("Jump"))
            {
                myHasJumped = true;
            }
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
                Vector3 direction = ((myEnemyTarget.transform.position + Vector3.up) - myStartPosition);
                myStateMachine.SetVelocityY(direction.y);
            }
        }
    }

    public override bool AttackHit()
    {
        if (myStateMachine.GetCurrentState() == PlayerStateMachine.eStates.AirKick)
        {
            if (myEnemyTarget != null)
            {
                myEnemyTarget.GetComponent<Enemy>().KickedAt(myStateMachine.transform.position, myStateMachine.GetCameraTransform().forward * 250.0f);

                Debug.Log("Hit");
                Time.timeScale = 0.0f;
                myHitFreezeTime = 0.2f;
                myHasHit = true;

                myStateMachine.SetScreenShakeIntensity(0.05f);
                return true;
            }
        }

        return false;

        //if (myStateMachine.GetCurrentState() == PlayerStateMachine.eStates.AirKick)
        //{
        //    Debug.Log("Hit");
        //    Time.timeScale = 0.0f;
        //    myHitFreezeTime = 0.2f;
        //    myHasHit = true;

        //    myStateMachine.SetScreenShakeIntensity(0.05f);
        //}
    }

    float EaseInSine(float aValue)
    {
        return 1.0f - Mathf.Cos((aValue * Mathf.PI) * 0.5f);
    }
}
