using UnityEngine;

public class FaceUser : MonoBehaviour
{
    public Transform userCamera; // Assign the user's camera (usually the VR headset camera)

    void Update()
    {
        // Make the button face the user
        Vector3 direction = (userCamera.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }
}