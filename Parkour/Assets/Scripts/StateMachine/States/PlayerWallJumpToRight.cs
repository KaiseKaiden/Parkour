using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallJumpToRight : State
{
    const float myJumpForce = 6.5f;

    public override void OnEnter()
    {
        Vector3 vel = myStateMachine.GetCurrentVelocity() + myStateMachine.transform.right * myJumpForce;
        vel.y = myJumpForce;
        myStateMachine.SetVelocityXYZ(vel.x, vel.y, vel.z);

        myStateMachine.GetPlayerAnimator().SetTrigger("jump");
    }

    public override void OnExit()
    {

    }

    public override void Tick()
    {
        myStateMachine.LookAround();

        Fall();
    }

    void Fall()
    {
        myStateMachine.GravityTick();

        // Transitions
        RaycastHit hit;
        if (myStateMachine.GetEdgeHit())
        {
            myStateMachine.ChangeState(PlayerStateMachine.eStates.LedgeClimb);
        }
        else if (myStateMachine.GetCharacterController().velocity.y < -0.0f)
        {
            myStateMachine.ChangeState(PlayerStateMachine.eStates.Falling);
        }
        else if (myStateMachine.GetCharacterController().velocity.y > 1.0f && myStateMachine.RaycastForward(out hit))
        {
            myStateMachine.ChangeState(PlayerStateMachine.eStates.WallRun);
        }
        else if (myStateMachine.RaycastForward(out hit))
        {
            myStateMachine.ChangeState(PlayerStateMachine.eStates.Falling);
        }
        else if (myStateMachine.WallRunnRightTransition() && !Input.GetButton("Crouch"))
        {
            // Wallrun Right
            myStateMachine.ChangeState(PlayerStateMachine.eStates.WallRunH);
        }
        else if (myStateMachine.IsGrounded())
        {
            myStateMachine.ChangeState(PlayerStateMachine.eStates.Idle);
        }
    }
}
