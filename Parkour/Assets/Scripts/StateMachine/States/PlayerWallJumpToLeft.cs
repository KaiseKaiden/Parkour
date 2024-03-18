using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallJumpToLeft : State
{
    const float myJumpForce = 6.5f;

    public override void OnEnter()
    {
        Vector3 vel = myStateMachine.GetCurrentVelocity() - myStateMachine.transform.right * myJumpForce;
        vel.y = myJumpForce;
        myStateMachine.SetVelocityXYZ(vel.x, vel.y, vel.z);

        myStateMachine.GetPlayerAnimator().SetTrigger("jump");
    }

    public override void OnExit()
    {

    }

    public override void Tick()
    {
        myStateMachine.LookAround();

        Fall();
    }

    void Fall()
    {
        myStateMachine.GravityTick();

        // Transitions
        GameObject obj;
        myStateMachine.EnemyIsInRange(out obj);

        RaycastHit hit;
        if (myStateMachine.GetEdgeHit())
        {
            myStateMachine.ChangeState(PlayerStateMachine.eStates.LedgeClimb);
        }
        else if (Input.GetButtonDown("Attack") && myStateMachine.CanKick())
        {
            myStateMachine.ChangeState(PlayerStateMachine.eStates.AirKick);
        }
        else if (myStateMachine.GetCharacterController().velocity.y < -0.0f)
        {
            myStateMachine.ChangeState(PlayerStateMachine.eStates.Falling);
        }
        else if (myStateMachine.GetCharacterController().velocity.y > 1.0f && myStateMachine.RaycastForward(out hit))
        {
            myStateMachine.ChangeState(PlayerStateMachine.eStates.WallRun);
        }
        else if (myStateMachine.RaycastForward(out hit))
        {
            myStateMachine.ChangeState(PlayerStateMachine.eStates.Falling);
        }
        else if (myStateMachine.WallRunnLeftTransition() && !Input.GetButton("Crouch"))
        {
            // Wallrun Left
            myStateMachine.ChangeState(PlayerStateMachine.eStates.WallRunH);
        }
        else if (myStateMachine.IsGrounded())
        {
            myStateMachine.ChangeState(PlayerStateMachine.eStates.Idle);
        }
        //else
        //{
        //    if (!myStateMachine.RaycastHead() && myStateMachine.RaycastHipp())
        //    {
        //        Vector3 p = myStateMachine.transform.position + Vector3.up * 1.9f + myStateMachine.transform.forward;

        //        if (Physics.Raycast(p, Vector3.down, out hit, 2.0f, myStateMachine.GetWallLayerMask()))
        //        {
        //            Message msg = new Message(eMessage.EdgeClimb, hit.point);
        //            PostMaster.Instance.SendMessage(msg);

        //            myStateMachine.ChangeState(PlayerStateMachine.eStates.EdgeClimbHead);
        //        }
        //    }
        //    else if (!myStateMachine.RaycastHipp() && myStateMachine.RaycastFeet())
        //    {
        //        Vector3 p = myStateMachine.transform.position + Vector3.up + myStateMachine.transform.forward;

        //        if (Physics.Raycast(p, Vector3.down, out hit, 2.0f, myStateMachine.GetWallLayerMask()))
        //        {
        //            Message msg = new Message(eMessage.EdgeClimb, hit.point);
        //            PostMaster.Instance.SendMessage(msg);

        //            myStateMachine.ChangeState(PlayerStateMachine.eStates.EdgeClimbHipp);
        //        }
        //    }
        //    else if (!myStateMachine.RaycastFeet())
        //    {
        //        Vector3 p = myStateMachine.transform.position + Vector3.up * 0.1f + myStateMachine.transform.forward;

        //        if (Physics.Raycast(p, Vector3.down, out hit, 2.0f, myStateMachine.GetWallLayerMask()))
        //        {
        //            Message msg = new Message(eMessage.EdgeClimb, hit.point);
        //            PostMaster.Instance.SendMessage(msg);

        //            myStateMachine.ChangeState(PlayerStateMachine.eStates.EdgeClimbFeet);
        //        }
        //    }
        //}
    }
}
