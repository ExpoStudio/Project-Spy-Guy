using System.Collections;
using System.Runtime.Serialization.Formatters;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

/// <summary>
/// Class that contains the essentials for a hitboxes
/// </summary>
public class HitBox : MonoBehaviour
{
    /// <summary>
    /// Builds the hitbox and its attributes from the ground up.
    /// <para> -Object Initialization- </para>
    /// <para><paramref name="hitBoxCollider"/> - The collider variable of the hitbox setting off the trigger. This is used to detect collisions.</para>
    /// <para><paramref name="movement"/> - Accessing the collidee's movement script for RigidBody.</para>
    /// <para><paramref name="healthScript"/> - Accessing the collidee's health script for Damage infliction.</para>
    /// <para>_______________________________ </para>
    /// <para> -Base Damage, Resistance, Super Armor and Charge- </para>
    /// <para> Important Note, Resistance scales after all calculations have been performed.</para>
    /// <para><paramref name="baseDamageAmount"/> - Amount of damage to inflict.</para>
    /// <para><paramref name="resistanceLowersDamageAmount"/> - Determines whether or not having resistance will resist hitox damage. Set to true by default.</para>
    /// <para><paramref name="resistanceLowersHorizKnockback"/> - Determines whether or not having resistance will deter horizontal knockback. Set to true by default. If false, knockback will scale regularly and ignores resistance values.</para>
    /// <para><paramref name="doesLaunchUpwardIfResistance"/> - Determines if the hitbox can do and scale upward knockback for stronger attacks with objects having resistance. Only effects hit objects with a resistance value greater than 0. False by default.</para>
    /// <para><paramref name="resistanceLowersVertKnockback"/> - If doesLaunchUpwardIfResistance is true, this determines whether or not having resistance will deter vertical knockback. Set to true by default. If false, knockback will scale regularly and ignores resistance values.</para>
    /// <para><paramref name="horizKnockbackIfSuperArmor"/> - Horizontal hitbox knockback if hit object has super armor. Set 0 for no knockback.</para>
    /// <para><paramref name="useSuperArmorKnockbackScaling"/> - Whether or not knockback for super armor is enabled and scales for stronger attacks.</para>
    /// <para><paramref name="superArmorKnockbackScaleRate"/> - If knockback scaling for super armor is enabled, this is the rate at which knockback will scale.</para>
    /// <para><paramref name="strongChargeAdd"/> - The amount of charge added if the attack connects.</para>
    /// <para>_______________________________</para>
    /// <para> -Knockback, Scaling and Hit Pause- </para>
    /// <para><paramref name="scalingChargeToApplyVar"/> - Place the scalingChargeToApply variable here</para>
    /// <para><paramref name="scalingDamageMultiplier"/> - Multiplier factor to scale damage added from strong attacks</para>
    /// <para><paramref name="knockbackAngle"/> - Launch angle in Euler Angles</para>
    /// <para><paramref name="baseKnockbackStrengthX"/> - Sets the unscaled knockback x value</para>
    /// <para><paramref name="baseKnockbackStrengthY"/> - Sets the unscaled knockback y value</para>
    /// <para><paramref name="knockbackStrengthScalingX"/> - Scaling of additional knockback from the scalingChargeToApply variable. On rare occasions greater than 1</para>
    /// <para><paramref name="knockbackStrengthScalingY"/> - Scaling of additional knockback from the scalingChargeToApply variable. On rare occasions greater than 1</para>
    /// <para><paramref name="hitPauseAmount"/> - Sets the duration of the Hit Pause effect</para>
    /// <para>_______________________________</para>
    /// <para> -Gravity Modifiers- </para>
    /// <para><paramref name="isLaunchModifyGravity"/> - Tweaks the gravity of the hit object for better distance travelling and emphasis</para>
    /// <para><paramref name="launchGravityMultiplier"/> - If isLaunchModifyGravity is true, This value multiplies the objects gravity. Set to any float greater than 1 for heavier gravity; Set a decimal less than 1 should be used for lighter.</para>
    /// <para><paramref name="gravityReturnDelay"/> - This is the gravityReturnDelay before the hit object returns to its original gravity value.</para>
    /// <para>_______________________________</para>
    /// <para> -Haptic Feedback- </para>
    /// <para><paramref name="HapticLowFreq"/> - The low frequency vibration</para>
    /// <para><paramref name="HapticHighFreq"/> - The high frequency vibration</para>
    /// <para><paramref name="HapticDuration"/> - The length of the vibration</para>
    /// <para>_______________________________</para>
    /// <para> -New Implementations- </para>
    /// <para><paramref name="normalizeKnockbackIfNoShield"/> - Normalizes the knockback if the hit object runs out of shield. Value is retrieved from the hit object's movement script. Set this value in the Inspector.</para>
    /// <para><paramref name="hitStunAmount"/> - Amount of time in seconds until the hit entity can move or register inputs. Only works on bosses in certain states</para>
    /// <para><paramref name="bounciness"/> - Modifies the entity bounce when hit, typically if you want it to ricochet off a surface. Time bounciness is applies depends directly on the amount of time stated in gravityReturnDelay. Clamped between 0 and 1</para>
    /// </summary>
    public static Coroutine returnRoutine;
    public static Coroutine bouncinessRoutine;
    public static void CreateDamageHitbox
    (
        MonoBehaviour caller,
        Collider2D hitBoxCollider = null,
        Movement2 movement = null,
        Health healthScript = null,


        float baseDamageAmount = 0,
        bool resistanceLowersDamageAmount = true,
        bool resistanceLowersHorizKnockback = true,
        bool doesLaunchUpwardIfResistance = false,
        bool resistanceLowersVertKnockback = true,
        float horizKnockbackIfSuperArmor = 0,
        bool useSuperArmorKnockbackScaling = false,
        float superArmorKnockbackScaleRate = 0f,
        float strongChargeAdd = 0f,


        float scalingChargeToApplyVar = 0,
        float scalingDamageMultiplier = 0,
        float knockbackAngle = 0f,
        float baseKnockbackStrengthX = 1f,
        float baseKnockbackStrengthY = 1f,
        float knockbackStrengthScalingX = 0f,
        float knockbackStrengthScalingY = 0f,
        float hitPauseAmount = 1f,

        bool isLaunchModifyGravity = false,
        float launchGravityMultiplier = 1f,
        float gravityReturnDelay = 1f,

        float HapticLowFreq = 0,
        float HapticHighFreq = 0,
        float HapticDuration = 0,

        bool normalizeKnockbackIfNoShield = true,
        float hitStunAmount = 0.4f,
        float bounciness = 0.5f
    )
    {
        Movement2 _directionCheck;
        StrongCharge _strongCharge;
        bounciness = Mathf.Clamp(bounciness, 0f, 1f);

        bool _storedBoolLaunchUpIfResistance;

        _storedBoolLaunchUpIfResistance = doesLaunchUpwardIfResistance;
        float radians = Mathf.Deg2Rad * knockbackAngle;
        GameObject hitObject = hitBoxCollider.gameObject;
        if (healthScript != null)
        {
            healthScript.TakeDamage(baseDamageAmount * (resistanceLowersDamageAmount ? movement.resistanceMultiplier : 1f) + (scalingChargeToApplyVar * scalingDamageMultiplier));
            if (caller != null)
            {
                HapticFeedback.VibrateGamepad(HapticLowFreq, HapticHighFreq, HapticDuration, caller);
            }
        }

        Attacks attacked = hitObject.GetComponentInParent<Attacks>();
        bool ignoreDragOnAttack = false;
        if (attacked != null)
        {
            attacked.hitstuntime = hitStunAmount;
            Debug.Log($"HitStun applied: {hitStunAmount} seconds");
            movement.speed = 0f;
        }

        if (hitObject != null && movement != null && movement.RigBod != null)
        {
            Movement2 _affectedMovement = movement;
            float _storedOriginalBounciness = movement.storedOriginalBounciness;
            if (_affectedMovement.RigBod.sharedMaterial != null) _storedOriginalBounciness = _affectedMovement.RigBod.sharedMaterial.bounciness;
            if (_affectedMovement.RigBod.sharedMaterial == null)
            {
                Debug.LogWarning("Rigidbody2D does not have a sharedMaterial assigned. Assigning a default material.");
                _affectedMovement.RigBod.sharedMaterial = new PhysicsMaterial2D("DefaultMaterial")
                {
                    bounciness = 0f // Set default bounciness
                };
            }
            if (caller != null)
            {
                _directionCheck = caller.GetComponentInParent<Movement2>();
            }
            else
            {
                Debug.LogWarning("Caller is null. Cannot retrieve Movement2 component.");
                _directionCheck = null;
            }
            if (caller != null)
            {
                _strongCharge = caller.GetComponentInParent<StrongCharge>();
                if (_strongCharge != null)
                {
                    _strongCharge.AddCharge(strongChargeAdd);
                }
            }
            if (caller != null && caller.TryGetComponent<HitPause>(out var hitPause))
            {
                hitPause.Stop(hitPauseAmount);
            }

            if (movement.hasSuperArmor)
            {
                ignoreDragOnAttack = false;
                attacked.InvokeOnAttacked(ignoreDragOnAttack);
                if (horizKnockbackIfSuperArmor != 0 || useSuperArmorKnockbackScaling)
                {
                    movement.RigBod.linearVelocity = new Vector2(
                        (horizKnockbackIfSuperArmor + (scalingChargeToApplyVar * superArmorKnockbackScaleRate)) *
                        (useSuperArmorKnockbackScaling ? 1f : 0f) *
                        DirectionDirCheck(_directionCheck),
                        movement.RigBod.linearVelocity.y
                    );
                }
            }
            else if (!movement.hasSuperArmor)
            {
                ignoreDragOnAttack = true;
                attacked.InvokeOnAttacked(ignoreDragOnAttack);
                Vector2 knockbackForce = _directionCheck switch
                {
                    null => new Vector2(
                        ((Mathf.Cos(radians) * baseKnockbackStrengthX) + (scalingChargeToApplyVar * knockbackStrengthScalingX)) * 1 * NormalizedXKnockbackIfNoShieldTest(healthScript, movement, resistanceLowersHorizKnockback, normalizeKnockbackIfNoShield),
                        ((Mathf.Sin(radians) * baseKnockbackStrengthY) + (scalingChargeToApplyVar * knockbackStrengthScalingY)) * LaunchUpwardIfResistanceTest(_storedBoolLaunchUpIfResistance, movement) * NormalizedXKnockbackIfNoShieldTest(healthScript, movement, resistanceLowersVertKnockback, normalizeKnockbackIfNoShield)
                        ),// If direction check is null, use default direction (1)
                    _ => new Vector2(
                        ((Mathf.Cos(radians) * baseKnockbackStrengthX) + (scalingChargeToApplyVar * knockbackStrengthScalingX)) * _directionCheck.Spr_Dir() * NormalizedXKnockbackIfNoShieldTest(healthScript, movement, resistanceLowersHorizKnockback, normalizeKnockbackIfNoShield),
                        ((Mathf.Sin(radians) * baseKnockbackStrengthY) + (scalingChargeToApplyVar * knockbackStrengthScalingY)) * LaunchUpwardIfResistanceTest(_storedBoolLaunchUpIfResistance, movement) * NormalizedXKnockbackIfNoShieldTest(healthScript, movement, resistanceLowersVertKnockback, normalizeKnockbackIfNoShield)
                        ),// If direction check is not null, use its direction
                };

                movement.RigBod.linearVelocity = knockbackForce;
                movement.RigBod.linearDamping = 0f;
            }
            if (isLaunchModifyGravity)
            {
                _affectedMovement.RigBod.gravityScale *= launchGravityMultiplier;
                if (caller != null && caller.isActiveAndEnabled)
                {
                    caller.StartCoroutine(ReturnGravityRoutine(_affectedMovement, gravityReturnDelay, launchGravityMultiplier));
                }
                else
                {
                    Debug.LogWarning("Cannot start ReturnGravityRoutine: caller is null, inactive, or disabled.");
                }
                if (bounciness > 0f)
                {
                    if (_affectedMovement.RigBod.sharedMaterial == null)
                    {
                        Debug.LogWarning("Rigidbody2D does not have a sharedMaterial assigned. Assigning a default material.");
                        _affectedMovement.RigBod.sharedMaterial = new PhysicsMaterial2D("DefaultMaterial")
                        {
                            bounciness = 0 // Set default bounciness
                        };
                    }
                    if (bouncinessRoutine != null && caller != null && caller.isActiveAndEnabled)
                    {
                        if (caller.IsInvoking("StopCoroutine"))
                        caller.StopCoroutine(bouncinessRoutine);
                    }
                    if (caller != null && caller.isActiveAndEnabled)
                    {
                        var routine = SetAndResetBouncinessRoutine(_affectedMovement, bounciness, gravityReturnDelay, _storedOriginalBounciness);
                        if (routine != null)
                        {
                            bouncinessRoutine = bounciness > 0f ? caller.StartCoroutine(routine) : null;
                        }
                        else
                        {
                            Debug.LogError("SetAndResetBouncinessRoutine returned null. Coroutine not started.");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Cannot start SetAndResetBouncinessRoutine: caller is null, inactive, or disabled.");
                    }
                }
                else
                {
                    ReturnBounciness(_affectedMovement, _storedOriginalBounciness);
                }
            }
            else
            {
                return;
            }
        }
    }
    private static IEnumerator ReturnGravityRoutine(Movement2 movement, float gravityReturnDelay, float launchGravityMultiplier)
    {
        yield return new WaitForSeconds(gravityReturnDelay);
        ReturnGravity(movement, launchGravityMultiplier);
    }
    private static IEnumerator SetAndResetBouncinessRoutine(Movement2 movement, float newBounciness, float returnDelay, float _storedOriginalBounciness)
    {
        movement.RigBod.sharedMaterial.bounciness = newBounciness;
        yield return new WaitForSeconds(returnDelay);
        ReturnBounciness(movement, _storedOriginalBounciness);
    }

    private static void ReturnBounciness(Movement2 movement, float _storedOriginalBounciness)
    {
        if (movement != null && movement.RigBod != null)
        {
            movement.RigBod.sharedMaterial.bounciness = _storedOriginalBounciness;
        }
    }

    private static int LaunchUpwardIfResistanceTest(bool _storedBoolLaunchUpIfResistance, Movement2 movement)
    {
        if (movement.resistanceRaw > 0)
        {
            return _storedBoolLaunchUpIfResistance ? 1 : 0;
        }
        else
        {
            return 1;
        }
    }
    private static int DirectionDirCheck(Movement2 _directionCheck)
    {
        if (_directionCheck != null)
        {
            return _directionCheck.Spr_Dir();
        }
        else
        {
            return 1;
        }
    }

    private static float NormalizedXKnockbackIfNoShieldTest(Health healthscript, Movement2 movement, bool knockbackAxis, bool choice)
    {
        if (choice && healthscript.shield > 0)
        {
            Debug.Log((knockbackAxis ? movement.resistanceMultiplier : 1f));
            return (knockbackAxis ? movement.resistanceMultiplier : 1f);
        }
        else if (choice && healthscript.shield <= 0.01f)
        {
            Debug.Log("1f");
            return 1f;
        }
        else
        {
            Debug.Log((knockbackAxis ? movement.resistanceMultiplier : 1f));
            return knockbackAxis ? movement.resistanceMultiplier : 1f;
        }
    }


    public static void ReturnGravity(Movement2 _affectedMovement, float _storedGrav)
    {
        if (_affectedMovement != null && _affectedMovement.RigBod != null)
        {
            _affectedMovement.RigBod.gravityScale /= _storedGrav;
        }
    }
}
