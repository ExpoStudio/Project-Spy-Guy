using UnityEngine;

public class DiveBomb : MonoBehaviour
{
    [SerializeField] protected Collider2D DiveBombCollider;
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


                if(movement.IsGrounded()) //The enemy is on the ground
                {
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
                        8f + knockbackFromFall,
                        //resistanceLowersDamageAmount = 
                        true,
                        //resistanceLowersHorizKnockback = 
                        false,
                        //doesLaunchUpwardIfResistance = 
                        true,
                        //resistanceLowersVertKnockback = 
                        false,
                        //horizKnockbackIfSuperArmor = 
                        3f + knockbackFromFall,
                        //useSuperArmorKnockbackScaling = 
                        false,
                        //superArmorKnockbackScaleRate = 
                        0f,
                        //strongChargeAdd = 
                        5f + knockbackFromFall,
                    
                        //-Knockback, Scaling and Hit Pause-
                        //scalingChargeToApplyVar = 
                        0f,
                        //scalingDamageMultiplier = 
                        0f,
                        //knockbackAngle = 
                        85f,
                        //baseKnockbackStrengthX = 
                        2f,
                        //baseKnockbackStrengthY = 
                        3f,
                        //knockbackStrengthScalingX = 
                        0.7f,
                        //knockbackStrengthScalingY = 
                        0.7f,
                        //hitPauseAmount = 
                        0.6f,
                    
                        //-Gravity Modifiers-
                        //isLaunchModifyGravity = 
                        true,
                        //launchGravityMultiplier = 
                        0.5f,
                        //GravityReturnDelay = 
                        1f,
                    
                        //-Haptic Feedback-
                        //HapticLowFreq = 
                        1f,
                        //HapticHighFreq = 
                        0.5f,
                        //HapticDuration = 
                        0.2f,
                    
                        //-NewImplementations-
                        //NormalizeKNockbackIfNoShield =
                        movement.normalizeKnockbackIfNoShield
                    );
                    CoolEffects.SlowDownTime(this,0.2f,0.2f);
                    Debug.Log("Grounded hitbox created with superArmor knockback horizontal scaling: " + (7f + attacks.Inputs.RigBod.linearVelocityY < -15f ? attacks.Inputs.RigBod.linearVelocityY/-12 : 0f));
                }
                else if (!movement.IsGrounded()) //The enemy is in the air
                {
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
                        8f + knockbackFromFall,
                        //resistanceLowersDamageAmount = 
                        true,
                        //resistanceLowersHorizKnockback = 
                        false,
                        //doesLaunchUpwardIfResistance = 
                        true,
                        //resistanceLowersVertKnockback = 
                        false,
                        //horizKnockbackIfSuperArmor = 
                        1f + knockbackFromFall,
                        //useSuperArmorKnockbackScaling = 
                        false,
                        //superArmorKnockbackScaleRate = 
                        None,
                        //strongChargeAdd = 
                        5f + knockbackFromFall,
                    
                        //-Knockback, Scaling and Hit Pause-
                        //scalingChargeToApplyVar = 
                        0f,
                        //scalingDamageMultiplier = 
                        0f,
                        //knockbackAngle = 
                        285f,
                        //baseKnockbackStrengthX = 
                        2f,
                        //baseKnockbackStrengthY = 
                        8f + knockbackFromFall,
                        //knockbackStrengthScalingX = 
                        1f,
                        //knockbackStrengthScalingY = 
                        1f,
                        //hitPauseAmount = 
                        0.5f,
                    
                        //-Gravity Modifiers-
                        //isLaunchModifyGravity = 
                        false,
                        //launchGravityMultiplier = 
                        1f,
                        //GravityReturnDelay = 
                        1f,
                    
                        //-Haptic Feedback-
                        //HapticLowFreq = 
                        1f,
                        //HapticHighFreq = 
                        0.2f,
                        //HapticDuration = 
                        0.2f,
                    
                        //-NewImplementations-
                        //NormalizeKNockbackIfNoShield =
                        movement.normalizeKnockbackIfNoShield
                    );
                    CoolEffects.SlowDownTime(this,0.2f,0.2f);
                }
                break;
            case "Shield":
                break;
            default:
                break;
        }
    }
}
