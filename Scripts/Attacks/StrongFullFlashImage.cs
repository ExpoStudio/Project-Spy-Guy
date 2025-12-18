using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class StrongFullFlashImage : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private Material imageMaterial;
    [SerializeField] private AnimationCurve flashCurve;

    private Color def = Color.yellow;
    private Coroutine flashRoutine;

    void Awake()
    {
        image = GetComponent<Image>();
        imageMaterial = new Material(image.material); // Create instance
        image.material = imageMaterial; // Assign the new instance}
        imageMaterial.SetFloat("_FlashAmount", 0f);
    }

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
        SetFlashColor(def);
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
        if (imageMaterial != null)
            imageMaterial.SetColor("_FlashColor", color);
    }

    public void SetFlashAmount(float amount)
    {
        if (imageMaterial != null)
            imageMaterial.SetFloat("_FlashAmount", amount);
    }
}
