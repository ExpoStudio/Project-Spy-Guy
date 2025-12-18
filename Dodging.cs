using UnityEngine;

public enum DodgingMove
{
    NONE,
    FORWARD_DODGE,
    BACKWARD_DODGE,
    SPOT_DODGE,
    JUMPING_DODGE,
    AIR_DODGE,
    AIR_DODGE_DIRECTIONAL
}

public enum DodgeType
{
    GROUND,
    AIR
}

public class Dodging : MonoBehaviour
{
    private Movement2 _movement;
    private Attacks _attacks;
    private Health _health;
    [SerializeField] private float _dodgeTimer;
    [SerializeField] private float _stickHorizHoldTimer;
    [SerializeField] private float _stickVertHoldTimer;
    [SerializeField] private Collider2D slowMotionBox;
    public DodgingMove dodgeState = DodgingMove.NONE;
    public DodgeType dodgeType = DodgeType.GROUND;

    //Properties
    public bool canDodge = false;
    public bool didDodge = false;
    public bool IsDodging => dodgeState != DodgingMove.NONE;


    private void Start()
    {
        _movement = GetComponent<Movement2>();
        _attacks = GetComponent<Attacks>();
        _health = GetComponent<Health>();
        slowMotionBox.enabled = false;
    }

    // Update is called once per frame
    private void Update()
    {
        if (didDodge) canDodge = false;
        if (dodgeState == DodgingMove.NONE)
        {
            bool flowControl = EvaluateDodgeState();
            if (!flowControl)
            {
                return;
            }
        }

        _stickHorizHoldTimer = Mathf.Clamp(_stickHorizHoldTimer, 0, 3f);
        _stickVertHoldTimer = Mathf.Clamp(_stickVertHoldTimer, 0, 3f);
        if(_movement.inputHandler.MoveInput.x != 0) _stickHorizHoldTimer += Time.deltaTime;
        else _stickHorizHoldTimer = 0;
        if(_movement.inputHandler.MoveInput.y != 0) _stickVertHoldTimer += Time.deltaTime;
        else _stickVertHoldTimer = 0;
        _dodgeTimer = Mathf.Clamp(_dodgeTimer, 0f, 30f);
        if (dodgeState != DodgingMove.NONE)
        {
            _dodgeTimer += Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        switch (dodgeType)
        {
            case DodgeType.GROUND:
                switch (dodgeState)
                {
                    case DodgingMove.FORWARD_DODGE:
                        if (didDodge && slowMotionBox.enabled)
                        {
                            _health.ChooseFlash(new Color(100f, 0, 100f, 0.1f), 26f, 0.02f);
                        }
                        else if (didDodge && !slowMotionBox.enabled && !_attacks.HurtBox.enabled)
                        {
                            _health.ChooseFlash(Color.gray, 26f, 0.35f);
                        }

                        if (!didDodge && _dodgeTimer < FrameUtils.FramesToSeconds(8f))
                        {
                            didDodge = true;
                            _attacks.HurtBox.enabled = false;
                            slowMotionBox.enabled = true;
                            _movement.RigBod.AddForce(new Vector2(10f * _movement.Spr_Dir(), 0f), ForceMode2D.Impulse);
                            //Switch direction when forward dodging
                            FlipDirection();
                        }
                        else if (didDodge && _dodgeTimer >= FrameUtils.FramesToSeconds(8f) && _dodgeTimer < FrameUtils.FramesToSeconds(18f))
                        {
                            slowMotionBox.enabled = false;
                            Debug.Log("Perfect Dodge Removed");
                        }
                        else if (didDodge && _dodgeTimer >= FrameUtils.FramesToSeconds(18f) && _dodgeTimer < FrameUtils.FramesToSeconds(30f))
                        {
                            _movement.RigBod.linearDamping = 15f;
                            Debug.Log("Drag Enabled");
                        }
                        else if (didDodge && _dodgeTimer >= FrameUtils.FramesToSeconds(30f) && _dodgeTimer < FrameUtils.FramesToSeconds(48f))
                        {
                            _attacks.HurtBox.enabled = true;
                            Debug.Log("HurboxRe-EnabledSuccessfully");
                        }
                        else if (didDodge && _dodgeTimer >= FrameUtils.FramesToSeconds(48f))
                        {
                            _movement.RigBod.linearVelocity = Vector2.zero;
                            _attacks.attackMove = AttackMove.None;
                            _movement.currAttackState = AttackState.DefensiveBlocking;
                            dodgeState = DodgingMove.NONE;
                            _movement.ignoreDefaultPhysics = false;
                            didDodge = false;
                            _attacks.AttkTimer += 14f;
                            _dodgeTimer = 0;
                            Debug.Log("Dodge finished. Resetting states.");
                        }
                        break;
                    case DodgingMove.BACKWARD_DODGE:
                        if (didDodge && slowMotionBox.enabled)
                        {
                            _health.ChooseFlash(new Color(100f, 0, 100f, 0.1f), 26f, 0.02f);
                        }
                        else if (didDodge && !slowMotionBox.enabled && !_attacks.HurtBox.enabled)
                        {
                            _health.ChooseFlash(Color.gray, 26f, 0.35f);
                        }

                        if (!didDodge && _dodgeTimer < FrameUtils.FramesToSeconds(8f))
                        {
                            didDodge = true;
                            _attacks.HurtBox.enabled = false;
                            slowMotionBox.enabled = true;
                            _movement.RigBod.AddForce(new Vector2(-10f * _movement.Spr_Dir(), 0f), ForceMode2D.Impulse);
                        }
                        else if (didDodge && _dodgeTimer >= FrameUtils.FramesToSeconds(8f) && _dodgeTimer < FrameUtils.FramesToSeconds(18f))
                        {
                            slowMotionBox.enabled = false;
                            Debug.Log("Perfect Dodge Removed");
                        }
                        else if (didDodge && _dodgeTimer >= FrameUtils.FramesToSeconds(18f) && _dodgeTimer < FrameUtils.FramesToSeconds(30f))
                        {
                            _movement.RigBod.linearDamping = 10f;
                            Debug.Log("Drag Enabled");
                        }
                        else if (didDodge && _dodgeTimer >= FrameUtils.FramesToSeconds(30f) && _dodgeTimer < FrameUtils.FramesToSeconds(48f))
                        {
                            _attacks.HurtBox.enabled = true;
                            Debug.Log("HurboxRe-EnabledSuccessfully");
                        }
                        else if (didDodge && _dodgeTimer >= FrameUtils.FramesToSeconds(48f))
                        {
                            _movement.RigBod.linearVelocity = Vector2.zero;
                            _attacks.attackMove = AttackMove.None;
                            _movement.currAttackState = AttackState.DefensiveBlocking;
                            dodgeState = DodgingMove.NONE;
                            _movement.ignoreDefaultPhysics = false;
                            didDodge = false;
                            _attacks.AttkTimer += 14f;
                            _dodgeTimer = 0;
                            Debug.Log("Dodge finished. Resetting states.");
                        }
                        break;
                    case DodgingMove.SPOT_DODGE:
                        if (didDodge && slowMotionBox.enabled)
                        {
                            _health.ChooseFlash(new Color(100f, 0, 100f, 0.1f), 26f, 0.02f);
                        }
                        else if (didDodge && !slowMotionBox.enabled && !_attacks.HurtBox.enabled)
                        {
                            _health.ChooseFlash(Color.gray, 26f, 0.35f);
                        }

                        if (!didDodge && _dodgeTimer < FrameUtils.FramesToSeconds(8f))
                        {
                            didDodge = true;
                            _attacks.HurtBox.enabled = false;
                            slowMotionBox.enabled = true;
                            _movement.RigBod.linearVelocity = Vector2.zero;
                        }
                        else if (didDodge && _dodgeTimer >= FrameUtils.FramesToSeconds(8f) && _dodgeTimer < FrameUtils.FramesToSeconds(16f))
                        {
                            slowMotionBox.enabled = false;
                            Debug.Log("Perfect Dodge Removed");
                        }
                        else if (didDodge && _dodgeTimer >= FrameUtils.FramesToSeconds(16f) && _dodgeTimer < FrameUtils.FramesToSeconds(26f))
                        {
                            _movement.RigBod.linearDamping = 10f;
                            _attacks.HurtBox.enabled = true;
                            Debug.Log("Drag Enabled");
                        }
                        else if (didDodge && _dodgeTimer >= FrameUtils.FramesToSeconds(35f))
                        {
                            _movement.RigBod.linearVelocity = Vector2.zero;
                            _attacks.attackMove = AttackMove.None;
                            _movement.currAttackState = AttackState.DefensiveBlocking;
                            dodgeState = DodgingMove.NONE;
                            _movement.ignoreDefaultPhysics = false;
                            didDodge = false;
                            _attacks.AttkTimer += 14f;
                            _dodgeTimer = 0;
                            Debug.Log("Dodge finished. Resetting states.");
                        }
                        break;
                    default:
                        if (!didDodge && _movement.IsGrounded() && canDodge && _movement.inputHandler.MoveInput.x != 0 && Mathf.Sign(_movement.inputHandler.MoveInput.x) == _movement.Spr_Dir() && _movement.inputHandler.MoveInput.y == 0 && _health.guardUp && _stickHorizHoldTimer < 0.2f)
                        {
                            _movement.ignoreDefaultPhysics = true;
                            _attacks.attackMove = AttackMove.Defensive;
                            _movement.currAttackState = AttackState.DefensiveDodge;
                            dodgeState = DodgingMove.FORWARD_DODGE;
                            _movement.RigBod.linearDamping = 0f;
                            _movement.RigBod.linearVelocity = Vector2.zero;
                        }
                        else if (!didDodge && _movement.IsGrounded() && canDodge && _movement.inputHandler.MoveInput.x != 0 && Mathf.Sign(_movement.inputHandler.MoveInput.x) != _movement.Spr_Dir() && _movement.inputHandler.MoveInput.y == 0 && _health.guardUp && _stickHorizHoldTimer < 0.2f)
                        {
                            _movement.ignoreDefaultPhysics = true;
                            _attacks.attackMove = AttackMove.Defensive;
                            _movement.currAttackState = AttackState.DefensiveDodge;
                            dodgeState = DodgingMove.BACKWARD_DODGE;
                            _movement.RigBod.linearDamping = 0f;
                            _movement.RigBod.linearVelocity = Vector2.zero;
                        }
                        else if (!didDodge && _movement.IsGrounded() && canDodge && _movement.inputHandler.MoveInput.x == 0 && Mathf.Sign(_movement.inputHandler.MoveInput.y) == -1 && _movement.inputHandler.MoveInput.y != 0 && _health.guardUp && _stickVertHoldTimer < 0.2f)
                        {
                            _movement.ignoreDefaultPhysics = true;
                            _attacks.attackMove = AttackMove.Defensive;
                            _movement.currAttackState = AttackState.DefensiveDodge;
                            dodgeState = DodgingMove.SPOT_DODGE;
                            _movement.RigBod.linearDamping = 0f;
                            _movement.RigBod.linearVelocity = Vector2.zero;
                        }
                        break;
                }
                break;
            case DodgeType.AIR:
            
                break;
        }
    }

    private void FlipDirection()
    {
        _movement.FacingRight = !_movement.FacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x = _movement.FacingRight ? Mathf.Abs(localScale.x) : -Mathf.Abs(localScale.x);
        transform.localScale = localScale;
    }

    private bool EvaluateDodgeState()
    {
        switch (didDodge)
        {
            case false:
                if (_movement.IsGrounded() && _health.guardUp && _movement.currAttackState == AttackState.DefensiveBlocking && !_movement.HoldRun())
                {
                    canDodge = true;
                    dodgeType = DodgeType.GROUND;
                }
                else if (_movement.IsGrounded() && !_movement.canmove && _movement.currAttackState != AttackState.DefensiveBlocking && !_movement.HoldRun())
                {
                    canDodge = false;
                    dodgeType = DodgeType.GROUND;
                }
                else if (!_movement.IsGrounded() && _movement.canmove && !_movement.HoldRun())
                {
                    canDodge = true;
                    dodgeType = DodgeType.AIR;
                }
                else
                {
                    canDodge = false;
                }
                break;
            case true:
                if (_movement.IsGrounded())
                {
                    didDodge = false;
                }
                else
                {
                    return false;
                }
                break;
        }
        return true;
    }
}
