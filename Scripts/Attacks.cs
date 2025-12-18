using System;
using System.Collections;
using UnityEngine;
public enum JabPhase
{
    None,
    FirstHit,
    SecondHit,
    LaunchHit
}

public enum StrongPhase
{
    None,
    Charging,
    releaseReady,
    Canceled,
    Released
}

public enum AttackMove
{
    None,
    Jab,
    strongGround,
    DashBash,
    Dive,
    JumperCut,
    Defensive
}

public enum DiveState
{
    Diving,
    Landed
}

public class Attacks : MonoBehaviour
{
    public Movement2 Inputs;
    [SerializeField] private WallJumping wallJumping;

    #region HitBox Definitions
    public Collider2D JabHitbox;
    public Collider2D JabLaunchbox;
    public Collider2D JabHitboxAlt1;
    public Collider2D StrongFist;
    public Collider2D StrongFistUp;
    public Collider2D StrongFistDown;
    public Collider2D StrongZoomBox;
    public Collider2D DiveHitbox;
    public Collider2D DiveBombHitbox;
    public Collider2D DiveBombLandHitbox;
    public Collider2D JumperCutHitbox;
    public Collider2D JumperCutWeakSpot;

    public bool phaseDone = false;
    
    //Player Collider Hurboxes
    public Collider2D HurtBox;
    public Collider2D PlayerCollider;
    public Collider2D DiveHurtbox;
    public Collider2D DiveCollider;
    #endregion

    public bool canattack;
    public bool wasattacked;
    public bool isattacking;
    public float AttkHoldTime;

    public bool releaseReady = false;
    public bool isCharging = false;

    public float chargeSubtract;
    public float StoredCharged;
    public float ScalingChargeToApply;
    public bool storedAlready;

    [SerializeField] private float _triggerTime = 0;

    [SerializeField] private StrongCharge strongCharge;
    public float hitstuntime;
    public bool inHitstun = false;
    public float AttkTimer;
    [SerializeField] private float blockHoldTime;

    #region Enumerator Definitions
    public AttackMove attackMove = AttackMove.None;
    public JabPhase jabPhase = JabPhase.None;
    public StrongPhase strongPhase = StrongPhase.None;
    public DiveState DiveState = DiveState.Diving;
    [HideInInspector] public float StickDir;

    //Attack Bools
    private bool startedDiving;
    private bool isDiveBombing;
    public bool diveBombLand = false;
    public bool didUpperCut = false;
    public bool isUpperCutting = false;

    //Coroutines
    public Coroutine DiveRoutine;
    public Coroutine HitstunRoutine;



    [SerializeField] private float canUndoTime;
    public float StoredVelocity { get; private set;}
    [SerializeField] private bool candive = true;
    public bool canblock = true;

    public event Action OnAttacked;


    //Hit Detector Flags
    public bool didAttackLand = false;
    public bool ignoreDragOnAttack = false;

    #endregion


