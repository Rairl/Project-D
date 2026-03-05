using UnityEngine;

public class LampFlicker : MonoBehaviour
{
    public Light lampLight;
    public float minIntensity = 0.7f;
    public float maxIntensity = 1.1f;
    public float flickerSpeed = 0.1f;

    void Update()
    {
        // Randomly oscillates the intensity to mimic a flame
        lampLight.intensity = Mathf.Lerp(lampLight.intensity, Random.Range(minIntensity, maxIntensity), flickerSpeed);
    }
}
