using UnityEngine;

public class LampAvoidance : MonoBehaviour
{
    public Transform lampTransform;   // The actual lamp object
    public Vector3 defaultOffset;     // Where the lamp sits normally (e.g., 0.5, -0.3, 0.5)
    public float leanDistance = 0.7f; // How far the "laser" reaches
    public float smoothSpeed = 5f;    // Speed of the pull-back animation

    void Update()
    {
        RaycastHit hit;
        Vector3 targetPosition = defaultOffset;

        // Shoot a ray forward from the camera
        if (Physics.Raycast(transform.position, transform.forward, out hit, leanDistance))
        {
            // Calculate how much to pull back based on how close the wall is
            float pullBackAmount = leanDistance - hit.distance;
            targetPosition = defaultOffset - new Vector3(0, 0, pullBackAmount);
        }

        // Smoothly move the lamp to the new position
        lampTransform.localPosition = Vector3.Lerp(lampTransform.localPosition, targetPosition, Time.deltaTime * smoothSpeed);
    }
}
