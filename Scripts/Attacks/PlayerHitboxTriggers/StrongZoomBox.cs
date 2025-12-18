using System.Collections;
using UnityEngine;

public class StrongZoomBox : MonoBehaviour
{
    [SerializeField] protected Collider2D strongZoomBox;
    [SerializeField] protected HitPause hitPause;
    [SerializeField] protected CameraFollow Camera;
    [SerializeField] protected Attacks attacks;
    private bool isZooming;
    private Coroutine DoZooming;
    private void OnTriggerEnter2D(Collider2D other)
    {

        GameObject hitObject = other.gameObject;
        Movement2 movement = hitObject.GetComponentInParent<Movement2>();
        string hitObjectType = hitObject.tag;

        switch (hitObjectType)
        {
            case "Hurtbox":
                if (!attacks.zoomedAlready)
                    if (movement.RigBod != null && movement != null && !hitObject.CompareTag("Boss"))
                    {
                        if (DoZooming != null)
                        {
                            StopCoroutine(DoZooming);
                            DoZooming = null;
                        }
                        DoZooming = StartCoroutine(DoZoom(0.2f));
                    }
                    else return;
                break;
            case "Shield":
                break;
            default:
                break;
        }
    }

    private IEnumerator DoZoom(float duration)
    {
        attacks.zoomBoxHit = true;
        Camera.Zoom(10f, 1.5f, 0.3f);
        CameraShake.TriggerUnscaledShake(1.1f, 0.15f, 0.5f);
        yield return new WaitForSeconds(duration);
        attacks.zoomBoxHit = false;
        DoZooming = null;
    }
}
