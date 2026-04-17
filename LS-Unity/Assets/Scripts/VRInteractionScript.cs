using UnityEngine;
using UnityEngine.XR;
using UnityEngine.SceneManagement;

public class VRInteractionScript : MonoBehaviour
{
    public Transform raycastOrigin;
    public string targetTag = "Human";

    private bool isHovering = false;
    private GameObject currentTarget = null;

    private PlayAudioClipsPOIs playAudioScript;


    void Update()
    {
        InputDevice rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

        Ray ray = new Ray(raycastOrigin.position, raycastOrigin.forward);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.transform.CompareTag(targetTag))
            {
                if (!isHovering)
                {
                    isHovering = true;
                    currentTarget = hit.transform.gameObject;

                    // Get a reference to the new audio script
                    playAudioScript = currentTarget.GetComponent<PlayAudioClipsPOIs>();
                }

                if (rightController.TryGetFeatureValue(CommonUsages.triggerButton, out bool isPressed) && isPressed)
                {
                    CallInteractionScripts();
                }
            }
            else
            {
                ResetHover();
            }
        }
        else
        {
            ResetHover();
        }
    }

    private void ResetHover()
    {
        if (isHovering)
        {
            isHovering = false;
            currentTarget = null;
            playAudioScript = null; // Clear the reference when not hovering
        }
    }

    private void CallInteractionScripts()
    {
        // Call the new audio script's method
        if (playAudioScript != null)
        {
            playAudioScript.ToggleAudio();
        }
    }
}