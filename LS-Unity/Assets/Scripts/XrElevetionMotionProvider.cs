using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;

public class XrElevationMotionProvider : MonoBehaviour
{
    [SerializeField]
    XRInputValueReader<Vector2> input = new("Left elevation control");

    [Header("Settings")]
    [SerializeField]
    private float speed = 1f;

    [SerializeField]
    private float minY = 0f;

    [SerializeField]
    private float maxY = 3f;

    protected void OnEnable()
    {
        input.EnableDirectActionIfModeUsed();
    }

    protected void OnDisable()
    {
        input.DisableDirectActionIfModeUsed();
    }

    protected void Update()
    {
        float deltaY = input.ReadValue().y * speed * Time.deltaTime;

        Vector3 pos = transform.position;
        pos.y = Mathf.Clamp(pos.y + deltaY, minY, maxY);
        transform.position = pos;
    }
}