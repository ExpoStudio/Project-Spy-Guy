using UnityEngine;

public class StrongFist : MonoBehaviour
{
[SerializeField] protected Collider2D strongFist;
[SerializeField] protected float storedGravDelay;
private void OnTriggerEnter2D(Collider2D other)
{
    GameObject hitObject = other.gameObject;
    Health healthscript = hitObject.GetComponentInParent<Health>();
    Movement2 movement = hitObject.GetComponentInParent<Movement2>();
    Attacks attacks = GetComponentInParent<Attacks>();
    string hitObjectType = hitObject.tag;
    switch (hitObjectType)
        {
        case "Hurtbox":
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
                10f,
                //resistanceLowersDamageAmount = 
                true,
                //resistanceLowersHorizKnockback = 
                true,
                //doesLaunchUpwardIfResistance = 
                false,
                //resistanceLowersVertKnockback = 
                true,
                //horizKnockbackIfSuperArmor = 
                3f,
                //useSuperArmorKnockbackScaling = 
                true,
                //superArmorKnockbackScaleRate = 
                0.26f,
                //strongChargeAdd = 
                None,
            
                //-Knockback, Scaling and Hit Pause-
                //scalingChargeToApplyVar = 
                attacks.ScalingChargeToApply,
                //scalingDamageMultiplier = 
                0.35f,
                //knockbackAngle = 
                45f,
                //baseKnockbackStrengthX = 
                9f,
                //baseKnockbackStrengthY = 
                4f,
                //knockbackStrengthScalingX = 
                0.12f,
                //knockbackStrengthScalingY = 
                0.06f,
                //hitPauseAmount = 
                0.2f,
            
                //-Gravity Modifiers-
                //isLaunchModifyGravity = 
                true,
                //launchGravityMultiplier = 
                0.46f,
                //GravityReturnDelay = 
                1f,
            
                //-Haptic Feedback-
                //HapticLowFreq = 
                0.9f,
                //HapticHighFreq = 
                1f,
                //HapticDuration = 
                0.2f,

                movement.normalizeKnockbackIfNoShield
            );
            break;
        case "Shield":
                break;
            default:
                break;
            
        }
    }
}