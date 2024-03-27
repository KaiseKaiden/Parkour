using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerKickBoost : State
{
    const float myJumpForce = 8.5f;
    const float myAcceleration = 15.0f;
    const float myMaxSpeed = 7.5f;

    Vector2 myCurrentXZForce;

    public override void OnEnter()
    {
        //myStateMachine.SetVelocityY(myJumpForce);
        Vector3 dir = myStateMachine.transform.forward;
        myStateMachine.SetVelocityXYZ(dir.x * myMaxSpeed, myJumpForce, dir.z * myMaxSpeed);

        myCurrentXZForce.x = myStateMachine.GetCurrentVelocityXZ().x;
        myCurrentXZForce.y = myStateMachine.GetCurrentVelocityXZ().z;

        myStateMachine.GetPlayerAnimator().SetTrigger("jump");

        myStateMachine.SetSpeedLinesActive(true);

        AudioManager.Instance.PlaySound(AudioManager.eSound.KickBoost, myStateMachine.transform.position);
    }

    public override void OnExit()
    {
        myStateMachine.SetSpeedLinesActive(false);
    }

    public override void Tick()
    {
        myStateMachine.LookAround();

        Movement();
        Fall();
    }

    void Fall()
    {
        myStateMachine.GravityTick();

        // Transitions
        Vector2 input = myStateMachine.GetInput();

        RaycastHit hit;
        if (Input.GetButtonDown("Attack"))
        {
            myStateMachine.ChangeState(PlayerStateMachine.eStates.AirKick);
        }
        else if (myStateMachine.GetEdgeHit())
        {
            myStateMachine.ChangeState(PlayerStateMachine.eStates.LedgeClimb);
        }
        if (input.y > 0.0f && myStateMachine.IsHeadingForObstacle())
        {
            myStateMachine.ChangeState(PlayerStateMachine.eStates.Vault);
        }
        else if (myStateMachine.RaycastForward(out hit))
        {
            myStateMachine.ChangeState(PlayerStateMachine.eStates.WallRun);
        }
        else if (myStateMachine.WallRunnLeftTransition() && !Input.GetButton("Crouch"))
        {
            // Wallrun Left
            myStateMachine.ChangeState(PlayerStateMachine.eStates.WallRunH);
        }
        else if (myStateMachine.WallRunnRightTransition() && !Input.GetButton("Crouch"))
        {
            // Wallrun Right
            myStateMachine.ChangeState(PlayerStateMachine.eStates.WallRunH);
        }
        else if (myStateMachine.GetCharacterController().velocity.y < 0)
        {
            myStateMachine.ChangeState(PlayerStateMachine.eStates.Falling);
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
