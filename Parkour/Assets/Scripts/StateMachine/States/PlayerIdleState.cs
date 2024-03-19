using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdleState : State
{
    public override void OnEnter()
    {
        myStateMachine.SetDesiredFOV(90.0f);
        myStateMachine.SetDesiredCameraTilt(0.0f);

        RaycastHit hit;
        if (Physics.Raycast(myStateMachine.transform.position, Vector3.down, out hit, 1.0f, myStateMachine.GetWallLayerMask()))
        {
            myStateMachine.GetCharacterController().enabled = false;
            myStateMachine.transform.position = hit.point;
            myStateMachine.GetCharacterController().enabled = true;
        }
    }

    public override void OnExit()
    {

    }

    public override void Tick()
    {
        myStateMachine.LookAround();
        if (!myStateMachine.IsGrounded())
        {
            myStateMachine.GravityTick();
        }

        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        Vector3 vel = Vector3.Lerp(myStateMachine.GetCurrentVelocity(), Vector3.zero, Time.deltaTime * 5.0f);
        myStateMachine.SetVelocityXZ(vel.x, vel.z);

        myStateMachine.GetPlayerAnimator().SetFloat("speed", 0.0f);

        // Transition To State
        GameObject obj;
        myStateMachine.EnemyIsInRange(out obj);

        if (myStateMachine.GroundIsSlippy())
        {
            myStateMachine.ChangeState(PlayerStateMachine.eStates.Slide);
        }
        else if (!myStateMachine.IsGrounded())
        {
            myStateMachine.ChangeState(PlayerStateMachine.eStates.CayoteFalling);
        }
        else if (Input.GetButtonDown("Attack") && myStateMachine.CanKick())
        {
            myStateMachine.ChangeState(PlayerStateMachine.eStates.AirKick);
        }
        else if (input.magnitude > 0.2f)
        {
            myStateMachine.ChangeState(PlayerStateMachine.eStates.Running);
        }
        else if (Input.GetButtonDown("Jump"))
        {
            myStateMachine.ChangeState(PlayerStateMachine.eStates.Jump);
        }
        else if (Input.GetButtonDown("Crouch"))
        {
            myStateMachine.ChangeState(PlayerStateMachine.eStates.Crouch);
        }
    }
}
