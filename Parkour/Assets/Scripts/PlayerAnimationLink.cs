using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationLink : MonoBehaviour
{
    [SerializeField] PlayerStateMachine myStateMachine;

    public void AnimImpact()
    {
        myStateMachine.AnimImpact();
    }

    public void AnimDone()
    {
        myStateMachine.AnimDone();
    }
}
