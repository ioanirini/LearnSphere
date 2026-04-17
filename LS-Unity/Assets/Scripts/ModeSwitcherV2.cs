using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;

public class ModeSwitcherV2 : MonoBehaviour
{
    public InputActionReference modeSwitcher;
    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor interactionRay; // Assign the interaction ray in the Inspector
    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor teleportRay;    // Assign the teleport ray in the Inspector
    private void OnEnable()
    {
        if (interactionRay == null) //checking for null interactors to prevent exception null
        {
            interactionRay = GameObject.FindWithTag("Object")?.GetComponent<XRRayInteractor>();
        }

        if (teleportRay == null)
        {
            teleportRay = GameObject.FindWithTag("Teleport")?.GetComponent<XRRayInteractor>();
        }

        modeSwitcher.action.started += ButtonWasPressed; //subscribing to the callback list
        modeSwitcher.action.Enable();
    }

    private void OnDisable()
    {

        modeSwitcher.action.started -= ButtonWasPressed;//unsubscribing to the callback list
        modeSwitcher.action.Disable();//disabling 
        
    }
   // private void Start()
    //{
       // modeSwitcher.action.Enable();
        //modeSwitcher.action.performed += ButtonWasPressed;
    //}
    void ButtonWasPressed(InputAction.CallbackContext context)
    {
        bool isInteractionAcive = interactionRay.enabled;
        interactionRay.enabled= !isInteractionAcive;
        teleportRay.enabled = isInteractionAcive;
    }
    
}
