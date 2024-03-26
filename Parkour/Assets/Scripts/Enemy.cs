using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Observer
{
    [SerializeField] Animator myAnimator;
    Rigidbody[] myRigidbodies;

    float myDeadTimer;
    const float myRespawnCooldown = 3.0f;

    private void Start()
    {
        myRigidbodies = GetComponentsInChildren<Rigidbody>();

        PostMaster.Instance.Subscribe(eMessage.Respawn, this);
    }

    private void Update()
    {
        myDeadTimer += Time.deltaTime;

        if (myDeadTimer > myRespawnCooldown)
        {
            myAnimator.enabled = true;
        }
    }

    override public void Recive(Message aMsg)
    {
        if (aMsg.GetMsg() == eMessage.Respawn)
        {
            myAnimator.enabled = true;
        }
    }

    public void KickedAt(Vector3 aPoint, Vector3 aForce)
    {
        myAnimator.enabled = false;
        myDeadTimer = 0.0f;

        foreach (Rigidbody r in myRigidbodies)
        {
            r.AddForceAtPosition(aForce, aPoint);
        }
    }

    public bool IsAlive()
    {
        return myAnimator.enabled;
    }
}