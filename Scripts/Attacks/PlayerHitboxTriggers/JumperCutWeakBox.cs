using UnityEngine;

public class JumperCutWeakHitbox : MonoBehaviour
{
    [SerializeField] protected Collider2D JumperCutWeakHitboxCollider;
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
                if (!attacks.didAttackLand)
                {
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
                        3f,
                        //resistanceLowersDamageAmount = 
                        true,
                        //resistanceLowersHorizKnockback = 
                        true,
                        //doesLaunchUpwardIfResistance = 
                        false,
                        //resistanceLowersVertKnockback = 
                        true,
                        //horizKnockbackIfSuperArmor = 
                        None,
                        //useSuperArmorKnockbackScaling = 
                        false,
                        //superArmorKnockbackScaleRate = 
                        0f,
                        //strongChargeAdd = 
                        1f,
                    
                        //-Knockback, Scaling and Hit Pause-
                        //scalingChargeToApplyVar = 
                        None,
                        //scalingDamageMultiplier = 
                        None,
                        //knockbackAngle = 
                        45f,
                        //baseKnockbackStrengthX = 
                        3f,
                        //baseKnockbackStrengthY = 
                        3f,
                        //knockbackStrengthScalingX = 
                        1f,
                        //knockbackStrengthScalingY = 
                        0f,
                        //hitPauseAmount = 
                        0.1f,
                    
                        //-Gravity Modifiers-
                        //isLaunchModifyGravity = 
                        false,
                        //launchGravityMultiplier = 
                        1f,
                        //GravityReturnDelay = 
                        1f,
                    
                        //-Haptic Feedback-
                        //HapticLowFreq = 
                        0.0f,
                        //HapticHighFreq = 
                        0.5f,
                        //HapticDuration = 
                        0.1f,
                    
                        //-NewImplementations-
                        //NormalizeKNockbackIfNoShield =
                        movement.normalizeKnockbackIfNoShield,
                        //HitStun Amount (In seconds)
                        0.2f,
                        //Hit Entity Bounciness // typically for richochet attacks (0-1)
                        0f
                    );
                }
                break;
            case "Shield":
                break;
            default:
                break;
        }
    }
}