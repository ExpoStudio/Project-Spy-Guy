
using System.Collections;
using UnityEngine;

public class HitPause : MonoBehaviour
{
    private Coroutine pauseRoutine;
    public void Stop(float duration)
    {
        if (pauseRoutine != null)
        {
            StopCoroutine(pauseRoutine);
            Time.timeScale = 1.0f;
        }
        pauseRoutine = StartCoroutine(Wait(duration));
    }
    IEnumerator Wait(float duration)
    {
        Time.timeScale = 0.0f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1.0f;
        pauseRoutine = null;
    }
}
