using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdleState : State
{
    float myActiveTime = 0.0f;

    public override void OnEnter()
    {
        myActiveTime = 0.0f;
        myStateMachine.SetDesiredFOV(90.0f);

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
        myActiveTime += Time.deltaTime;
        myStateMachine.RemoveFlowPoint(Time.deltaTime * myActiveTime * 10.0f);

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
        if (myStateMachine.GroundIsSlippy())
        {
            myStateMachine.ChangeState(PlayerStateMachine.eStates.Slide);
        }
        else if (!myStateMachine.IsGrounded())
        {
            myStateMachine.ChangeState(PlayerStateMachine.eStates.CayoteFalling);
        }
        else if (input.magnitude > 0.2f)
        {
            myStateMachine.ChangeState(PlayerStateMachine.eStates.Running);
        }
        else if (Input.GetButtonDown("Jump"))
        {
            myStateMachine.ChangeState(PlayerStateMachine.eStates.Jump);
        }
    }
}