    // Update is called once per frame
    void Start()
    {
        //FistColliders
        JabHitbox.enabled = false;
        JabLaunchbox.enabled = false;
        JabHitboxAlt1.enabled = false;

        StrongFist.enabled = false;
        StrongFistUp.enabled = false;
        StrongFistDown.enabled = false;
        StrongZoomBox.enabled = false;

        JumperCutHitbox.enabled = false;
        JumperCutWeakSpot.enabled = false;
        //End

        //DiveColliders
        DiveHurtbox.enabled = false;
        DiveHitbox.enabled = false;
        DiveBombHitbox.enabled = false;
        DiveBombLandHitbox.enabled = false;
        DiveCollider.enabled = false;
        //End

        canattack = true;
        candive = true;

        // Subscribe to its own OnAttacked event to handle scenarios where the player is attacked.
        // This event triggers logic to update the player's state, such as entering hitstun, stopping ongoing attack routines,
        // and resetting relevant flags and timers to ensure proper gameplay behavior after being attacked.
        OnAttacked += () =>
        {
            wasattacked = true;
            Inputs.currAttackState = AttackState.notAttacking;
            inHitstun = true;
            if (HitstunRoutine != null)
            {
                StopCoroutine(HitstunRoutine);
                HitstunRoutine = null;
            }
            HitstunRoutine = StartCoroutine(Hitstun(hitstuntime));
            Debug.Log("Player was attacked, entering hitstun.");
        };
    }
    public virtual void Update()
    {
        if (AttkTimer > 0)
        {
            float TimeSub = Mathf.Min(AttkTimer, 10f * Time.deltaTime);
            AttkTimer -= TimeSub;
        }
        AttkTimer = Mathf.Clamp(AttkTimer, 0f, 2000f);

        #region Jab Attack Logic
        if (attackMove == AttackMove.Jab && AttkTimer < 6f && !isCharging)
        {
            attackMove = AttackMove.None;
            EnableMovement();
        }
        if (attackMove == AttackMove.None)
        {
            Inputs.currAttackState = AttackState.notAttacking;
        }
        #endregion

        if (isattacking || wasattacked)
        {
            return;
        }

        AttkHoldTime = Mathf.Clamp(AttkHoldTime, 0f, 30f);
        blockHoldTime = Mathf.Clamp(blockHoldTime, 0f, 30f);
        _triggerTime = Mathf.Clamp(_triggerTime, 0f, 30f);
        if (Inputs.Attack() && AttkHoldTime < 30) AttkHoldTime += 0.6f * Time.deltaTime;
        if (!Inputs.Attack()) AttkHoldTime = 0;
        if (Inputs.Block() && blockHoldTime < 30) blockHoldTime += 0.6f * Time.deltaTime;
        if (!Inputs.Block()) blockHoldTime = 0;
        
        if (attackMove == AttackMove.JumperCut) _triggerTime += 5f * Time.deltaTime;
        else _triggerTime = 0;
    }
    public virtual void FixedUpdate()
    {
        switch (attackMove)
        {
            case AttackMove.Jab:
                float horizontalInput = Inputs.inputHandler.MoveInput.x;
                if (Mathf.Abs(horizontalInput) < 0.5f) horizontalInput = 0;
                if (horizontalInput > 0.5f) horizontalInput = 1;
                if (horizontalInput < -0.5f) horizontalInput = -1; // Add threshold check
                Inputs.currAttackState = AttackState.Attacking;
                switch (jabPhase)
                {
                    case JabPhase.FirstHit:
                        if (Inputs.IsGrounded() && AttkHoldTime > 0 && AttkHoldTime < 1f && AttkTimer >= 7 && AttkTimer < 13f && !Inputs.runningState)
                        {
                            PerformJab();
                        }
                        break;
                    case JabPhase.SecondHit:
                        if (Inputs.IsGrounded() && AttkHoldTime > 0 && AttkHoldTime < 1f && AttkTimer >= 7 && AttkTimer < 13f && horizontalInput == Inputs.Spr_Dir())
                        {
                            PerformJabAlt();
                            RestrictInput();
                        }
                        else if (Inputs.IsGrounded() && AttkHoldTime > 0 && AttkHoldTime < 1f && AttkTimer >= 7 && AttkTimer < 13f && horizontalInput == 0 && Inputs.vert == 0)
                        {
                            PerformJab();
                        }
                        break;
                    case JabPhase.LaunchHit:
                        if (Inputs.IsGrounded() && AttkHoldTime > 0 && AttkHoldTime < 1f && AttkTimer >= 7 && AttkTimer < 13f && !Inputs.runningState)
                        {
                            PerformJabLaunch();
                        }
                        break;
                }
                break;
            case AttackMove.strongGround:
                switch (strongPhase)
                {
                    case StrongPhase.Charging:
                        DeIncrememntCharge();
                        RestrictInput();
                        Inputs.currAttackState = AttackState.Attacking;
                        HapticHold.StartVibration(0f, Mathf.Lerp(1f, 3f, 1));
                        Invoke(nameof(ReadyToAttack), 0.5f);
                        DeIncrememntCharge();
                        if (!Inputs.Strong() && !Inputs.StrongAxis())
                        {
                            strongPhase = StrongPhase.Canceled;
                        }
                        break;
                    case StrongPhase.releaseReady:
                        HapticHold.StartVibration(0f, 0.1f);
                        EnableMovement();
                        DeIncrememntCharge();
                        if (Inputs.Block())
                        {
                            strongPhase = StrongPhase.Canceled;
                        }
                        else if (!Inputs.Strong() && !Inputs.StrongAxis())
                        {
                            strongPhase = StrongPhase.Released;
                        }
                        break;
                    case StrongPhase.Canceled:
                        strongCharge.strongCharge += StoredCharged;
                        HapticHold.StopVibration();
                        ResetCharge();
                        releaseReady = false;
                        CancelInvoke(nameof(ReadyToAttack));
                        isCharging = false;
                        canattack = false;
                        Invoke(nameof(EnableMovement), 0.5f);
                        strongPhase = StrongPhase.None;
                        AttkTimer += 14f;
                        break;
                    case StrongPhase.Released:
                        Inputs.currAttackState = AttackState.Attacking;
                        HapticHold.StopVibration();
                        RestrictInput();
                        float verticalInput = Inputs.vert;
                        ScalingChargeToApply = StoredCharged;
                        if (Mathf.Abs(verticalInput) < 0.5f) verticalInput = 0; // Add threshold check
                        if (verticalInput >= 0.5f) verticalInput = 1;
                        if (verticalInput <= -0.5f) verticalInput = -1;
                        // Debug logs to check vertical input and attack logic

                        switch (StoredCharged)
                        {
                            case > 50f:
                                isattacking = true;
                                zoomedAlready = false;
                                if (verticalInput == 1)
                                {
                                    StrongHitFistUp();
                                    StrongZoomHitBox();
                                }
                                else if (verticalInput == -1)
                                {
                                    StrongHitFistDown();
                                    StrongZoomHitBox();
                                }
                                else
                                {
                                    StrongHitFist();
                                    StrongZoomHitBox();
                                }
                                releaseReady = false;
                                break;
                            default:
                                isattacking = true;
                                if (verticalInput == 1)
                                {
                                    StrongHitFistUp();
                                }
                                else if (verticalInput == -1)
                                {
                                    StrongHitFistDown();
                                }
                                else
                                {
                                    StrongHitFist();
                                }
                                releaseReady = false;
                                break;
                        }
                        break;
                    default:
                        attackMove = AttackMove.None;
                        break;
                }
                break;
            case AttackMove.Dive:
                switch (DiveState)
                {
                    case DiveState.Diving:
                        if (!startedDiving && !isDiveBombing)
                        {
                            canUndoTime = 0f;
                            Inputs.currAttackState = AttackState.Attacking;
                            Inputs.RigBod.linearVelocity = Vector2.zero;
                            Inputs.RigBod.AddForce(new Vector2(14f * Inputs.Spr_Dir(), 5f), ForceMode2D.Impulse);
                            startedDiving = true;
                            DiveHitbox.enabled = true;
                            DiveHurtbox.enabled = true;
                            DiveCollider.enabled = true;
                            PlayerCollider.enabled = false;
                            HurtBox.enabled = false;
                            StoredVelocity = 0;
                            //Note, add player getting knockbed back when diving into something with super armor
                        }
                        if (startedDiving && !isDiveBombing && canUndoTime > 6f && canUndoTime < 30f && blockHoldTime < 10f && blockHoldTime > 0f && Inputs.inputHandler.MoveInput.y < 0f && AttkTimer < 5f)
                        {
                            PerformDiveBomb();
                        }
                        else if (startedDiving && isDiveBombing && !Inputs.IsGrounded())
                            StoredVelocity = Inputs.RigBod.linearVelocityY;

                        canUndoTime = Mathf.Clamp(canUndoTime, 0f, 30f);
                        if (canUndoTime < 30) canUndoTime += 0.8f;
                        if (Inputs.IsGrounded() && canUndoTime > 10f)
                        {
                            DiveState = DiveState.Landed;
                        }
                        break;
                    case DiveState.Landed:
                        DiveRoutine ??= StartCoroutine(UndoDiveDelay(StoredVelocity));
                        canUndoTime = 0f;
                        break;
                }
                break;
            case AttackMove.JumperCut:
                if (!didUpperCut && !isUpperCutting && _triggerTime < 2f && _triggerTime > 0)
                {
                    PerformJumperCutInitialState();
                    isUpperCutting = true;
                    Inputs.ignoreDefaultPhysics = true;
                }
                else if (isUpperCutting && !didUpperCut && _triggerTime >= 2f && _triggerTime < 7f)
                {
                    didUpperCut = true;
                    Inputs.currAttackState = AttackState.notAttacking;
                    Inputs.ignoreDefaultPhysics = true;
                    JumperCutRemainingState();
                }
                else if ((AttkTimer < 8f || Inputs.IsGrounded()) && didUpperCut && isUpperCutting)
                {
                    Inputs.currAttackState = AttackState.notAttacking;
                    EnableMovement();
                    AttkTimer = 6f;
                    isUpperCutting = false;
                    didUpperCut = false;
                    didAttackLand = false;
                    attackMove = AttackMove.None;
                    Inputs.ignoreDefaultPhysics = false;
                }
                else if (!Inputs.IsGrounded() && didUpperCut && isUpperCutting && _triggerTime >= 9f)
                {
                    if (blockHoldTime < 10f && blockHoldTime > 0f && Inputs.inputHandler.MoveInput.y < 0f)
                    {
                        AttkTimer = 4f;
                        startedDiving = true;
                        canUndoTime = 20f;
                        isUpperCutting = false;
                        didUpperCut = false;
                        PerformDiveBomb();
                        attackMove = AttackMove.Dive;
                        didAttackLand = false;
                        Inputs.ignoreDefaultPhysics = false;
                    }
                    else if ((Inputs.StrongAxis() || Inputs.Strong()) && strongCharge.strongCharge > 0f)
                    {
                        attackMove = AttackMove.strongGround;
                        strongPhase = StrongPhase.Charging;
                        isUpperCutting = false;
                        didUpperCut = false;
                        didAttackLand = false;
                        Inputs.ignoreDefaultPhysics = false;
                        return;
                    }
                    else if (Inputs.Block() && (Inputs.runningState || Inputs.HoldRun()) && blockHoldTime > 0 && blockHoldTime < 3f && candive)
                    {
                        attackMove = AttackMove.Dive;
                        DiveState = DiveState.Diving;
                        candive = false;
                        isUpperCutting = false;
                        didUpperCut = false;
                        didAttackLand = false;
                        Inputs.ignoreDefaultPhysics = false;
                        return;
                    }
                }
                break;
            default:
                if (JabDetected)
                {
                    RestrictInput();
                    JabHitbox.enabled = true;
                    Inputs.speed = 0;
                    attackMove = AttackMove.Jab;
                    jabPhase = JabPhase.FirstHit;
                    Inputs.RigBod.linearVelocity = Vector2.zero;
                    Invoke(nameof(EnableMovement), 0.3f);
                    Invoke(nameof(DisableJab), 0.2f);
                    AttkTimer = 14f;
                    return;
                }
                else if (StrongDetected)
                {
                    attackMove = AttackMove.strongGround;
                    strongPhase = StrongPhase.Charging;
                    return;
                }
                else if (DiveDetected)
                {
                    attackMove = AttackMove.Dive;
                    DiveState = DiveState.Diving;
                    candive = false;
                    return;
                }
                else if (JumperCutDetected)
                {
                    attackMove = AttackMove.JumperCut;
                    didUpperCut = false;
                    isUpperCutting = false;
                    return;
                }
                break;
        }
    }

