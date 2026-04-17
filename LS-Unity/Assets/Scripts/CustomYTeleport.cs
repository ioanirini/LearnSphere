using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.XR.CoreUtils;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

public class CustomYTeleport : MonoBehaviour
{
    public XROrigin xrOrigin; // Reference to your XR Origin
    public float customYOffset = 0f; // Custom Y offset for this teleporter

    

    public void ApplyTeleportOffset()
    {
        if (xrOrigin != null)
        {

            // Adjust the Camera Y Offset for this teleportation
            xrOrigin.CameraYOffset = customYOffset;
            
           

        }
    }

}