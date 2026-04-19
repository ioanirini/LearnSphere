using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRSimpleInteractable))]
public class XRToggleUi : MonoBehaviour
{
    [SerializeField] private GameObject ui = null!;
    [SerializeField] private GameObject highlight = null!;

    private Transform xrCamera;
    private XRSimpleInteractable interactable;
    private static XRToggleUi currentlyOpen;

    private void Awake()
    {
        interactable = GetComponent<XRSimpleInteractable>();
        ResolveCamera();
    }

    private void OnEnable()
    {
        interactable.selectEntered.AddListener(OnSelected);
    }

    private void OnDisable()
    {
        interactable.selectEntered.RemoveListener(OnSelected);

        if (currentlyOpen == this)
            currentlyOpen = null;
    }

    private void Start()
    {
        HideUi();
        ResolveCamera();
    }

    private void Update()
    {
        if (xrCamera == null)
            ResolveCamera();

        if (ui != null && ui.activeSelf && xrCamera != null)
            FaceCamera();
    }

    private void ResolveCamera()
    {
        if (Camera.main != null)
            xrCamera = Camera.main.transform;
    }

    private void OnSelected(SelectEnterEventArgs args)
    {
        if (ui == null)
            return;

        if (currentlyOpen == this)
        {
            HideUi();
            currentlyOpen = null;
            return;
        }

        if (currentlyOpen != null)
            currentlyOpen.HideUi();

        ShowUi();
        currentlyOpen = this;
    }

    private void ShowUi()
    {
        ui.SetActive(true);

        if (highlight != null)
            highlight.SetActive(false);

        if (xrCamera != null)
            FaceCamera();
    }

    private void HideUi()
    {
        ui.SetActive(false);

        if (highlight != null)
            highlight.SetActive(true);
    }

    private void FaceCamera()
    {
        Vector3 direction = xrCamera.position - ui.transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        ui.transform.rotation = Quaternion.LookRotation(direction);

        // Flip 180 so the visible front of the UI faces the player
        ui.transform.Rotate(0f, 180f, 0f);
    }
}