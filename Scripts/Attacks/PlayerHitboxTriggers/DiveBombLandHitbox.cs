using UnityEngine;

public class DiveBombLandHitbox : MonoBehaviour
{
    [SerializeField] protected Collider2D DiveBombLandCollider;
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

                float fallVelocity = Mathf.Abs(attacks.Inputs.RigBod.linearVelocity.y);
                float knockbackFromFall = Mathf.Max(0, fallVelocity - 10f);

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
                    10f + Mathf.Max(0, Mathf.Abs(attacks.StoredVelocity)-10f),
                    //resistanceLowersDamageAmount = 
                    false,
                    //resistanceLowersHorizKnockback = 
                    true,
                    //doesLaunchUpwardIfResistance = 
                    false,
                    //resistanceLowersVertKnockback = 
                    true,
                    //horizKnockbackIfSuperArmor = 
                    8f + Mathf.Abs(knockbackFromFall),
                    //useSuperArmorKnockbackScaling = 
                    false,
                    //superArmorKnockbackScaleRate = 
                    None,
                    //strongChargeAdd = 
                    8f + Mathf.Abs(knockbackFromFall),
                
                    //-Knockback, Scaling and Hit Pause-
                    //scalingChargeToApplyVar = 
                    None,
                    //scalingDamageMultiplier = 
                    1f,
                    //knockbackAngle = 
                    80f,
                    //baseKnockbackStrengthX = 
                    4f,
                    //baseKnockbackStrengthY = 
                    12f,
                    //knockbackStrengthScalingX = 
                    0.7f,
                    //knockbackStrengthScalingY = 
                    0.7f,
                    //hitPauseAmount = 
                    0.5f,
                
                    //-Gravity Modifiers-
                    //isLaunchModifyGravity = 
                    true,
                    //launchGravityMultiplier = 
                    0.4f,
                    //GravityReturnDelay = 
                    2f,
                
                    //-Haptic Feedback-
                    //HapticLowFreq = 
                    2f,
                    //HapticHighFreq = 
                    0.7f,
                    //HapticDuration = 
                    0.5f,
                
                    //-NewImplementations-
                    //NormalizeKNockbackIfNoShield =
                    movement.normalizeKnockbackIfNoShield
                );
                CameraShake.TriggerUnscaledShake(0.2f, 0.2f, 0.5f);
                break;
            case "Shield":
                break;
            default:
                break;
        }
    }
}