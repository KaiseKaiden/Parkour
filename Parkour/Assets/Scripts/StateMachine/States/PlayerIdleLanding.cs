using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdleLanding : State
{
    public override void OnEnter()
    {
        myStateMachine.GetPlayerAnimator().SetTrigger("land");
        myStateMachine.SetGroundedYVelocity();

        myStateMachine.CreateLandParticle();

        RaycastHit hit;
        if (Physics.Raycast(myStateMachine.transform.position, Vector3.down, out hit, 1.0f, myStateMachine.GetWallLayerMask()))
        {
            myStateMachine.GetCharacterController().enabled = false;
            myStateMachine.transform.position = hit.point;
            myStateMachine.GetCharacterController().enabled = true;
        }

        myStateMachine.ChangeState(PlayerStateMachine.eStates.Idle);
        AudioManager.Instance.PlaySound(AudioManager.eSound.Land, myStateMachine.transform.position);
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
