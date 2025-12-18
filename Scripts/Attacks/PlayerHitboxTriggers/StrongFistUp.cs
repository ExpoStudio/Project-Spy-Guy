using UnityEngine;

public class StrongFistUp : MonoBehaviour
{
    [SerializeField] protected Collider2D strongFistUp;
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
                    8f,
                    //resistanceLowersDamageAmount = 
                    true,
                    //resistanceLowersHorizKnockback = 
                    true,
                    //doesLaunchUpwardIfResistance = 
                    true,
                    //resistanceLowersVertKnockback = 
                    true,
                    //horizKnockbackIfSuperArmor = 
                    2f,
                    //useSuperArmorKnockbackScaling = 
                    false,
                    //superArmorKnockbackScaleRate = 
                    0.1f,
                    //strongChargeAdd = 
                    None,
                
                    //-Knockback, Scaling and Hit Pause-
                    //scalingChargeToApplyVar = 
                    attacks.ScalingChargeToApply,
                    //scalingDamageMultiplier = 
                    0.24f,
                    //knockbackAngle = 
                    80f,
                    //baseKnockbackStrengthX = 
                    0.3f,
                    //baseKnockbackStrengthY = 
                    7f,
                    //knockbackStrengthScalingX = 
                    0.09f,
                    //knockbackStrengthScalingY = 
                    0.22f,
                    //hitPauseAmount = 
                    0.4f,
                
                    //-Gravity Modifiers-
                    //isLaunchModifyGravity = 
                    true,
                    //launchGravityMultiplier = 
                    0.46f,
                    //GravityReturnDelay = 
                    1f,
                
                    //-Haptic Feedback-
                    //HapticLowFreq = 
                    0.1f,
                    //HapticHighFreq = 
                    3f,
                    //HapticDuration = 
                    0.3f,
                    
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