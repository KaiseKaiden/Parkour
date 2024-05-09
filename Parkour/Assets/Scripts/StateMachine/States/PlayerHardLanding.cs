using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHardLanding : State
{
    float myActiveTime = 0.0f;

    public override void OnEnter()
    {
        myStateMachine.SetVelocityXYZ(0.0f, 0.0f, 0.0f);

        myStateMachine.GetPlayerAnimator().SetTrigger("hardLand");

        myStateMachine.RemoveFlowPoint(20.0f);
        myActiveTime = 0.0f;
    }

    public override void OnExit()
    {
        myStateMachine.AdjustLookRotation();
    }

    public override void Tick()
    {
        myActiveTime += Time.deltaTime;
        myStateMachine.RemoveFlowPoint(Time.deltaTime * myActiveTime * 10.0f);

        myStateMachine.AdjustCameraRotation();
    }

    public override bool AnimImpact()
    {
        myStateMachine.SetScreenShakeIntensity(0.05f);
        myStateMachine.CreateLandParticle();
        AudioManager.Instance.PlaySound(AudioManager.eSound.HardLand, myStateMachine.transform.position);

        return true;
    }

    public override void AnimDone()
    {
        myStateMachine.ChangeState(PlayerStateMachine.eStates.Idle);
    }
}
