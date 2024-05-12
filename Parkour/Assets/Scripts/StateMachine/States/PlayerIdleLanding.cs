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

        Vector3 start = myStateMachine.transform.position + Vector3.up * 1.5f;

        RaycastHit hit;
        if (Physics.Raycast(start, Vector3.down, out hit, 2.5f, myStateMachine.GetWallLayerMask()))
        {
            
            Vector3 end = hit.point;
            Vector3 newPos = start + Vector3.down * (myStateMachine.SphereCastStartToMiddleDistance(start, end) + myStateMachine.GetCharacterController().radius);

            myStateMachine.GetCharacterController().enabled = false;
            myStateMachine.transform.position = newPos;
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
