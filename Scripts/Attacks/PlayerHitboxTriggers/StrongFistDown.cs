using System.Collections;
using UnityEngine;

public class StrongFistDown : MonoBehaviour
{
    [SerializeField] protected Collider2D strongFistDown;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnTriggerEnter2D(Collider2D other)
    {
        GameObject hitObject = other.gameObject;
        string hitboxType = hitObject.tag;
        switch (hitboxType)
        {
            case "Hurtbox":
            Health healthscript = hitObject.GetComponentInParent<Health>();
            Movement2 movement = hitObject.GetComponentInParent<Movement2>();
            Attacks attacks = GetComponentInParent<Attacks>();
            if (healthscript == null || movement == null || attacks == null)
            {
                Debug.LogWarning("Missing component(s): " +
                    (healthscript == null ? "Health " : "") +
                    (movement == null ? "Movement2 " : "") +
                    (attacks == null ? "Attacks " : "")
                );
                return;
            }
                int None = 0;
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
                    12f,
                    //resistanceLowersDamageAmount = 
                    false,
                    //resistanceLowersHorizKnockback = 
                    false,
                    //doesLaunchUpwardIfResistance = 
                    false,
                    //resistanceLowersVertKnockback = 
                    true,
                    //horizKnockbackIfSuperArmor = 
                    9f,
                    //useSuperArmorKnockbackScaling = 
                    true,
                    //superArmorKnockbackScaleRate = 
                    0.7f,
                    //strongChargeAdd = 
                    None,
                
                    //-Knockback, Scaling and Hit Pause-
                    //scalingChargeToApplyVar = 
                    attacks.ScalingChargeToApply,
                    //scalingDamageMultiplier = 
                    0.42f,
                    //knockbackAngle = 
                    50f,
                    //baseKnockbackStrengthX = 
                    0.1f,
                    //baseKnockbackStrengthY = 
                    2f,
                    //knockbackStrengthScalingX = 
                    0.12f,
                    //knockbackStrengthScalingY = 
                    0.01f,
                    //hitPauseAmount = 
                    0.8f,
                
                    //-Gravity Modifiers-
                    //isLaunchModifyGravity = 
                    true,
                    //launchGravityMultiplier = 
                    0.16f,
                    //GravityReturnDelay = 
                    3f,
                
                    //-Haptic Feedback-
                    //HapticLowFreq = 
                    0.5f,
                    //HapticHighFreq = 
                    2f,
                    //HapticDuration = 
                    0.2f,
                
                    //-NewImplementations-
                    //NormalizeKNockbackIfNoShield =
                    movement.normalizeKnockbackIfNoShield
                );
                break;
            case "Shield":

                break;
            default:
                return;
        }
    }
}

