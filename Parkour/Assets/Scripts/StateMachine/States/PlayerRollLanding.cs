using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRollLanding : State
{
    const float mySpeed = 2.5f;
    float myActiveTime;

    public override void OnEnter()
    {
        if (myStateMachine.GroundIsSlippy())
        {
            myStateMachine.ChangeState(PlayerStateMachine.eStates.Slide);
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(myStateMachine.transform.position + Vector3.up, Vector3.down, out hit, 2.0f, myStateMachine.GetWallLayerMask()))
        {
            if (Vector3.Dot(Vector3.up, hit.normal) < 0.95f)
            {
                if (Vector3.Dot(myStateMachine.transform.forward, hit.normal) >= 0.0f)
                {
                    myStateMachine.ChangeState(PlayerStateMachine.eStates.Slide);
                    return;
                }
            }
        }

        myStateMachine.SetGroundedYVelocity();
        myActiveTime = 0.0f;

        Vector3 forward = myStateMachine.transform.forward * mySpeed;
        myStateMachine.SetVelocityXYZ(forward.x, -0.5f, forward.z);

        myStateMachine.GetPlayerAnimator().SetTrigger("rolling");

        myStateMachine.CreateLandParticle();
    }

    public override void OnExit()
    {
        
    }

    public override void Tick()
    {
        if (!myStateMachine.IsGrounded())
        {
            myStateMachine.GravityTick();
        }

        Land();
    }

    void Land()
    {
        myActiveTime += Time.deltaTime;

        if (myActiveTime > 1.0f)
        {
            myStateMachine.ChangeState(PlayerStateMachine.eStates.Idle);
        }
    }
}
