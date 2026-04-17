using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtonHandler : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        // Check for JoystickButton6 (Oculus menu button)
        if (Input.GetKeyDown(KeyCode.JoystickButton6))
        {
            HandleMenuAction();
        }
    }

    void HandleMenuAction()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        Debug.Log($"Menu button pressed in scene: {currentScene}");

        if (currentScene == "AghiosVasiliosFinal" || currentScene == "BridgeOfArtaNewNew" || currentScene == "Panaghia")
        {
            // Load Sample Scene
            Debug.Log("Returning to Sample Scene...");
            SceneManager.LoadScene("MainScene");
        }
        else if (currentScene == "MainScene")
        {
            // Quit application
            Debug.Log("Quitting application...");
            QuitApplication();
        }
    }

    void QuitApplication()
    {
        Application.Quit();
        Debug.Log("Application Quit");
    }
}
