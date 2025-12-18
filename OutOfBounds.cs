using UnityEngine;
using UnityEngine.InputSystem;

public class OutOfBounds : MonoBehaviour
{
    // Manually assigned colliders triggers
    [SerializeField] private Collider2D[] OutOfBoundsColliderTriggers = new Collider2D[4];

    private void OnTriggerStay2D(Collider2D other)
    {
        GameObject outOfBoundsObject = other.gameObject;
        Movement2 movement = outOfBoundsObject.GetComponentInParent<Movement2>();
        string hitObjectType = outOfBoundsObject.tag;
        if (hitObjectType == "Player")
        {
            if (movement != null)
            {
                movement.OutOfBoundsTimer += Time.deltaTime;
                if (movement.OutOfBounds)
                {
                    CameraPositionController.NotifyOutOfBounds(movement);
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        GameObject outOfBoundsObject = other.gameObject;
        Movement2 movement = outOfBoundsObject.GetComponentInParent<Movement2>();
        string hitObjectType = outOfBoundsObject.tag;
        if (hitObjectType == "Player")
        {
            movement.OutOfBoundsTimer = 0f;
        }
    }
}
