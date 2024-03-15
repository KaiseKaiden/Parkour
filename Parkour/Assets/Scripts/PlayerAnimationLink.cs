using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationLink : MonoBehaviour
{
    [SerializeField] PlayerStateMachine myStateMachine;
    [SerializeField] BoxCollider myKickAttackBox;

    public void AirKick()
    {
        myStateMachine.Attacked();
    }

    public void AttackDone()
    {
        myStateMachine.AttackDone();
    }
}
