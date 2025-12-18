using UnityEngine;
using System.Collections;
using System;

public enum AttackState
{
    Attacking,
    AttackingCanMove,
    AttackingCanJump,
    AttackingCanMoveJump,
    notAttacking,
    DefensiveBlocking,
    DefensiveDodge,
    DefensiveParrying,
    InInventory
}

public enum MovementState
{
    Idle,
    Running,
    Sliding,
    Jumping,
    Falling,
    Blocking,
    InInventory,
}

[RequireComponent(typeof(Attacks))]
public class Movement2 : MonoBehaviour
{
    [Header("Ground Detection")]
    private Coroutine RunDelayRout;
    [SerializeField] private Rigidbody2D rb;
    public Rigidbody2D RigBod;
    public bool turning;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform RightWallCheck;
    [SerializeField] private Transform LeftWallCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask RightWallLayer;
    [SerializeField] private LayerMask LeftWallLayer;

    [Header("Movement2")]
    [SerializeField] private float jumpPower = 1f;
    [SerializeField] private float speedMulti = 2.5f;
    public float jumpTime = 0;
    public bool runningState = false;
    public bool FacingRight = true;

    [HideInInspector] public float horiz;
    public float vert;
    public bool canmove = true;
    [HideInInspector] public float speed = 0f;
    public bool canjump;
    public GameControlsManager inputHandler;
    [SerializeField] private WallJumping wallJumpScrpt;
    public Health playerHealth;
    public float movement;
    [SerializeField] public Attacks attacks;

    public AttackState currAttackState = AttackState.notAttacking;
    public MovementState currMovementState = MovementState.Idle;
    public bool hasSuperArmor = false;
    public bool encounteredBoss = false;
    //resistance value later calculated as a raw percentage
    /// <summary>
    /// <param name="resistanceRaw">Resistance as a percentage. From 0 to 100</param>
    /// </summary>
    public float resistanceRaw = 0f;
    public bool normalizeKnockbackIfNoShield;
    
    [Range(0, 1)]
    public float storedOriginalBounciness;

    public float jumpPressedTimer = 0;

    public bool ignoreDefaultPhysics = false;
    public float idleDrag = 10f;
    [SerializeField] private bool justAttacked = false;

    public Movement2 TargetFirstRevivePlayer = null; 
    
    public bool OutOfBounds = false;
    private float _outOfBoundsTimer = 0f;
    public float OutOfBoundsTimer
    {
        get => _outOfBoundsTimer;
        set
        {
            _outOfBoundsTimer = Mathf.Clamp(value, 0f, 10f);
            // These numbers are in seconds with Time.deltaTime
            OutOfBounds = _outOfBoundsTimer > 3f;
            if (_outOfBoundsTimer <= 0f)
            {
                _outOfBoundsTimer = 0f;
            }
        }
    }


    public void NormalizeKnockbackIfNoShield()
    {
        if (CompareTag("Player"))
        {
            normalizeKnockbackIfNoShield = false;
        }
        else return;
    }

    [HideInInspector] public float resistanceMultiplier;

    public int Spr_Dir()
    {
        return FacingRight ? 1 : -1;
    }

    public virtual void Awake()
    {
        Attacks attacks = GetComponent<Attacks>();
        rb = RigBod;
        rb.sharedMaterial = new PhysicsMaterial2D("PhysicsMaterial2D")
        {
            friction = 0f,
            bounciness = storedOriginalBounciness
        };
        resistanceMultiplier = Mathf.Clamp(1 - resistanceRaw/100,0f,1f);
        NormalizeKnockbackIfNoShield();
    }
    // Update is called once per frame
    void Start()
    {
        playerHealth.health = 100f;
        playerHealth.shield = 50f;
    }
    public virtual void Update()
    {
        
        //managing attackStates to modify movement and jump ability
        switch (currAttackState)
        {
            case AttackState.Attacking:
                canmove = false;
                canjump = false;
                return;
            case AttackState.AttackingCanMove:
                canmove = true;
                canjump = false;
                return;
            case AttackState.AttackingCanJump:
                canmove = false;
                canjump = true;
                return;
            case AttackState.DefensiveBlocking:
                canmove = false;
                canjump = false;
                return;
            case AttackState.DefensiveDodge:
                canmove = false;
                canjump = false;
                break;
            case AttackState.InInventory:
                canmove = false;
                canjump = false;
                return;
            default:
                break;
        }
        if (canmove)
        {
            if (StrongAxisDir() == 0) horiz = inputHandler.MoveInput.x;
            if (StrongAxisDir() == 0) vert = inputHandler.MoveInput.y;
        }
        else if (!canmove)
        {
            return;
        }

        StrongAxisDir();

        jumpPressedTimer = Mathf.Clamp(jumpPressedTimer, 0f, 30f);
        if (Jump() && jumpPressedTimer < 30) jumpPressedTimer += 1f * Time.deltaTime;
        if (!Jump()) jumpPressedTimer = 0;

    }

    #region Input Methods
    public bool HoldRun()
    {
        return inputHandler.sprintTriggered;
    }

