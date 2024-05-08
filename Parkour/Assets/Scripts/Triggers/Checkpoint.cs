using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] LayerMask myLayerMask;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, myLayerMask))
            {
                Message position = new Message(eMessage.CheckpointPosition, hit.point);
                PostMaster.Instance.SendMessage(position);

                Message orientation = new Message(eMessage.CheckpointOrientation, transform.eulerAngles);
                PostMaster.Instance.SendMessage(orientation);
            }
        }
    }
}
