using UnityEngine;
using System.Collections;

public class CoolEffects : MonoBehaviour
{
    private static Coroutine slowRoutine = null;
    private static bool running = false;
    public static void SlowDownTime(MonoBehaviour caller, float timeScale, float duration)
    {
        if (!running)
        {
            if (slowRoutine != null)
            {
                caller.StopCoroutine(slowRoutine);
            }
            slowRoutine = caller.StartCoroutine(SlowDown(timeScale, duration));
            running = true;
        }
    }
    /// <summary>
    /// Slows down the time scale (between 0 and 1) for a given duration.
    /// </summary>
    /// <param name="timeScale"></param>
    /// <param name="duration"></param>
    /// <returns></returns>
    private static IEnumerator SlowDown(float timeScale, float duration)
    {
        Debug.Log("SetTrue");
        Time.timeScale = timeScale;
        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = 1;
        running = false;
    }
}