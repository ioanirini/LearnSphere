using UnityEngine;


public class ModeSwitcher : MonoBehaviour
{
    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor interactionRay; // Assign the interaction ray in the Inspector
    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor teleportRay;    // Assign the teleport ray in the Inspector

    void Update()
        {
    // Check if the A button is pressed (Joystick Button 0 for Oculus controllers)
    if (Input.GetKeyDown(KeyCode.JoystickButton0))
       {
            // Toggle modes
            //bool isInteractionActive = interactionRay.enabled;
            //interactionRay.enabled = !isInteractionActive;
            //teleportRay.enabled = isInteractionActive;
            Debug.Log("A is pressed");
      }
    }


   
}
