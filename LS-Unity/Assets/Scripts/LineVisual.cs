using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;

public class LineVisual : MonoBehaviour
{
    private XRInteractorLineVisual lineVisual;

    private void Awake()
    {
        lineVisual = GetComponent<XRInteractorLineVisual>();
    }

    private void OnDestroy()
    {
        if (lineVisual != null)
        {
            lineVisual.enabled = false; // prevents OnBeforeRender calls after destruction
        }
    }
}

