using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(ModelLoader), typeof(XRSimpleInteractable))]
public class ModelUi : MonoBehaviour
{
    [SerializeField]
    private GameObject ui = null!;

    [SerializeField]
    private Renderer highlighter = null!;

    [SerializeField]
    private Color normalColor = Color.white;
    [SerializeField]
    private Color hoverColor = Color.magenta;
    [SerializeField]
    private Color highlightColor = Color.cyan;

    [SerializeField]
    private Button loadOptimizedButton = null!;
    [SerializeField]
    private Button loadUnoptimizedButton = null!;
    [SerializeField]
    private GameObject loadingText = null!;

    private void Start()
    {
        highlighter.material.color = normalColor;

        Vector3 highlighterPosition = highlighter.transform.position;
        ui.transform.position = new(highlighterPosition.x, ui.transform.position.y, highlighterPosition.z);

        ModelLoader modelLoader = GetComponent<ModelLoader>();
        loadOptimizedButton.onClick.AddListener(() => modelLoader.LoadOptimizedModel());
        loadUnoptimizedButton.onClick.AddListener(() => modelLoader.LoadUnoptimizedModel());
        
        modelLoader.LoadingStarted += () =>
        {
            loadingText.SetActive(true);
            loadOptimizedButton.gameObject.SetActive(false);
            loadUnoptimizedButton.gameObject.SetActive(false);
        };
        modelLoader.LoadingFinished += () =>
        {
            loadingText.SetActive(false);
            loadOptimizedButton.gameObject.SetActive(true);
            loadUnoptimizedButton.gameObject.SetActive(true);
        };

        XRSimpleInteractable simpleInteractable = GetComponent<XRSimpleInteractable>();
        simpleInteractable.focusEntered.AddListener((_) => Focus());
        simpleInteractable.focusExited.AddListener((_) => Unfocus());

        simpleInteractable.hoverEntered.AddListener((_) => Hover());
        simpleInteractable.hoverExited.AddListener((_) => UnHover());
    }

    private void Update()
    {
        if (!ui.activeInHierarchy) { return; }

        Vector3 cameraDirection = ui.transform.position - Camera.main.transform.position;
        cameraDirection.y = 0;

        float angle = Vector3.SignedAngle(
            ui.transform.forward,
            cameraDirection.normalized,
            Vector3.up
        );

        ui.transform.Rotate(Vector3.up, angle);
    }

    private void Hover() => highlighter.material.color = hoverColor;
    private void UnHover()
    {
        highlighter.material.color = ui.activeSelf ? highlightColor : normalColor;
    }

    private void Focus()
    {
        ui.SetActive(true);
        highlighter.GetComponent<Collider>().enabled = false;
        highlighter.material.color = highlightColor;
    }

    private void Unfocus()
    {
        ui.SetActive(false);
        highlighter.GetComponent<Collider>().enabled = true;
        highlighter.material.color = normalColor;
    }
}