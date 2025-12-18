using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class HapticHold : MonoBehaviour
{
    public static bool isVibrating = false;

    public static void StartVibration(float lowFrequency, float highFrequency)
    {
        if (Gamepad.current != null)
        {
            Gamepad.current.SetMotorSpeeds(lowFrequency, highFrequency);
            isVibrating = true;
        }
    }

    public static void StopVibration()
    {
        if (Gamepad.current != null)
        {
            Gamepad.current.SetMotorSpeeds(0f, 0f);
            isVibrating = false;
        }
    }

    public static void VibrateForDuration(float lowFrequency, float highFrequency, float duration, MonoBehaviour caller)
    {
        StartVibration(lowFrequency, highFrequency);
        caller.StartCoroutine(StopVibrationAfterDelay(duration));
    }

    private static IEnumerator StopVibrationAfterDelay(float duration)
    {
        yield return new WaitForSeconds(duration);
        StopVibration();
    }

    // --- NEW: Vibrate for a specific PlayerInput's gamepad ---
    public static void StartVibration(float lowFrequency, float highFrequency, PlayerInput playerInput)
    {
        var gamepad = GetGamepadForPlayer(playerInput);
        if (gamepad != null)
        {
            gamepad.SetMotorSpeeds(lowFrequency, highFrequency);
            isVibrating = true;
        }
    }

    public static void StopVibration(PlayerInput playerInput)
    {
        var gamepad = GetGamepadForPlayer(playerInput);
        if (gamepad != null)
        {
            gamepad.SetMotorSpeeds(0f, 0f);
            isVibrating = false;
        }
    }

    public static void VibrateForDuration(float lowFrequency, float highFrequency, float duration, MonoBehaviour caller, PlayerInput playerInput)
    {
        StartVibration(lowFrequency, highFrequency, playerInput);
        caller.StartCoroutine(StopVibrationAfterDelay(duration, playerInput));
    }

    private static IEnumerator StopVibrationAfterDelay(float duration, PlayerInput playerInput)
    {
        yield return new WaitForSeconds(duration);
        StopVibration(playerInput);
    }

    private static Gamepad GetGamepadForPlayer(PlayerInput playerInput)
    {
        foreach (var device in playerInput.devices)
        {
            if (device is Gamepad gamepad)
                return gamepad;
        }
        return null;
    }
}