using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class VRHoverEffect : MonoBehaviour
{
    private Material material;
    private bool isHovered = false;

    public string sceneToLoad;

    public GameObject tipTutorial1;
    public GameObject tipTutorial2;
    public XROrigin xrOrigin;

    private XRSimpleInteractable interactable;

    void Start()
    {
        material = GetComponent<Renderer>().material;
        interactable = GetComponent<XRSimpleInteractable>();
        interactable.hoverEntered.AddListener(OnHoverEnter);
        interactable.hoverExited.AddListener(OnHoverExit);
    }

    void Update()
    {
       
        if (isHovered)
        {
            InputDevice rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            if (rightController.TryGetFeatureValue(CommonUsages.triggerButton, out bool isPressed) && isPressed)
            {
                LoadScene();
            }
        }
    }

    private void OnHoverEnter(HoverEnterEventArgs args)
    {
        material.SetFloat("_IsHovering", 1.0f);
        isHovered = true;

        if (xrOrigin.CameraYOffset == 20)
        {
            tipTutorial1.SetActive(true);
        }
        else if (xrOrigin.CameraYOffset == 2)
        {
            tipTutorial2.SetActive(true);
        }
    }

    private void OnHoverExit(HoverExitEventArgs args)
    {
        material.SetFloat("_IsHovering", 0.0f);
        isHovered = false;

        tipTutorial1.SetActive(false);
        tipTutorial2.SetActive(false);
    }

    void LoadScene()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToLoad);
        }
    }

}