    private bool JumperCutDetected => Inputs.vert > 0 && !Inputs.runningState && !Inputs.HoldRun() && Inputs.IsGrounded() && AttkHoldTime > 0 && AttkHoldTime < 4f && AttkTimer < 0.6f && !didUpperCut && !isUpperCutting;
    private bool DiveDetected => Inputs.Block() && Inputs.HoldRun() && blockHoldTime > 0 && blockHoldTime < 3f && candive && AttkTimer < 3f && Inputs.horiz != 0;
    private bool StrongDetected => (Inputs.StrongAxis() || Inputs.Strong()) && AttkTimer < 4f && strongCharge.strongCharge > 0f;
    private bool JabDetected => Inputs.IsGrounded() && AttkHoldTime > 0 && AttkHoldTime < 2f && AttkTimer == 0 && !Inputs.runningState && Inputs.speed < 2f && Inputs.horiz == 0 && Inputs.vert == 0;

    /// <summary>
    /// Flag to avoid zooming or triggering if more than one thing is hit with the zoombox
    /// </summary>
    public bool zoomedAlready;
    public bool zoomBoxHit = false;
    private void PerformDiveBomb()
    {
        Inputs.RigBod.linearVelocity = Vector2.zero;
        Inputs.RigBod.AddForce(new Vector2(2f * Inputs.Spr_Dir(), -10f), ForceMode2D.Impulse);
        Inputs.RigBod.gravityScale = 3f;
        DiveHitbox.enabled = false;
        DiveHurtbox.enabled = false;
        DiveCollider.enabled = false;
        PlayerCollider.enabled = true;
        HurtBox.enabled = true;
        DiveBombHitbox.enabled = true;
        startedDiving = false;
        isDiveBombing = true;
    }

