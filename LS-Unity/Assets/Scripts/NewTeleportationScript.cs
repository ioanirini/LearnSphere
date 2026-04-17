using System.Collections;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

public class NewTeleportationScript : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputAsset;
    [SerializeField] private TeleportationProvider teleportationProvider;
    public XRRayInteractor teleportInteractor;
    public XRRayInteractor objectInteractor;
    private bool teleportIsActivated;

    private InputAction thumbstick;
    private InputAction teleportActivate;
    private InputAction teleportCancel;
    

    void Start()
    {
        objectInteractor.enabled = true;
        teleportInteractor.enabled = false;

        teleportActivate = inputAsset.FindActionMap("XRI Right Locomotion").FindAction("Teleport Mode"); // searches through the input map in the "XRI Right Locomotion" for the "Teleport Mode" to get the button we want to use to activate teleportation
        teleportActivate.Enable();
        teleportActivate.performed += OnTeleportActivate;

        teleportCancel = inputAsset.FindActionMap("XRI Right Locomotion").FindAction("Teleport Mode Cancel");
        teleportCancel.Enable();
        teleportCancel.performed += OnTeleportCancel;

        thumbstick = inputAsset.FindActionMap("XRI Right Locomotion").FindAction("Move");
        thumbstick.Enable();

    }

    private void OnDestroy()
    {
        if(teleportActivate != null)
        {
            teleportActivate.performed -= OnTeleportActivate;
        }
        
        if (teleportCancel != null)
        {
            teleportCancel.performed -= OnTeleportCancel;
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!teleportIsActivated)//checking if teleporation is currently activated or not
        {
            return;
        }
        if (!teleportInteractor.enabled)//checking if the ray is active or ot

        {
            return;
        }

       Vector2 moveInput = thumbstick.ReadValue<Vector2>(); //checking if the thumbstick has moved
        if (moveInput.magnitude > 0.3f)
            return;

        if (!teleportInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit)) //is the ray hitting a valid target
        {
            objectInteractor.enabled = true;
            teleportInteractor.enabled = false;
            teleportIsActivated = false;
            return;
        }

        if(!hit.collider.CompareTag("Teleportation")) //'filtering' valid targets and only make teleportation possible on targets with the Teleportation tag on them
        {
            return;
        }

       CustomYTeleport customYTeleport= hit.collider.GetComponent<CustomYTeleport>(); //checking if an object has the component with the custom teleportation script 
        if (customYTeleport != null)
        {
            customYTeleport.ApplyTeleportOffset();
        }

        TeleportRequest request = new TeleportRequest()
         {
           destinationPosition = hit.point
         };

        teleportationProvider.QueueTeleportRequest(request);

        StartCoroutine(DisableTeleportationInteractor());
        teleportIsActivated = false;

    }


    private void OnTeleportActivate(InputAction.CallbackContext context)
    {
        if (!teleportIsActivated)
        {;
            StartCoroutine(EnableTeleportationInteractor());
            teleportIsActivated = true;
        }
    }

    private void OnTeleportCancel(InputAction.CallbackContext context)
    {
        if(teleportIsActivated && teleportInteractor.enabled==true)
        {
            teleportInteractor.enabled = false;
            teleportIsActivated = false;
        }

    }  


    IEnumerator EnableTeleportationInteractor()
    {
        yield return null;
        objectInteractor.enabled = false;
        yield return null;
        teleportInteractor.enabled = true;
        
    }

    IEnumerator DisableTeleportationInteractor()
    {
        yield return null;
        objectInteractor.enabled = true;
        yield return null;
        teleportInteractor.enabled = false;

    }
}
