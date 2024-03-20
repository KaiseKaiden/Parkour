using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVaultState : State
{
    Vector3 myStartPos;
    Vector3 myDesiredPos;
    Vector3 myDiffernce;
    Vector3 myDeciredAngle;

    float myTime;

    float mySpeed;
    float myMultiplier;
    float myNewRemainingDistanceToGround;
    const float myMinSpeed = 1.0f;
    const float myMaxSpeed = 7.5f;

    const float myAnimationHeight = 1.0f;

    public override void OnEnter()
    {
        myStateMachine.SetGroundedYVelocity();

        myStateMachine.GetCharacterController().enabled = false;
        myStartPos = myStateMachine.transform.position;
        myDesiredPos = myStartPos + (myStateMachine.GetObstacleVaultPosition() - myStartPos) * 2.0f;

        myDiffernce = myDesiredPos - myStartPos;
        myTime = 0.0f;

        myDeciredAngle = myDiffernce;
        myDeciredAngle.y = 0.0f;
        myDeciredAngle.Normalize();

        mySpeed = Mathf.Clamp(myStateMachine.GetCurrentVelocityXZ().magnitude, myMinSpeed, myMaxSpeed);
        myMultiplier = (mySpeed / myDiffernce.magnitude);

        myNewRemainingDistanceToGround = 0.0f;
        RaycastHit hit;
        if (Physics.SphereCast(myStateMachine.transform.position + Vector3.up * 1.5f, myStateMachine.GetCharacterController().radius, Vector3.down, out hit, 3.0f, myStateMachine.GetGroundLayerMask()))
        {
            // Ground The Player
            Vector3 start = myStateMachine.transform.position + Vector3.up * 1.5f;
            Vector3 end = hit.point;
            Vector3 newPos = start + Vector3.down * (myStateMachine.SphereCastStartToMiddleDistance(start, end) + myStateMachine.GetCharacterController().radius);

            myNewRemainingDistanceToGround = newPos.y - myStartPos.y;
        }

        myStateMachine.GetPlayerAnimator().SetTrigger("vault");
    }

    public override void OnExit()
    {
        myStateMachine.GetCharacterController().enabled = true;
        myStateMachine.AdjustLookRotation();
    }

    public override void Tick()
    {
        myStateMachine.ForwardLookAround();

        Quaternion rotation = Quaternion.LookRotation(new Vector3(myDeciredAngle.x, 0.0f, myDeciredAngle.z));
        myStateMachine.transform.rotation = Quaternion.Lerp(myStateMachine.transform.rotation, rotation, 5.0f * Time.deltaTime);

        myTime += Time.deltaTime * myMultiplier;
        myTime = Mathf.Clamp01(myTime);

        Vector3 newPosition = myStartPos + myDiffernce * myTime;
        newPosition.y = myStartPos.y + Mathf.Sin(myTime * Mathf.PI) * ((myStateMachine.GetObstacleVaultPosition().y - myStartPos.y) - myAnimationHeight);
        if (myTime > 0.5f) newPosition.y += (myTime - 0.5f) * 2.0f * myNewRemainingDistanceToGround; // Apply Distance To Ground Behind Obstacle
        myStateMachine.transform.position = newPosition;

        if (myTime == 1.0f)
        {
            myStateMachine.ChangeState(PlayerStateMachine.eStates.Running);

            Vector3 vel = myStateMachine.transform.forward * mySpeed;
            myStateMachine.SetVelocityXZ(vel.x, vel.z);
        }
    }
}
