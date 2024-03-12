using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRollLanding : State
{
    const float mySpeed = 2.5f;
    float myActiveTime;

    public override void OnEnter()
    {
        RaycastHit hit;
        if (Physics.Raycast(myStateMachine.transform.position + Vector3.up, Vector3.down, out hit, 2.0f, myStateMachine.GetWallLayerMask()))
        {
            if (Vector3.Dot(Vector3.up, hit.normal) < 0.95f)
            {
                myStateMachine.ChangeState(PlayerStateMachine.eStates.Slide);
                return;
            }
        }

        myStateMachine.SetGroundedYVelocity();
        myActiveTime = 0.0f;

        Vector3 forward = myStateMachine.transform.forward * mySpeed;
        myStateMachine.SetVelocityXYZ(forward.x, -0.5f, forward.z);

        myStateMachine.GetAnimator().SetTrigger("Roll");
        myStateMachine.SetDesiredCameraTilt(-35.0f);

        myStateMachine.SetHeight(0.5f);

        myStateMachine.CreateLandParticle();
    }

    public override void OnExit()
    {
        myStateMachine.SetDesiredCameraTilt(0.0f);
    }

    public override void Tick()
    {
        myStateMachine.GravityTick();

        Land();
    }

    void Land()
    {
        myActiveTime += Time.deltaTime;

        myStateMachine.SetCameraHeight(0.5f + Mathf.Cos(myActiveTime * Mathf.PI) * 1.5f);
        myStateMachine.SetDesiredCameraTilt((1.0f - myActiveTime) * -35.0f);

        if (myActiveTime > 1.0f)
        {
            if (Physics.Raycast(myStateMachine.transform.position + Vector3.one * 0.3f, Vector3.up, 1.7f, myStateMachine.GetWallLayerMask()))
            {
                myStateMachine.ChangeState(PlayerStateMachine.eStates.Crouch);
            }
            else if (Input.GetButton("Crouch"))
            {
                myStateMachine.ChangeState(PlayerStateMachine.eStates.Crouch);
            }
            else
            {
                myStateMachine.ChangeState(PlayerStateMachine.eStates.Idle);
                myStateMachine.SetDesiredCameraHeight(2.0f);
                myStateMachine.SetHeight(2.0f);
            }
        }
    }
}