    private IEnumerator UndoDiveDelay(float StoredVelocity)
    {
        Debug.Log("Stored Velocity: " + StoredVelocity);
        DiveBombHitbox.enabled = false;
        HurtBox.enabled = true;
        Inputs.canjump = false;
        Inputs.canmove = false;
        if (isDiveBombing)
        {
            Inputs.currAttackState = AttackState.Attacking;
            diveBombLand = true;
            PerformDiveBombLand();
            Inputs.RigBod.linearVelocity = new Vector2(0,0);
            RestrictInput();
            DiveHitbox.enabled = false;
            DiveHurtbox.enabled = false;
        }
        yield return new WaitForSeconds(0.5f);
        DiveCollider.enabled = false;
        PlayerCollider.enabled = true;
        DiveHitbox.enabled = false;
        DiveHurtbox.enabled = false;
        Inputs.currAttackState = AttackState.notAttacking;
        isDiveBombing = false;
        DiveBombHitbox.enabled = false;
        attackMove = AttackMove.None;
        DiveState = DiveState.Diving;
        diveBombLand = false;
        startedDiving = false;
        candive = true;
        Inputs.canjump = true;
        Inputs.canmove = true;
        Debug.Log("Is Set True and Reached");
        DiveRoutine = null;
        Inputs.RigBod.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void PerformJab()
    {
        RestrictInput();
        JabHitbox.enabled = true;
        Inputs.speed = 0;
        Inputs.RigBod.linearVelocity = Vector2.zero;
        CancelInvoke(nameof(EnableMovement));
        Invoke(nameof(EnableMovement), 0.5f);
        Invoke(nameof(DisableJab), 0.2f);
        AttkTimer = 15f;

        if (jabPhase == JabPhase.FirstHit && phaseDone)
        {
            jabPhase = JabPhase.SecondHit;
            phaseDone = false;
        }
        else if (jabPhase == JabPhase.SecondHit && phaseDone)
        {
            jabPhase = JabPhase.LaunchHit;
            phaseDone = false;
        }
        else if (jabPhase == JabPhase.LaunchHit && phaseDone)
        {
            jabPhase = JabPhase.LaunchHit;
            phaseDone = false;
        }
    }
    private void PerformJabLaunch()
    {
        RestrictInput();
        JabLaunchbox.enabled = true;
        Inputs.speed = 0;
        Inputs.RigBod.linearVelocity = new Vector2(2 * Inputs.Spr_Dir(), 0);
        CancelInvoke(nameof(EnableMovement));
        Invoke(nameof(EnableMovement), 0.5f);
        Invoke(nameof(DisableLaunch), 0.3f);
        AttkTimer = 14f;
        if (phaseDone)
        {
            attackMove = AttackMove.None;
            jabPhase = JabPhase.None;
        }
    }
    private void PerformJabAlt()
    {
        RestrictInput();
        JabHitboxAlt1.enabled = true;
        Inputs.speed = 0;
        Inputs.RigBod.linearVelocity = Vector2.zero;
        CancelInvoke(nameof(EnableMovement));
        Invoke(nameof(EnableMovement), 0.5f);
        Invoke(nameof(DisableJab), 0.1f);
        AttkTimer = 14f;
        if (phaseDone)
        {
            attackMove = AttackMove.None;
            jabPhase = JabPhase.None;
        }
    }

    private void PerformJumperCutInitialState()
    {
        StrongZoomBox.enabled = true;
        JumperCutHitbox.enabled = true;
        zoomedAlready = true;
        Inputs.RigBod.linearVelocity = Vector2.zero;
        AttkTimer = 30f;
        isUpperCutting = true;
    }

    private void JumperCutRemainingState()
    {
        isUpperCutting = true;

        // Reset vertical velocity to prevent compounding forces
        Inputs.RigBod.linearVelocity = new Vector2(Inputs.RigBod.linearVelocity.x, 0f);

        // Apply upward and horizontal force
        float upwardForce = 14f; // Reduced from 14f to a more reasonable value
        float horizontalForce = 2f * Inputs.Spr_Dir();
        Inputs.RigBod.AddForce(new Vector2(horizontalForce, upwardForce), ForceMode2D.Impulse);

        // Clamp the upward velocity to prevent excessive speed
        float maxUpwardVelocity = 14f; // Maximum allowed upward velocity
        if (Inputs.RigBod.linearVelocity.y > maxUpwardVelocity)
        {
            Inputs.RigBod.linearVelocity = new Vector2(Inputs.RigBod.linearVelocity.x, maxUpwardVelocity);
        }

        // Adjust gravity and damping
        Inputs.RigBod.linearDamping = 0f;
        Inputs.RigBod.gravityScale = 1.2f;

        // Enable and disable hitboxes
        StrongZoomBox.enabled = false;
        JumperCutHitbox.enabled = false;
        JumperCutWeakSpot.enabled = true;

        // Schedule disabling the JumperCut
        Invoke(nameof(DisableJumperCut), 1f);
    }

    private void PerformDiveBombLand()
    {
        DiveBombLandHitbox.enabled = true;
        Inputs.speed = 0;
        Inputs.RigBod.linearVelocity = Vector2.zero;
        Invoke(nameof(DisableDive), 0.3f);
        AttkTimer = 14f;
        CameraShake.TriggerUnscaledShake(0.1f, 0.05f, 0.4f);
    }
    private void DisableDive()
    {
        DiveBombHitbox.enabled = false;
        DiveBombLandHitbox.enabled = false;
    }

    #region StrongHitFist
    private void StrongHitFist()
    {
        RestrictInput();
        StrongFist.enabled = true;
        Inputs.speed = 0;
        Inputs.RigBod.linearVelocity = Vector2.zero;
        CancelInvoke(nameof(EnableMovement));
        Invoke(nameof(EnableMovement), 0.7f);
        Invoke(nameof(DisableStrong), 0.4f);
        AttkTimer = 14f;
        attackMove = AttackMove.None;
        Invoke(nameof(ResetCharge),0.1f);
        releaseReady = false;
        storedAlready = false;
    }
    private void StrongHitFistUp()
    {
        RestrictInput();
        StrongFistUp.enabled = true;
        Inputs.speed = 0;
        Inputs.RigBod.linearVelocity = Vector2.zero;
        CancelInvoke(nameof(EnableMovement));
        Invoke(nameof(EnableMovement), 0.6f);
        Invoke(nameof(DisableStrong), 0.4f);
        AttkTimer = 14f;
        attackMove = AttackMove.None;
        attackMove = AttackMove.None;
        Invoke(nameof(ResetCharge),0.1f);
        releaseReady = false;
        storedAlready = false;
    }
    private void StrongHitFistDown()
    {
        RestrictInput();
        StrongFistDown.enabled = true;
        Inputs.speed = 0;
        Inputs.RigBod.linearVelocity = Vector2.zero;
        CancelInvoke(nameof(EnableMovement));
        Invoke(nameof(EnableMovement), 0.6f);
        Invoke(nameof(DisableStrong), 0.4f);
        AttkTimer = 14f;
        attackMove = AttackMove.None;
        attackMove = AttackMove.None;
        Invoke(nameof(ResetCharge),0.1f);
        releaseReady = false;
        storedAlready = false;
    }
    private void StrongZoomHitBox()
    {
        RestrictInput();
        StrongZoomBox.enabled = true;
        Inputs.speed = 0;
        Inputs.RigBod.linearVelocity = Vector2.zero;
        CancelInvoke(nameof(EnableMovement));
        Invoke(nameof(EnableMovement), 0.6f);
        Invoke(nameof(DisableStrong), 0.4f);
        AttkTimer = 14f;
        attackMove = AttackMove.None;
        jabPhase = JabPhase.None;
    }
    void ReadyToAttack()
    {
        releaseReady = true;
        strongPhase = StrongPhase.releaseReady;
        HapticHold.StopVibration();
    }
    #endregion 

    private void DisableJumperCut()
    {
        JumperCutHitbox.enabled = false;
        JumperCutWeakSpot.enabled = false;
        didUpperCut = true;
    }
    private void DisableJab()
    {
        JabHitbox.enabled = false;
        JabHitboxAlt1.enabled = false;
        phaseDone = true;
    }
    private void DisableLaunch()
    {
        JabLaunchbox.enabled = false;
        phaseDone = true;
    }
    private void DisableStrong()
    {
        StrongFist.enabled = false;
        StrongFistUp.enabled = false;
        StrongFistDown.enabled = false;
        StrongZoomBox.enabled = false;
        releaseReady = false;
        strongPhase = StrongPhase.None;
    }

    private void DeIncrememntCharge()
    {
        chargeSubtract = Mathf.Min(strongCharge.strongCharge, 12f * Time.deltaTime);
        StoredCharged += chargeSubtract; 
        strongCharge.strongCharge -= chargeSubtract;
    }
    private void ResetCharge()
    {
        StoredCharged = 0;
    }

    private void RestrictInput()
    {
        Inputs.RigBod.constraints = RigidbodyConstraints2D.FreezeAll;
        Inputs.canmove = false;
        Inputs.canjump = false;
    }
    private void EnableMovement()
    {
        Inputs.RigBod.constraints = RigidbodyConstraints2D.FreezeRotation;
        Inputs.speed = 0;
        Inputs.RigBod.linearVelocity = Vector2.zero;
        isattacking = false;
        Inputs.currAttackState = AttackState.notAttacking;
        Inputs.canmove = true;
        Inputs.canjump = true;
        canattack = true;
        phaseDone = false;
    }

    private void EnableCanAttackMovement()
    {
        Inputs.RigBod.constraints = RigidbodyConstraints2D.FreezeRotation;
        isattacking = false;
        Inputs.currAttackState = AttackState.AttackingCanMove;
        Inputs.canmove = true;
        Inputs.canjump = true;
    }

    public IEnumerator Hitstun(float delay)
    {
        Inputs.turning = false;
        Inputs.skidTimer = 20;
        Inputs.canjump = false;
        canattack = false;
        candive = false;
        Inputs.canmove = false;
        wallJumping.ignoreJumpInput = true;
        wasattacked = true;
        attackMove = AttackMove.None;

        // Fully interrupt dive state
        DiveHitbox.enabled = false;
        DiveHurtbox.enabled = false;
        DiveBombHitbox.enabled = false;
        DiveBombLandHitbox.enabled = false;
        DiveCollider.enabled = false;
        HurtBox.enabled = true;
        PlayerCollider.enabled = true;
        isDiveBombing = false;
        startedDiving = false;
        diveBombLand = false;
        DiveState = DiveState.Diving;
        if (DiveRoutine != null)
        {
            StopCoroutine(DiveRoutine);
            DiveRoutine = null;
        }

        // Interrupt JumperCut state
        JumperCutHitbox.enabled = false;
        JumperCutWeakSpot.enabled = false;
        isUpperCutting = false;
        didUpperCut = false;

        // Reset velocity and physics
        Inputs.RigBod.constraints = RigidbodyConstraints2D.FreezeRotation;
        Inputs.RigBod.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(delay);

        Inputs.ignoreDefaultPhysics = false;
        Inputs.canjump = true;
        wallJumping.ignoreJumpInput = false;
        wasattacked = false;
        canattack = true;
        candive = true;
        Inputs.canmove = true;
        inHitstun = false;
        hitstuntime = 0;
        wasattacked = false;
    }

    internal void InvokeOnAttacked(bool choice)
    {
        ignoreDragOnAttack = choice;
        OnAttacked?.Invoke();
    }
}
