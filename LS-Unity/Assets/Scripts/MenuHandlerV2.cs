using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MenuHandlerV2 : MonoBehaviour
{
    public InputActionReference menuHandler;

    private void Awake()
    {
        menuHandler.action.started += HandleMenuAction;
        menuHandler.action.Enable();    
    }

    private void OnDisable()
    {
        menuHandler.action.performed -= HandleMenuAction;
        menuHandler.action.Disable();
    }
    void HandleMenuAction(InputAction.CallbackContext context)
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
