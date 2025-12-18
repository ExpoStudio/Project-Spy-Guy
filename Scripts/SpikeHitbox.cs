using UnityEngine;

public class SpikeHitbox : MonoBehaviour
{
    [SerializeField] protected Collider2D SpikeCollider;
    private void OnTriggerEnter2D(Collider2D other)
    {
        GameObject hitObject = other.gameObject;
        Health healthscript = hitObject.GetComponentInParent<Health>();
        Movement2 movement = hitObject.GetComponentInParent<Movement2>();
        // Add debug logs
        string hitObjectType = hitObject.tag;
        
        switch (hitObjectType)
        {
            case "Hurtbox":
                if (healthscript == null) Debug.LogWarning("Health component not found in parent chain of " + hitObject.name);
                if (movement == null) Debug.LogWarning("Movement2 component not found in parent chain of " + hitObject.name);
                int None = 0;
                // Add null check for movement before using its property
                bool normalizeKnockback = movement != null && movement.normalizeKnockbackIfNoShield;
                HitBox.CreateDamageHitbox
                (
                    //-Object Initialization-

                    //MonoBehaviour caller = 
                    this,
                    //hitBoxCollider = 
                    other,
                    //movement = 
                    movement,
                    //healthScript = 
                    healthscript,

                    //-Base Damage, Resistance and Super Armor-
                    //baseDamageAmount = 
                    18f,
                    //resistanceLowersDamageAmount = 
                    false,
                    //resistanceLowersHorizKnockback = 
                    false,
                    //doesLaunchUpwardIfResistance = 
                    true,
                    //resistanceLowersVertKnockback = 
                    false,
                    //horizKnockbackIfSuperArmor = 
                    1,
                    //useSuperArmorKnockbackScaling = 
                    false,
                    //superArmorKnockbackScaleRate = 
                    1,
                    //strongChargeAdd = 
                    1,

                    //-Knockback, Scaling and Hit Pause-
                    //scalingChargeToApplyVar = 
                    None,
                    //scalingDamageMultiplier = 
                    None,
                    //knockbackAngle = 
                    90f,
                    //baseKnockbackStrengthX = 
                    0f,
                    //baseKnockbackStrengthY = 
                    17f,
                    //knockbackStrengthScalingX = 
                    1f,
                    //knockbackStrengthScalingY = 
                    1f,
                    //hitPauseAmount = 
                    hitObject.transform.CompareTag("Player") ? 0.5f : None,

                    //-Gravity Modifiers-
                    //isLaunchModifyGravity = 
                    false,
                    //launchGravityMultiplier = 
                    1f,
                    //GravityReturnDelay = 
                    1f,

                    //-Haptic Feedback-
                    //HapticLowFreq = 
                    0.5f,
                    //HapticHighFreq = 
                    0.5f,
                    //HapticDuration = 
                    0.2f,

                    //-NewImplementations-
                    //NormalizeKNockbackIfNoShield =
                    normalizeKnockback,
                    //HitStun Amount (In seconds)
                    0.5f,
                    //Hit Entity Bounciness // typically for richochet attacks (0-1)
                    None
                );
                break;
            case "Shield":
                break;
            default:
                break;
        }
    }
}