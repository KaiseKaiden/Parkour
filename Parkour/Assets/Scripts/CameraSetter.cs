using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSetter : MonoBehaviour
{
    [SerializeField] Camera myTopCamera;

    private void Update()
    {
        myTopCamera.fieldOfView = Camera.main.fieldOfView;
    }
}
