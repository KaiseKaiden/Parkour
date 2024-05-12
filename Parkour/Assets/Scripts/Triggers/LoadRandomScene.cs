using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadRandomScene : MonoBehaviour
{
    public string[] sceneNames; // Array to hold the names of the scenes you want to load
    private bool loaded = false; // Flag to prevent multiple scene loads

    // Reference to the player's Rigidbody
    private Rigidbody playerRigidbody;

    // Reference to the player's state machine
    private PlayerStateMachine playerStateMachine;

    // Store the player's velocity
    private Vector3 myVelocity;

    // Store the player's current state
    private PlayerStateMachine.eStates myCurrentState;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !loaded) // Check if the colliding object is the player and scene is not already loaded
        {
            // Get the player's Rigidbody
            playerRigidbody = other.GetComponent<Rigidbody>();
            if (playerRigidbody == null)
            {
                Debug.LogError("Player Rigidbody not found!");
                return;
            }

            // Get the player's state machine
            playerStateMachine = other.GetComponent<PlayerStateMachine>();
            if (playerStateMachine == null)
            {
                Debug.LogError("Player State Machine not found!");
                return;
            }

            // Store the player's current velocity

            myVelocity = playerStateMachine.GetCurrentVelocity();

            // Store the player's current state
            myCurrentState = playerStateMachine.GetCurrentState();

            LoadRandom(); // Call the method to load random scene
            loaded = true; // Set loaded flag to true to prevent multiple loads
        }
    }

    private void LoadRandom()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        int randomIndex;
        do
        {
            randomIndex = Random.Range(0, sceneNames.Length); // Generate a random index within the range of sceneNames array
        } while (sceneNames[randomIndex] == currentScene.name); // Keep generating new random index until it's different from current scene

        string sceneToLoad = sceneNames[randomIndex]; // Get the scene name at the random index
       
        SceneManager.LoadScene(sceneToLoad); // Load the random scene
        RestorePlayerState();
    }

    public void RestorePlayerState()
    {
        if (playerRigidbody != null && playerStateMachine != null)
        {
            
            // Set the player's velocity to the stored velocity
            playerRigidbody.velocity = myVelocity;

            // Change the player's state back to the stored state
            playerStateMachine.ChangeState(myCurrentState);
        }
        else
        {
            Debug.LogError("Player Rigidbody or State Machine is null!");
        }
    }
}
