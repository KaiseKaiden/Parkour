using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] Animator myAnimator;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            myAnimator.enabled = !myAnimator.enabled;
        }
    }
}