using System.Collections;
using UnityEngine;

public class DamageFlash : MonoBehaviour
{
    [ColorUsage(true, true)]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Material spriteMaterial;
    [SerializeField] private AnimationCurve flashCurve;

    private Coroutine flashRoutine;
    public float duration;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteMaterial = spriteRenderer.material;
    }
    // Update is called once per frame
    public void DmgFlash()
    {
        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
        }

        flashRoutine = StartCoroutine(FlashRoutine());
    }
    private IEnumerator FlashRoutine()
    {
        SetFlashColor(Color.red);
        float currFlashAmount = 0f;
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            currFlashAmount = Mathf.Lerp(1f, 0, elapsedTime / duration);
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
