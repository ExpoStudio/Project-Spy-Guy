using UnityEngine;

public class LeapDive : MonoBehaviour
{
    [SerializeField] protected Collider2D leapDive;
    private void OnTriggerEnter2D(Collider2D other)
    {
        GameObject hitObject = other.gameObject;
        Health healthscript = hitObject.GetComponentInParent<Health>();
        Movement2 movement = hitObject.GetComponentInParent<Movement2>();
        Movement2 selfMovement = GetComponentInParent<Movement2>();
        Attacks attacks = GetComponentInParent<Attacks>();
        string hitObjectType = hitObject.tag;

        switch (hitObjectType)
        {
            case "Hurtbox":
                if (movement.hasSuperArmor)
                {
                    attacks.attackMove = AttackMove.None;
                    if (attacks.DiveRoutine != null)
                    attacks.StopCoroutine(attacks.DiveRoutine);
                    attacks.StartCoroutine(attacks.Hitstun(5f));
                    attacks.DiveHitbox.enabled = false;
                    attacks.DiveHurtbox.enabled = false;
                    attacks.HurtBox.enabled = true;
                    selfMovement.RigBod.linearVelocity = new Vector2(-5f*selfMovement.Spr_Dir(),4f);
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
                    5f,
                    //resistanceLowersDamageAmount = 
                    false,
                    //resistanceLowersHorizKnockback = 
                    true,
                    //doesLaunchUpwardIfResistance = 
                    true,
                    //resistanceLowersVertKnockback = 
                    false,
                    //horizKnockbackIfSuperArmor = 
                    7f,
                    //useSuperArmorKnockbackScaling = 
                    false,
                    //superArmorKnockbackScaleRate = 
                    None,
                    //strongChargeAdd = 
                    7f,
                
                    //-Knockback, Scaling and Hit Pause-
                    //scalingChargeToApplyVar = 
                    None,
                    //scalingDamageMultiplier = 
                    None,
                    //knockbackAngle = 
                    55f,
                    //baseKnockbackStrengthX = 
                    10f,
                    //baseKnockbackStrengthY = 
                    5f,
                    //knockbackStrengthScalingX = 
                    1f,
                    //knockbackStrengthScalingY = 
                    0.5f,
                    //hitPauseAmount = 
                    0.4f,
                
                    //-Gravity Modifiers-
                    //isLaunchModifyGravity = 
                    true,
                    //launchGravityMultiplier = 
                    0.4f,
                    //GravityReturnDelay = 
                    1f,
                
                    //-Haptic Feedback-
                    //HapticLowFreq = 
                    2f,
                    //HapticHighFreq = 
                    0.1f,
                    //HapticDuration = 
                    0.1f,
                
                    //-NewImplementations-
                    //NormalizeKNockbackIfNoShield =
                    movement.normalizeKnockbackIfNoShield
                );
                CoolEffects.SlowDownTime(this,0.2f,0.2f);
                break;
            case "Shield":
                break;
            default:
                break;
        }
    }
}