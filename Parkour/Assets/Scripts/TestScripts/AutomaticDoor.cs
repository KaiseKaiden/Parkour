using UnityEngine;

public class AutomaticDoor : MonoBehaviour
{
    public Animator doorAnimator; // Animator for the door
    public string openTriggerName = "Open"; // Name of the open trigger
    public string closeTriggerName = "Close"; // Name of the close trigger

    private bool isOpen = false; // Flag to check if the door is open

    void Start()
    {
        if (doorAnimator == null)
        {
            doorAnimator = GetComponent<Animator>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isOpen)
        {
            doorAnimator.SetTrigger(openTriggerName);
            isOpen = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && isOpen)
        {
            doorAnimator.SetTrigger(closeTriggerName);
            isOpen = false;
        }
    }
}
