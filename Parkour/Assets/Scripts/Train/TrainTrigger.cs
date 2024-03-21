using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainTrigger : MonoBehaviour
{
    [SerializeField] Train myTrain;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            myTrain.EndTutorial();
        }
    }
}
