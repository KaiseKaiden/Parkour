using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadRandomScene : MonoBehaviour
{
    public string[] sceneNames; // Array to hold the names of the scenes you want to load
    private bool loaded = false; // Flag to prevent multiple scene loads

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !loaded) // Check if the colliding object is the player and scene is not already loaded
        {
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
    }
}
