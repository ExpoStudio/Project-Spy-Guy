using UnityEngine;

public class JabScript : MonoBehaviour
{
    [SerializeField] protected Collider2D JabHitbox;
    [SerializeField] protected float storedGravDelay;
    private void OnTriggerEnter2D(Collider2D other)
    {
        GameObject hitObject = other.gameObject;
        Health healthscript = hitObject.GetComponentInParent<Health>();
        Movement2 movement = hitObject.GetComponentInParent<Movement2>();
        string hitObjectType = hitObject.tag;
        switch (hitObjectType)
            {
            case "Hurtbox":
                Debug.Log("Hitlogged");
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
                    2,
                    //resistanceLowersDamageAmount = 
                    true,
                    //resistanceLowersHorizKnockback =
                    true,
                    //doesLaunchUpwardIfResistance = 
                    false,
                    //resistanceLowersVertKnockback = 
                    true,
                    //horizKnockbackIfSuperArmor = 
                    0,
                    //useSuperArmorKnockbackScaling = 
                    false,
                    //superArmorKnockbackScaleRate = 
                    0f,
                    //strongChargeAdd = 
                    2f,

                    //-Knockback, Scaling and Hit Pause-
                    //scalingChargeToApplyVar = 
                    0,
                    //scalingDamageMultiplier = 
                    0,
                    //knockbackAngle = 
                    0f,
                    //baseKnockbackStrengthX = 
                    0f,
                    //baseKnockbackStrengthY = 
                    0f,
                    //knockbackStrengthScalingX = 
                    1f,
                    //knockbackStrengthScalingY = 
                    1f,
                    //hitPauseAmount = 
                    0.2f,

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
                    0.5f,
                    //HapticDuration = 
                    0.1f
                );
                break;
            case "Shield":
                    break;
                default:
                    break;
                
            }
    }
}
