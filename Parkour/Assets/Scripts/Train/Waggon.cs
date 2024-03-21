using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waggon : MonoBehaviour
{
    Train myTrain;

    public void SetTrainObject(Train aTrain)
    {
        myTrain = aTrain;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == "Player")
        {
            myTrain.HitPlayer();
        }
        else if (other.transform.tag == "EnemyBody")
        {
            Enemy enemy = other.transform.root.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.KickedAt(other.transform.position + Vector3.up * 1.5f, myTrain.transform.forward * myTrain.GetSpeed());
            }
        }
    }
}
