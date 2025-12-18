using UnityEngine;

public class JabLaunch : MonoBehaviour
{
    [SerializeField] protected Collider2D JLaunchBox;
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
                    6f,
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
                    None,
                    //strongChargeAdd = 
                    6f,
                
                    //-Knockback, Scaling and Hit Pause-
                    //scalingChargeToApplyVar = 
                    None,
                    //scalingDamageMultiplier = 
                    None,
                    //knockbackAngle = 
                    35f,
                    //baseKnockbackStrengthX = 
                    6f,
                    //baseKnockbackStrengthY = 
                    4f,
                    //knockbackStrengthScalingX = 
                    1f,
                    //knockbackStrengthScalingY = 
                    1f,
                    //hitPauseAmount = 
                    0.2f,
                
                    //-Gravity Modifiers-
                    //isLaunchModifyGravity = 
                    true,
                    //launchGravityMultiplier = 
                    0.26f,
                    //GravityReturnDelay = 
                    1f,
                
                    //-Haptic Feedback-
                    //HapticLowFreq = 
                    0.5f,
                    //HapticHighFreq = 
                    1f,
                    //HapticDuration = 
                    0.4f
                );
                break;
            case "Shield":
                    break;
                default:
                    break;
                
            }
    }
}