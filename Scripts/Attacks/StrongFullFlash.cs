using System.Collections;
using UnityEngine;

public class StrongFullFlash : MonoBehaviour
{
    [ColorUsage(true, true)]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Material spriteMaterial;
    [SerializeField] private AnimationCurve flashCurve;

    private Color def = Color.yellow;
    private Coroutine flashRoutine;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteMaterial = spriteRenderer.material;
    }
    // Update is called once per frame
    public void DmgFlash(float amount, float duration)
    {
        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
        }

        flashRoutine = StartCoroutine(FlashRoutine(amount, duration));
    }
    private IEnumerator FlashRoutine(float amount, float duration)
    {
        SetFlashColor(Color.yellow);
        float currFlashAmount = 0f;
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            currFlashAmount = Mathf.Lerp(amount, 0, elapsedTime / duration);
            SetFlashAmount(currFlashAmount);
            yield return null;
        }
        flashRoutine = null;
    }

    public void SetFlashColor(Color color)
    {
        spriteMaterial.SetColor("_FlashColor", color);
    }

    public void SetFlashAmount(float amount)
    {
        spriteMaterial.SetFloat("_FlashAmount", amount);
    }
}
