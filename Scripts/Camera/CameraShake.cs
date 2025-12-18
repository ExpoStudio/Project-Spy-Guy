using UnityEngine;

public enum CameraShakeType
{
    Scaled,
    Unscaled
}

public class CameraShake : MonoBehaviour
{
    // Singleton instance
    private static CameraShake instance;

    // Shake parameters
    public float shakeDuration = 0.5f; // How long the shake lasts
    public float shakeMagnitude = 0.1f; // How intense the shake is
    public float dampingSpeed = 1.0f; // How quickly the shake settles
    public float shakeFrequency = 10f; // How many times per second the shake oscillates

    private Vector3 initialPosition; // The camera's original position
    private float shakeTimeRemaining = 0f; // Time remaining for the shake

    public CameraShakeType ShakeType { get; set; } = CameraShakeType.Scaled;

    private void Awake()
    {
        // Ensure only one instance of CameraShake exists
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple CameraShake instances detected. Destroying duplicate.");
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        initialPosition = transform.localPosition; // Store the camera's initial position
        if (shakeTimeRemaining > 0)
        {
            switch (ShakeType)
            {
                case CameraShakeType.Scaled:
                    ScaledShake();
                    break;
                case CameraShakeType.Unscaled:
                    UnscaledShake();
                    break;
            }
        }
    }

    private void UnscaledShake()
    {
        if (shakeTimeRemaining > 0)
        {
            // Generate Perlin noise values for smooth shaking
            float x = (Mathf.PerlinNoise(Time.unscaledTime * shakeFrequency, 0) * 2) - 1;
            float y = (Mathf.PerlinNoise(Time.unscaledTime * shakeFrequency, 10) * 2) - 1;

            // Apply the shake to the camera's position
            transform.localPosition = initialPosition + (new Vector3(x, y, 0) * shakeMagnitude);

            // Reduce the remaining shake time using unscaledDeltaTime
            shakeTimeRemaining -= Time.unscaledDeltaTime * dampingSpeed;
        }
        else
        {
            // Smoothly return to the original position
            transform.localPosition = Vector3.Lerp(transform.localPosition, initialPosition, Time.unscaledDeltaTime * dampingSpeed);
            if (Mathf.Abs(transform.localPosition.x - initialPosition.x) < 0.01f && Mathf.Abs(transform.localPosition.y - initialPosition.y) < 0.01f)
            {
                transform.localPosition = initialPosition; // Snap to original position to avoid floating point errors
                shakeTimeRemaining = 0; // Reset shake time remaining
            }
        }
    }

    private void ScaledShake()
    {
        if (shakeTimeRemaining > 0)
        {
            // Generate Perlin noise values for smooth shaking
            float x = (Mathf.PerlinNoise(Time.time * shakeFrequency, 0) * 2) - 1;
            float y = (Mathf.PerlinNoise(Time.time * shakeFrequency, 10) * 2) - 1;

            // Apply the shake to the camera's position
            transform.localPosition = initialPosition + (new Vector3(x, y, 0) * shakeMagnitude);

            // Reduce the remaining shake time
            shakeTimeRemaining -= Time.deltaTime * dampingSpeed;
        }
        else
        {
            // Smoothly return to the original position
            transform.localPosition = Vector3.Lerp(transform.localPosition, initialPosition, Time.deltaTime * dampingSpeed);
            if (Mathf.Abs(transform.localPosition.x - initialPosition.x) < 0.01f && Mathf.Abs(transform.localPosition.y - initialPosition.y) < 0.01f)
            {
                transform.localPosition = initialPosition; // Snap to original position to avoid floating point errors
                shakeTimeRemaining = 0; // Reset shake time remaining
            }
        }
    }

    // Public static method to trigger a scaled shake
    public static void TriggerShake(float duration, float magnitude, float damping, float frequency = 5)
    {
        if (instance == null)
        {
            Debug.LogError("CameraShake instance not found. Ensure a CameraShake script is attached to a GameObject in the scene.");
            return;
        }

        instance.shakeDuration = duration;
        instance.shakeMagnitude = magnitude;
        instance.dampingSpeed = damping;
        instance.shakeFrequency = frequency; // Set the frequency
        instance.shakeTimeRemaining = duration;
        instance.ShakeType = CameraShakeType.Scaled;
    }

    // Public static method to trigger an unscaled shake
    public static void TriggerUnscaledShake(float duration, float magnitude, float damping, float frequency = 5)
    {
        if (instance == null)
        {
            Debug.LogError("CameraShake instance not found. Ensure a CameraShake script is attached to a GameObject in the scene.");
            return;
        }

        instance.shakeDuration = duration;
        instance.shakeMagnitude = magnitude;
        instance.dampingSpeed = damping;
        instance.shakeFrequency = frequency; // Set the frequency
        instance.shakeTimeRemaining = duration;
        instance.ShakeType = CameraShakeType.Unscaled;
    }
}