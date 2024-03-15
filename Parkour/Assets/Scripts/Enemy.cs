using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] Animator myAnimator;
    Rigidbody[] myRigidbodies;

    private void Start()
    {
        myRigidbodies = GetComponentsInChildren<Rigidbody>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            myAnimator.enabled = true;
        }
    }

    public void KickedAt(Vector3 aPoint, Vector3 aForce)
    {
        myAnimator.enabled = false;

        foreach(Rigidbody r in myRigidbodies)
        {
            r.AddForceAtPosition(aForce, aPoint);
        }
    }
}