using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuSpinn : MonoBehaviour
{
    [SerializeField] float mySpinnSpeed;

    void Update()
    {
        transform.Rotate(Vector3.up * mySpinnSpeed * Time.deltaTime);
    }
}
