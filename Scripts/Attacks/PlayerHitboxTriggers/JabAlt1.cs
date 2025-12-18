using UnityEngine;

public class JabAlt1 : MonoBehaviour
{
    [SerializeField] protected Collider2D JLaunchBox;
    [SerializeField] protected Movement2 Direction;
    [SerializeField] protected HitPause hitPause;
    private Movement2 affectedMovement;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnTriggerEnter2D(Collider2D other)
    {
        GameObject hitObject = other.gameObject;
        Health healthscript = hitObject.GetComponent<Health>();
        Movement2 movement = hitObject.GetComponent<Movement2>();
        string hitbox = hitObject.name;

        switch (hitbox)
        {
            case "Hurtbox":
                JabHitboxLogic(hitObject, healthscript, movement);
                break;
            case "Shield":

                break;
            default:
                return;
        }
    }

    private void JabHitboxLogic(GameObject hitObject, Health healthscript, Movement2 movement)
    {
        if (healthscript != null)
        {
            healthscript.TakeDamage(8f);
            HapticFeedback.VibrateGamepad(0f, 2f, 0.1f, this);
        }

        if (movement.RigBod != null && movement != null && !hitObject.CompareTag("Boss"))
        {
            affectedMovement = movement;
            hitPause.Stop(0.4f);
            if (!movement.hasSuperArmor)
            {
                movement.RigBod.linearVelocity = new Vector2(6f * (Direction.FacingRight ? 1f : -1f), 3f);
            }
            movement.RigBod.gravityScale *= 0.46f;
            Invoke(nameof(ReturnGravity), 1f);
        }
    }

    void ReturnGravity()
    {
        if (affectedMovement != null && affectedMovement.RigBod != null)
        {
            affectedMovement.RigBod.gravityScale /= 0.46f;
        }
    }
}

