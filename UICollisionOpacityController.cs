using UnityEngine;

public class UICollisionOpacityController : MonoBehaviour
{
    public Collider2D UICollider;
    public UICharacterListener uICharacterListener;

    void OnTriggerStay2D(Collider2D other)
    {
        GameObject gameObject = other.gameObject;
        if (gameObject.CompareTag("Player"))
        {
            uICharacterListener.MaximumAlpha = 0.20f;
        }
        else
        {
            return;
        }
    }
    void OnTriggerExit2D(Collider2D other)
    {
        GameObject gameObject = other.gameObject;
        if (gameObject.CompareTag("Player"))
        {
            uICharacterListener.MaximumAlpha = 1f;
        }
        else
        {
            return;
        }
    }
}
