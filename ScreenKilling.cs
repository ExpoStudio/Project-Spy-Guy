using UnityEngine;
public class ScreenKilling : MonoBehaviour
{
    [SerializeField] private Collider2D[] ScreenKillingColliders = new Collider2D[4];
    // Start is called once before the first execution of Update after the MonoBehaviour is created 
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"OnTriggerEnter2D called with collider: {other.name}, tag: {tag}");
        if (other.CompareTag("Player"))
        {
            Debug.Log("CompareTag('Player') passed, processing hit...");
            Transform hitObject = other.transform.GetChild(0).transform;
            Debug.Log($"hitObject: {hitObject.name}, position: {hitObject.position}");
            Health healthscript = hitObject.GetComponentInParent<Health>();
            Movement2 movement = hitObject.GetComponentInParent<Movement2>();
            Transform TargetPlayer = CameraPositionController.Instance.PrioritizedPlayerInput.transform.GetChild(0).transform;
            Vector2 LaunchVector = CameraPositionController.Instance.PrioritizedPlayerInput != null
                ? new Vector2
                    (
                        TargetPlayer.transform.position.x - hitObject.position.x,
                        TargetPlayer.transform.position.y - hitObject.position.y
                    )
                : new Vector2
                    (
                        CameraPositionController.Instance.transform.position.x - hitObject.position.x,
                        CameraPositionController.Instance.transform.position.y - hitObject.position.y
                    );
            Debug.Log($"LaunchVector calculated: {LaunchVector}");
            Debug.DrawRay(hitObject.position, LaunchVector, Color.red, 3f);
            int None = 0;
            Vector2 launchPower = LaunchVector.FindPowerVectorFromDistanceOverflow();
            Debug.Log($"Power Vector: {launchPower}");
            Debug.DrawRay(hitObject.position, launchPower, Color.cyan, 3f);
            float angle = LaunchVector.FindAngleFromVector2();
            Debug.Log($"angle: {angle}");
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
                20f,
                //resistanceLowersDamageAmount = 
                false,
                //resistanceLowersHorizKnockback = 
                false,
                //doesLaunchUpwardIfResistance = 
                true,
                //resistanceLowersVertKnockback = 
                false,
                //horizKnockbackIfSuperArmor = 
                Mathf.Abs(launchPower.x),
                //useSuperArmorKnockbackScaling = 
                false,
                //superArmorKnockbackScaleRate = 
                None,
                //strongChargeAdd = 
                None,

                //-Knockback, Scaling and Hit Pause-
                //scalingChargeToApplyVar = 
                None,
                //scalingDamageMultiplier = 
                None,
                //knockbackAngle = 
                angle,
                //baseKnockbackStrengthX = 
                Mathf.Abs(launchPower.x),
                //baseKnockbackStrengthY = 
                Mathf.Abs(launchPower.y),
                //knockbackStrengthScalingX = 
                1.5f,
                //knockbackStrengthScalingY = 
                1.5f,
                //hitPauseAmount = 
                1f,

                //-Gravity Modifiers-
                //isLaunchModifyGravity = 
                true,
                //launchGravityMultiplier = 
                0.5f,
                //GravityReturnDelay = 
                1f,

                //-Haptic Feedback-
                //HapticLowFreq = 
                0.1f,
                //HapticHighFreq = 
                0.1f,
                //HapticDuration = 
                0.1f,

                //-NewImplementations-
                //NormalizeKNockbackIfNoShield =
                movement.normalizeKnockbackIfNoShield,
                //HitStun Amount (In seconds)
                1f,
                //Hit Entity Bounciness // typically for richochet attacks (0-1)
                None
            );
            CoolEffects.SlowDownTime(this, 0.1f, 0.5f);
            Debug.Log("HitBox.CreateDamageHitbox called.");
        }
        else
        {
            Debug.Log("CompareTag('Player') failed, returning.");
            return;
        }
    }
    
    private void OnTriggerStay2D(Collider2D other)
    {
        Debug.Log($"OnTriggerEnter2D called with collider: {other.name}, tag: {tag}");
        if (other.CompareTag("Player"))
        {
            Debug.Log("CompareTag('Player') passed, processing hit...");
            Transform hitObject = other.transform.GetChild(0).transform;
            Debug.Log($"hitObject: {hitObject.name}, position: {hitObject.position}");
            Health healthscript = hitObject.GetComponentInParent<Health>();
            Movement2 movement = hitObject.GetComponentInParent<Movement2>();
            Transform TargetPlayer = CameraPositionController.Instance.PrioritizedPlayerInput.transform.GetChild(0).transform;
            Vector2 LaunchVector = CameraPositionController.Instance.PrioritizedPlayerInput != null
                ? new Vector2
                    (
                        TargetPlayer.transform.position.x - hitObject.position.x,
                        TargetPlayer.transform.position.y - hitObject.position.y
                    )
                : new Vector2
                    (
                        CameraPositionController.Instance.transform.position.x - hitObject.position.x,
                        CameraPositionController.Instance.transform.position.y - hitObject.position.y
                    );
            Debug.Log($"LaunchVector calculated: {LaunchVector}");
            Debug.DrawRay(hitObject.position, LaunchVector, Color.red, 3f);
            int None = 0;
            Vector2 launchPower = LaunchVector.FindPowerVectorFromDistanceOverflow();
            Debug.Log($"Power Vector: {launchPower}");
            Debug.DrawRay(hitObject.position, launchPower, Color.cyan, 3f);
            float angle = LaunchVector.FindAngleFromVector2();
            Debug.Log($"angle: {angle}");
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
                0.5f,
                //resistanceLowersDamageAmount = 
                false,
                //resistanceLowersHorizKnockback = 
                false,
                //doesLaunchUpwardIfResistance = 
                true,
                //resistanceLowersVertKnockback = 
                false,
                //horizKnockbackIfSuperArmor = 
                Mathf.Abs(launchPower.x),
                //useSuperArmorKnockbackScaling = 
                false,
                //superArmorKnockbackScaleRate = 
                None,
                //strongChargeAdd = 
                None,

                //-Knockback, Scaling and Hit Pause-
                //scalingChargeToApplyVar = 
                None,
                //scalingDamageMultiplier = 
                None,
                //knockbackAngle = 
                angle,
                //baseKnockbackStrengthX = 
                Mathf.Abs(launchPower.x),
                //baseKnockbackStrengthY = 
                Mathf.Abs(launchPower.y),
                //knockbackStrengthScalingX = 
                1.5f,
                //knockbackStrengthScalingY = 
                1.5f,
                //hitPauseAmount = 
                1f,

                //-Gravity Modifiers-
                //isLaunchModifyGravity = 
                true,
                //launchGravityMultiplier = 
                0.5f,
                //GravityReturnDelay = 
                1f,

                //-Haptic Feedback-
                //HapticLowFreq = 
                0f,
                //HapticHighFreq = 
                0f,
                //HapticDuration = 
                0f,

                //-NewImplementations-
                //NormalizeKNockbackIfNoShield =
                movement.normalizeKnockbackIfNoShield,
                //HitStun Amount (In seconds)
                1f,
                //Hit Entity Bounciness // typically for richochet attacks (0-1)
                None
            );
            Debug.Log("HitBox.CreateDamageHitbox called.");
        }
        else
        {
            Debug.Log("CompareTag('Player') failed, returning.");
            return;
        }
    }
}


