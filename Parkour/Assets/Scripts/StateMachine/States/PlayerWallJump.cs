using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallJump : State
{
    const float myJumpForce = 4.5f;
    const float myAcceleration = 15.0f;
    const float myMaxSpeed = 7.5f;

    Vector2 myCurrentXZForce;

    public override void OnEnter()
    {
        Vector3 forward = myStateMachine.transform.forward;
        myStateMachine.SetVelocityXZ(forward.x * myMaxSpeed, forward.z * myMaxSpeed);
        myStateMachine.SetVelocityY(myJumpForce);

        myCurrentXZForce.x = myStateMachine.GetCurrentVelocityXZ().x;
        myCurrentXZForce.y = myStateMachine.GetCurrentVelocityXZ().z;

        myStateMachine.GetPlayerAnimator().SetTrigger("jumpFrom180");
        AudioManager.Instance.PlaySound(AudioManager.eSound.Jump, myStateMachine.transform.position);
    }

    public override void OnExit()
    {
        myStateMachine.AdjustLookRotation();
    }

    public override void Tick()
    {
        myStateMachine.ForwardLookAround();

        Movement();
        Fall();
    }

    void Fall()
    {
        myStateMachine.GravityTick();

        // Transitions
        GameObject obj;
        myStateMachine.EnemyIsInRange(out obj);

        RaycastHit hit;
        if (myStateMachine.GetEdgeHit())
        {
            myStateMachine.ChangeState(PlayerStateMachine.eStates.LedgeClimb);
        }
        else if (Input.GetButtonDown("Attack") && myStateMachine.CanKick())
        {
            myStateMachine.ChangeState(PlayerStateMachine.eStates.AirKick);
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
