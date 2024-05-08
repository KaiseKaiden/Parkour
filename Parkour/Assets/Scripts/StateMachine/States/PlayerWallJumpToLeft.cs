using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallJumpToLeft : State
{
    const float myJumpForce = 6.5f;

    public override void OnEnter()
    {
        Vector3 vel = myStateMachine.GetCurrentVelocity() + myStateMachine.transform.forward * myJumpForce;
        vel.y = myJumpForce * 0.5f;
        myStateMachine.SetVelocityXYZ(vel.x, vel.y, vel.z);

        myStateMachine.GetPlayerAnimator().SetTrigger("jump");
        AudioManager.Instance.PlaySound(AudioManager.eSound.Jump, myStateMachine.transform.position);
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
        else if (myStateMachine.WallRunnLeftTransition() && !Input.GetButton("Crouch"))
        {
            // Wallrun Left
            myStateMachine.ChangeState(PlayerStateMachine.eStates.WallRunH);
        }
        else if (myStateMachine.IsGrounded())
        {
            myStateMachine.ChangeState(PlayerStateMachine.eStates.Idle);
        }
    }
}
