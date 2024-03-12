using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdleLanding : State
{
    public override void OnEnter()
    {
        myStateMachine.SetGroundedYVelocity();

        myStateMachine.ChangeState(PlayerStateMachine.eStates.Idle);

        myStateMachine.CreateLandParticle();

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
        myStateMachine.AdjustLookRotation();
    }

    public override void Tick()
    {
        myStateMachine.ForwardLookAround();
    }
}
