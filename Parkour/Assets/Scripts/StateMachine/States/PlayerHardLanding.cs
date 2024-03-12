using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHardLanding : State
{
    float myActiveTime;

    public override void OnEnter()
    {
        myStateMachine.SetVelocityXYZ(0.0f, 0.0f, 0.0f);
        myActiveTime = 0.0f;

        myStateMachine.SetScreenShakeIntensity(0.2f);

        myStateMachine.CreateLandParticle();
    }

    public override void OnExit()
    {
        myStateMachine.AdjustLookRotation();
    }

    public override void Tick()
    {
        myStateMachine.ForwardLookAround();

        Land();
    }

    void Land()
    {
        myActiveTime += Time.deltaTime;

        myStateMachine.SetCameraHeight(1.3f + myStateMachine.GetAnimationCurves().myHardLandingCurve.Evaluate(myActiveTime) * 0.7f);

        if (myActiveTime > 1.0f)
        {
            myStateMachine.ChangeState(PlayerStateMachine.eStates.Idle);
        }
    }
}