    [HideInInspector]
    public bool Jump()
    {
        return inputHandler.JumpTriggered;
    }
    [HideInInspector]
    public bool Attack()
    {
        if (attacks.canattack) return inputHandler.attackTriggered;
        else return false;
    }
    [HideInInspector]
    public bool Block()
    {
        if (attacks.canblock) return inputHandler.blockTriggered;
        else return false;
    }
    [HideInInspector]
    public bool Parry()
    {
        if (attacks.canblock && TargetFirstRevivePlayer == null) return inputHandler.parryTriggered;
        else return false;
    }

    [HideInInspector]
    public bool Strong()
    {
        if (attacks.canattack) return inputHandler.strongTriggered;
        else return false;
    }

    [HideInInspector]
    public bool StrongAxis()
    {
        if (attacks.canattack) return inputHandler.strongAxisTriggered != Vector2.zero;
        else return false;
    }

    [HideInInspector]
    public int StrongAxisDir()
    {
        if (attacks.canattack)
        {
            float axisx = inputHandler.strongAxisTriggered.x;
            float axisy = inputHandler.strongAxisTriggered.y;
            if (StrongAxis())
            {
                if (axisx == 0) return 0;
                horiz = axisx > 0 ? 1 : -1;
                if (Mathf.Sign(axisx) == Spr_Dir())
                {
                    return 1;
                }
                else if (Mathf.Sign(axisx) != Spr_Dir())
                {
                    Vector3 localScale = transform.localScale;
                    localScale = new Vector3(localScale.x*-1f,1f,1f);
                    FacingRight = !FacingRight;
                    transform.localScale = localScale;
                    return -1;
                }
                if (Mathf.Sign(axisy) == 1f)  {
                    vert = 1;
                    return 2;
                } else if (Mathf.Sign(axisy) == -1f) {
                    horiz = -1;
                    return -2;
                }
            }
        }
        return 0;
    }
    public void ChangeResistance(float resistanceValue)
    {
        resistanceRaw = resistanceValue;
        resistanceMultiplier = Mathf.Clamp(1 - resistanceRaw/100,0f,1f);
    }
    public bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }
    public bool IsRightWall()
    {
        return Physics2D.OverlapCircle(RightWallCheck.position, 0.1f, RightWallLayer);
    }
    public bool IsLeftWall()
    {
        return Physics2D.OverlapCircle(LeftWallCheck.position, 0.1f, LeftWallLayer);
    }

    public bool IsDucking => IsGrounded() && vert < -0.1;

    #endregion




    public virtual void FixedUpdate()
    {
        if (ignoreDefaultPhysics)
            return;
        if (attacks.wasattacked || attacks.inHitstun)
        {
            rb.linearDamping = attacks.ignoreDragOnAttack ? 0f : idleDrag; // Disable damping during hitstun or when attacked
            return;
        }

        if (!turning) GroundDetect();

        HandleMovement();

        // Handle jumping
        HandleJumping();

        if (rb.linearVelocity.y < 0 && !IsRightWall())
        {
            rb.gravityScale = 2.5f;
        }

        // Ground detection


        if (turning)
        {
            DeccelerationRunning();
        }

        // Handle movement

    }

    private void HandleJumping()
    {
        if (IsGrounded() && canjump && Jump())
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f); // Reset vertical velocity
            rb.AddForce(new Vector2(0f, jumpPower), ForceMode2D.Impulse);

            rb.linearDamping = 0f; // No damping while jumping
            // Clamp the vertical velocity to prevent excessive jump height
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Clamp(rb.linearVelocity.y, 0f, jumpPower));
        }
        else if (!IsGrounded() && canjump && !Jump())
        {
            // Disable jump if not grounded and jump input is pressed
            canjump = false;
            rb.gravityScale = 2.5f; // Apply gravity when airborne
        }
    }

    private void HandleMovement()
    {
        if (IsDucking)
        {
            rb.linearDamping = 12f;
            if (horiz != 0)
            {

            }
        }
        if (attacks.attackMove == AttackMove.Dive && attacks.DiveState == DiveState.Landed && inputHandler.MoveInput.x != 0 && inputHandler.MoveInput.x == -1 * Spr_Dir())
            {
                rb.linearDamping = 3f;
            }
        if (canmove)
        {
            if (horiz != 0)
            {
                Moving();
                rb.linearDamping = 0f; // No damping while moving
                if(IsGrounded()) rb.gravityScale = 1.2f;
            }
            else if (horiz == 0 && IsGrounded())
            {
                rb.linearDamping = 10f; // Apply damping to slow down
            }
            Direction();
        }
    }

    public virtual void JumpingInput()
    {
        // Check if either jump input isn't detected or (if the player is diving) block input isn't triggered
        if (!Jump() && !IsGrounded())
        {
            canjump = false;
            rb.gravityScale = 2.5f;
        }

        if (IsGrounded() && canjump && Jump())
        {
            rb.AddForceY(jumpPower, ForceMode2D.Impulse);
        }
    }

    private void GroundDetect()
    {
        if (IsGrounded() && currAttackState == AttackState.notAttacking)
        {
            jumpTime = 0f;
            canjump = true;
            justAttacked = false; // Reset justAttacked flag
            rb.gravityScale = 1.2f;
        }
    }

    private void Moving()
    {
        if (horiz != 0 && !turning && !wallJumpScrpt.walljumpingState)
        {
            if (IsDucking)
            {
                
            }
            // Determine if the player is walking or running
            else if (HoldRun() && !runningState && IsGrounded())
            {
                // Start running after a delay
                RunDelayRout ??= StartCoroutine(RunningAnimDelay(0.4f)); // Delay before running
            }
            else if (!HoldRun() && IsGrounded())
            {
                // Cancel running if the player stops holding the run button
                CancelRunning();
                runningState = false;
            }

            // Apply movement force based on the current state (walking or running)
            float currentSpeed = runningState ? speedMulti * 1.5f : speedMulti; // Running is faster

            if (IsGrounded())
            {
                // Apply force for grounded movement
                rb.AddForce(new Vector2(horiz * currentSpeed * 3f, 0f), ForceMode2D.Force);
            }
            else
            {
                // Apply reduced force for midair movement
                float airControlMultiplier = 1f; // less control in midair
                float airSpeed = currentSpeed * airControlMultiplier;
                rb.AddForce(new Vector2(horiz * airSpeed, 0f), ForceMode2D.Force);
            }

            // Clamp the horizontal velocity
            rb.linearVelocityX = runningState
                ? Mathf.Clamp(rb.linearVelocity.x, -16f, 16f)
                : Mathf.Clamp(rb.linearVelocity.x, -5f, 5f);

            //rb.linearDamping = IsGrounded() ? 0f : 0.5f; // Apply damping in midair
        }
        else if (horiz == 0 && !turning && !wallJumpScrpt.walljumpingState)
        {
            // Apply damping to stop movement when idle
            rb.linearDamping = IsGrounded() ? 10f : 4f;
        }
    }

    #region RunningTimers

    IEnumerator RunningAnimDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        runningState = true;
    }
    private void CancelRunning()
    {
        if (RunDelayRout != null)
        {
            StopCoroutine(RunDelayRout);
            RunDelayRout = null;
        }
    }
    #endregion

    #region DirectionChecking
    private bool FacingRightStickLeft => FacingRight && horiz < 0f;
    private bool FacingLeftStickRight => !FacingRight && horiz > 0f;
    public void Direction()
    {
        if (horiz != 0 && (FacingRightStickLeft || FacingLeftStickRight) && !wallJumpScrpt.walljumpingState && !turning)
        {
            if (Mathf.Abs(rb.linearVelocity.x) > 5f && runningState && IsGrounded())
            {
                turning = true;
                canjump = false;
                canmove = false;
                attacks.AttkHoldTime = 10f;
            }
            else if (Mathf.Abs(rb.linearVelocity.x) > 8f && IsGrounded())
            {
                turning = true;
                canjump = false;
                canmove = false;
                attacks.AttkHoldTime = 10f;
            }
            else
            {
                if (IsGrounded()) rb.linearVelocity = Vector2.zero;
                FacingRight = !FacingRight;
                Vector3 localScale = transform.localScale;
                localScale.x = FacingRight ? Mathf.Abs(localScale.x) : -Mathf.Abs(localScale.x);
                transform.localScale = localScale;
                canmove = true;
                turning = false;
            }
        }
    }

    public float skidTimer = 10f;

    public void DeccelerationRunning()
    {
        if (turning)
        {
            if (skidTimer > 0)
            {        

                skidTimer = Mathf.Clamp(skidTimer, 0.1f, 20f);
                float initialVelocity = Mathf.Max(Mathf.Abs(rb.linearVelocity.x), 0.01f);
                float targetVelocity = 0.01f;
                // Apply high damping to slow down the player
                float requiredDrag = 3f * -Mathf.Log(targetVelocity / initialVelocity)/skidTimer;
                if(requiredDrag == float.PositiveInfinity || requiredDrag == float.NegativeInfinity)
                rb.linearDamping = 0;
                else
                rb.linearDamping = requiredDrag;

                // Decrement the skid timer
                skidTimer -= Time.fixedDeltaTime * 20f;
                attacks.canattack = false;
                attacks.canblock = false;
                canjump = false;
            }
            // Check if the skid duration has elapsed
            if (skidTimer <= 0.1f)
            {
                rb.linearVelocity = Vector2.zero; // Stop the player completely
                FacingRight = !FacingRight; // Flip the direction
                Vector3 localScale = transform.localScale;
                localScale.x = FacingRight ? Mathf.Abs(localScale.x) : -Mathf.Abs(localScale.x);
                transform.localScale = localScale;

                // Reset turning state
                canmove = true;
                turning = false;
                canjump = true;
                skidTimer = 10;
                attacks.canattack = true;
                attacks.canblock = true;
                attacks.AttkTimer = 10f;
            }
        }
    }

    public void WallCling()
    {
        speed = 0;
        movement = 0;
        RigBod.linearVelocity = new Vector2(0, RigBod.linearVelocity.y);
    }
}
#endregion
