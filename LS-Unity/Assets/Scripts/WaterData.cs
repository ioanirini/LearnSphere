using Unity.XR.CoreUtils;
using UnityEngine;

public class WaterData : MonoBehaviour
{
    public XROrigin origin;
    public GameObject gameobject;

    public void WaterDataThing()
    {
        if (origin.CameraYOffset==2)
        {
            gameobject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
