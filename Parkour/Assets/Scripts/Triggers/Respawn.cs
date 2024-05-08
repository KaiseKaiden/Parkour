using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Respawn : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            Message position = new Message(eMessage.Respawn, Vector3.zero);
            PostMaster.Instance.SendMessage(position);
        }
    }
}
