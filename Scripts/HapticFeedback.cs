using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class HapticFeedback : MonoBehaviour
{
    public static void Vibrate()
    {
        if (SystemInfo.supportsVibration)
        {
            Handheld.Vibrate();
        }
    }

    public static void VibrateGamepad(float lowFrequency, float highFrequency, float duration, MonoBehaviour caller)
    {
        if (Gamepad.current != null)
        {
            Gamepad.current.SetMotorSpeeds(lowFrequency, highFrequency);
            caller.StartCoroutine(StopVibrationAfterDelay(duration));
        }
    }

    private static IEnumerator StopVibrationAfterDelay(float duration)
    {
        yield return new WaitForSeconds(duration);
        if (Gamepad.current != null)
        {
            Gamepad.current.SetMotorSpeeds(0f, 0f); // Stop vibration
        }
    }
}
