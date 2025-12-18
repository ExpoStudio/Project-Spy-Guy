using UnityEngine;

public class JumperCutInitialHitbox : MonoBehaviour
{
    [SerializeField] protected Collider2D JumperCutInitialCollider;
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
                    12f,
                    //resistanceLowersDamageAmount = 
                    true,
                    //resistanceLowersHorizKnockback = 
                    true,
                    //doesLaunchUpwardIfResistance = 
                    true,
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
                    89f,
                    //baseKnockbackStrengthX = 
                    1f,
                    //baseKnockbackStrengthY = 
                    9f,
                    //knockbackStrengthScalingX = 
                    1f,
                    //knockbackStrengthScalingY = 
                    1f,
                    //hitPauseAmount = 
                    0.8f,
                
                    //-Gravity Modifiers-
                    //isLaunchModifyGravity = 
                    true,
                    //launchGravityMultiplier = 
                    0.5f,
                    //GravityReturnDelay = 
                    3f,
                
                    //-Haptic Feedback-
                    //HapticLowFreq = 
                    1.2f,
                    //HapticHighFreq = 
                    0.3f,
                    //HapticDuration = 
                    0.2f,
                
                    //-NewImplementations-
                    //NormalizeKNockbackIfNoShield =
                    movement.normalizeKnockbackIfNoShield,
                    //HitStun Amount (In seconds)
                    0.5f,
                    //Hit Entity Bounciness // typically for richochet attacks (0-1)
                    0.3f
                );
                CoolEffects.SlowDownTime(this, 0.1f, 0.6f);
                attacks.didAttackLand = true;
                break;
            case "Shield":
                break;
            default:
                break;
        }
    }
}
