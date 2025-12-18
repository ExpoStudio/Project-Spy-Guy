using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public enum PlayerDownedState
{
    ALIVE,
    DOWNED_NOT_LANDED,
    DOWNED_LANDED,
    DOWNED_HEALTH_DRAIN_STAGE,
    DEAD_AND_DESPAWNING
}

public interface IHealthNotifier
{
    event Action OnHealthChanged;
}

public class Health : MonoBehaviour, IHealthNotifier
{
    public event Action OnHealthChanged;
    // The current health of the entity
    public float health;
    public float shield;
    private float _damageGiven = 0f; //Backing field for damageGiven property
    public float DamageGiven
    {
        get => _damageGiven;
        set
        {
            _damageGiven = value;
            OnHealthChanged?.Invoke(); // Notify subscribers when damageGiven changes, i.e. Health is changing
        }
    }
    public float damage;
    private float _originalScaleX;
    private float _originalScaleY;
    private float _originalShieldY;

    public float _originalShieldX { get; private set; }
    public float OriginalAlpha { get; private set; }
    [SerializeField] private PlayerInput _playerInput;
    [SerializeField] private CharacterSelectionSystem _characterSelectionSystem;

    [SerializeField] private DamageFlash DamageFlash, CustomFlash;
    [SerializeField] private DamageFlashImage ShieldMatFlash;
    [SerializeField] private DamageFlashImage HealthMatFlash;
    [SerializeField] private StrongCharge StrongCharge;
    [SerializeField] public RectTransform HealthBar;
    [SerializeField] public RectTransform ShieldBar;

    [SerializeField] private GameObject _assignedUI;
    private Movement2 Movement;
    private Attacks attacks;
    private Image healthBarComponent;
    private Image healthBarChildComponent;
    private Image shieldBarComponent;
    private Coroutine slowRoutiune;
    public bool shieldBroken = false;
    public float shieldMaxValue;
    public bool guardUp;
    public float _storedShield;
    public float totalDmgInflicted = 0;
    private bool canUnshield;
    public int playerIndex;
    private float deadActionTimer;
    private bool CanRevive => _characterSelectionSystem.PlayerPhase is PlayerGameState.DOWNED && playerDownedState is PlayerDownedState.DOWNED_HEALTH_DRAIN_STAGE;
    private bool _alreadyAssigned = false;

    internal bool AvailablePlayerState => _characterSelectionSystem.PlayerPhase is not(PlayerGameState.EXISTING_BUT_NOT_JOINED or PlayerGameState.CHARACTER_SELECTION or PlayerGameState.DOWNED or PlayerGameState.DEAD);
    public PlayerDownedState playerDownedState = PlayerDownedState.ALIVE;
    void Awake()
    {
        _playerInput = GetComponentInParent<PlayerInput>();
        
        if (_playerInput == null)
        {
            Debug.LogError("PlayerInput component missing!");
            return;
        }

        playerIndex = _playerInput.playerIndex;
        Debug.Log($"Player Index: {playerIndex}");

    }
    void OnEnable()
    {
        if(!_alreadyAssigned && AvailablePlayerState) InitializeUI();
    }
    void Start()
    {
        if(!_alreadyAssigned && AvailablePlayerState) InitializeUI();
    }
    void InitializeUI()
    {
        _assignedUI = GameObject.Find($"Player{playerIndex}HUD");
        if (CompareTag("Player"))
        {
            HealthBar = (RectTransform)_assignedUI.transform.GetChild(5).transform;
            if (HealthBar == null)
            {
                Debug.LogError("HealthBar RectTransform not found!");
                return;
            }
            ShieldBar = (RectTransform)_assignedUI.transform.GetChild(6).transform;
            if (ShieldBar == null)
            {
                Debug.LogError("ShieldBar RectTransform not found!");
                return;
            }
        }
        else
        {
            Debug.Log("Non-Player Tag Detected, using default HealthBar and ShieldBar.");
        }
        if (CompareTag("Player")) ShieldMatFlash = ShieldBar.GetChild(0).GetComponent<DamageFlashImage>();
        if (CompareTag("Player")) HealthMatFlash = HealthBar.GetChild(0).GetComponent<DamageFlashImage>();

        _originalScaleX = HealthBar.transform.localScale.x;
        _originalScaleY = HealthBar.transform.localScale.y;
        _originalShieldY = ShieldBar.transform.localScale.y;
        _originalShieldX = ShieldBar.transform.localScale.x;
        shieldBarComponent = ShieldBar.GetComponentInChildren<Image>();
        OriginalAlpha = shieldBarComponent.color.a + 0.2f;
        StrongCharge = GetComponent<StrongCharge>();
        Movement = GetComponentInParent<Movement2>();
        attacks = GetComponent<Attacks>();
        healthBarComponent = HealthBar.GetComponent<Image>();
        healthBarChildComponent = HealthBar.GetComponentInChildren<Image>();
        shieldMaxValue = shield;
        _storedShield = shieldMaxValue;
        CustomFlash = DamageFlash;

        _alreadyAssigned = true;
    }

    #region Shield Implemented In Health
    private void UpdateShield(float value)
    {
        shield = value;
    }


    void Update()
    {
        if (!_alreadyAssigned && AvailablePlayerState)
        {
            InitializeUI();
            return;
        }

        //Later Change This To Alive Player Count List in a seperate
        int AlivePlayerCount = GameStateManager.Instance.AlivePlayerCount;
        float frameTime = Time.deltaTime;
        switch (playerDownedState)
        {
            case PlayerDownedState.ALIVE:
                if (health <= 0 && AlivePlayerCount == 1)
                {
                    deadActionTimer = 0;
                    playerDownedState = PlayerDownedState.DEAD_AND_DESPAWNING;
                    _characterSelectionSystem.PlayerPhase = PlayerGameState.DOWNED;
                }
                else if (health <= 0 && AlivePlayerCount > 1)
                {
                    deadActionTimer = 0;
                    playerDownedState = PlayerDownedState.DOWNED_NOT_LANDED;
                    _characterSelectionSystem.PlayerPhase = PlayerGameState.DOWNED;
                }
                break;
            case PlayerDownedState.DOWNED_NOT_LANDED:
                deadActionTimer += frameTime;
                if (Movement.IsGrounded() && deadActionTimer >= 0.25f)
                {
                    deadActionTimer = 0;
                    playerDownedState = PlayerDownedState.DOWNED_LANDED;
                }
                break;
            case PlayerDownedState.DOWNED_LANDED:
                if (!Movement.IsGrounded())
                {
                    deadActionTimer = 0f;
                    playerDownedState = PlayerDownedState.DOWNED_NOT_LANDED;
                }
                else if (deadActionTimer > 1f)
                {
                    deadActionTimer = 0f;
                    playerDownedState = PlayerDownedState.DOWNED_HEALTH_DRAIN_STAGE;
                }
                break;
            case PlayerDownedState.DOWNED_HEALTH_DRAIN_STAGE:
                deadActionTimer += frameTime;
                break;
            case PlayerDownedState.DEAD_AND_DESPAWNING:
                CameraPositionController.NotifyOutOfBounds(Movement);
                if (deadActionTimer < 3f)
                {
                    deadActionTimer += frameTime;
                }
                else if (deadActionTimer >= 3f)
                {
                    deadActionTimer = 0;
                    _characterSelectionSystem.PlayerPhase = PlayerGameState.DEAD;
                }
                break;
            default:
                if (health > 0)
                {
                    playerDownedState = PlayerDownedState.ALIVE;
                }
                break;
        }


        bool isDodging = TryGetComponent<Dodging>(out var dodging) && dodging.IsDodging;
        if(isDodging) 
        {
            HapticHold.StopVibration();
            return;
        }

        float damageToApply = Mathf.Min(DamageGiven, 30f * Time.deltaTime);
        
        if (DamageGiven > 0f && shield > 0)
        {
            shield -= damageToApply * (CompareTag("Player") ? Movement.resistanceMultiplier : 1f);
            DamageGiven -= damageToApply * (CompareTag("Player") ? Movement.resistanceMultiplier : 1f);
            damage = 0;
        }
        else if (DamageGiven > 0f && shield <= 0.01f)
        {
            shield = 0;
            health -= damageToApply * (CompareTag("Player") ? 1f : (Movement.resistanceMultiplier + 0.85f));
            DamageGiven -= damageToApply * (CompareTag("Player") ? 1f : (Movement.resistanceMultiplier + 0.85f));
            damage = 0;
        }
        health = Mathf.Clamp(health, 0f, 1000f);
        shield = Mathf.Clamp(shield, 0f, 1000f);
        DamageGiven = Mathf.Clamp(DamageGiven, 0f, 1000f);

        if (this != null && CompareTag("Player"))
        {
            bool isDamageGivenLessThanThreshold = DamageGiven < totalDmgInflicted * 0.6f;
            if (Movement.IsGrounded() && !Movement.runningState && !shieldBroken)
            {
                GroundShieldingLogic(isDamageGivenLessThanThreshold);
            }
            else if (Movement.IsGrounded() && !Movement.runningState && shieldBroken)
            {
                GroundGuardNoShieldLeftLogic();
            }
        }
        else
        {
            if (Mathf.Approximately(shield, 0) && !shieldBroken)
            {
                SlowDownTime(0.2f, 2f);
                shieldBroken = true;
            }
            else if (shield > 0 && shieldBroken)
            {
                shieldBroken = false;
            }
        }
    }

    private void GroundGuardNoShieldLeftLogic()
    {
        bool isDodging = TryGetComponent<Dodging>(out var dodging) && dodging.IsDodging;

        if (isDodging)
        {
            HapticHold.StopVibration();
            return;
        }

        bool isBlocking = Movement.Block();
        if (isBlocking)
        {
            Movement.speed = 0;
            Movement.movement = 0;
            Movement.RigBod.linearVelocity = new Vector2(0,0);
            ShieldingFlash();
            if (TryGetComponent<Attacks>(out var attacks))
            {
                attacks.canattack = false;
            }
            if (!guardUp) guardUp = true;
            Movement.currAttackState = AttackState.DefensiveBlocking;
            attacks.AttkTimer = 14f;
        }
        else if (!isBlocking && canUnshield)
        {
            if (guardUp)
            {
                _storedShield = shield;
                HapticHold.StopVibration();
                Movement.ChangeResistance(0); //store shield so it grows in background.
                UpdateShield(0);
                DamageFlash.SetFlashAmount(0);
                if (TryGetComponent<Attacks>(out var attacks))
                {
                    attacks.canattack = true;
                }
                Movement.currAttackState = AttackState.notAttacking;
                Movement.canmove = true;
            } //Set Shield to make visual effects disappear.
            totalDmgInflicted = 0f;
            guardUp = false;
        }
    }

    private void GroundShieldingLogic(bool isDamageGivenLessThanThreshold)
    {
        bool isDodging = TryGetComponent<Dodging>(out var dodging) && dodging.IsDodging;
        
        if (isDodging)
        {
            HapticHold.StopVibration();
            return;
        }

        if (totalDmgInflicted > 0)
        {
            canUnshield = isDamageGivenLessThanThreshold;
        }
        else
        {
            canUnshield = true;
        }

        bool isBlocking = Movement.Block();
        if (isBlocking && !shieldBroken && !Movement.runningState)
        {
            Movement.speed = 0;
            Movement.RigBod.linearVelocityX = 0;
            ShieldingFlash();
            Movement.hasSuperArmor = true;
            if (shield <= 0.01f && !guardUp)
            {
                shield = _storedShield;
                Movement.ChangeResistance(50);
                Movement.RigBod.linearVelocity = Vector2.zero;
            }
            if (!guardUp) guardUp = true;
            if (TryGetComponent<Attacks>(out var attacks))
            {
                attacks.canattack = false;
            }
            HapticHold.StartVibration(0.2f, 3f);
            Movement.currAttackState = AttackState.DefensiveBlocking;
            attacks.AttkTimer = 14f;
        }
        else if (!isBlocking && canUnshield)
        {
            if (guardUp)
            {
                _storedShield = shield;
                HapticHold.StopVibration();
                Movement.ChangeResistance(0); //store shield so it grows in background.
                UpdateShield(0);
                DamageFlash.SetFlashAmount(0);
                Movement.currAttackState = AttackState.notAttacking;
                Movement.canmove = true;
                if (TryGetComponent<Attacks>(out var attacks))
                {
                    attacks.canattack = true;
                }
            } //Set Shield to make visual effects disappear.
            shield = 0; //Set Shield to make visual effects disappear. 
            totalDmgInflicted = 0f;
            guardUp = false;
            Movement.hasSuperArmor = false;
        }

        //**Shield Draining While Blocking**
        if (guardUp && shield > 0 && !shieldBroken)
        {
            float guardMinus = Mathf.Min(0.5f, 3f * Time.deltaTime);
            shield -= guardMinus;
            shield = Mathf.Clamp(shield, 0f, shieldMaxValue);
        }

        if (!guardUp && !shieldBroken)
        {
            _storedShield = Mathf.Clamp(_storedShield, 0f, shieldMaxValue);
            if (_storedShield < shieldMaxValue)
            {
                float guardPlus = Mathf.Min(0.3f, 2f * Time.deltaTime);
                _storedShield += guardPlus;
            }
            //stop the shield regeneration at its maximum threshold
        }

        if (shield <= 0.01f && !shieldBroken && guardUp)
        {
            _storedShield = shield;
            if (_storedShield <= 0.01f)
            {
                TryGetComponent<Attacks>(out var attacks);
                SlowDownTime(0.2f, 1.2f);
                shieldBroken = true;
                Movement.currAttackState = AttackState.notAttacking;
                Movement.RigBod.linearVelocity = Vector2.up * 8f;
                StartCoroutine(attacks.Hitstun(2f));
            }
        }
    }
    #endregion 
    void FixedUpdate()
    {
        if (!_alreadyAssigned)
        {
            InitializeUI();
            return;
        }


        float ScaleCap = Mathf.Lerp(Math.Abs(HealthBar.transform.localScale.x), Mathf.Abs(_originalScaleX * (health * 0.01f)), Time.fixedDeltaTime * 4f);
        float ScaleSCap = Mathf.Lerp(Mathf.Abs(ShieldBar.transform.localScale.x), Mathf.Abs(_originalShieldX * shield/shieldMaxValue), Time.fixedDeltaTime * 3f);


        HealthBar.transform.localScale = new Vector2(ScaleCap, _originalScaleY);
        if (guardUp && CompareTag("Player"))
        {
            shieldBarComponent.color = new Color(shieldBarComponent.color.r, shieldBarComponent.color.g, shieldBarComponent.color.b, OriginalAlpha);
            ShieldBar.transform.localScale = new Vector2(ScaleSCap, _originalShieldY);
            shieldBarComponent.color = Color.Lerp(shieldBarComponent.color, new Color(shieldBarComponent.color.r, shieldBarComponent.color.g, shieldBarComponent.color.b, OriginalAlpha), Time.fixedDeltaTime*4f);
            if (Mathf.Approximately(shieldBarComponent.color.a, OriginalAlpha))
            {
                shieldBarComponent.color = new Color(shieldBarComponent.color.r, shieldBarComponent.color.g, shieldBarComponent.color.b, OriginalAlpha);
            }
        }
        else if(CompareTag("Player") && !guardUp)
        {
            shieldBarComponent.color = Color.Lerp(shieldBarComponent.color, new Color(shieldBarComponent.color.r, shieldBarComponent.color.g, shieldBarComponent.color.b, 0), Time.fixedDeltaTime*4f);
            if (Mathf.Approximately(shieldBarComponent.color.a, 0))
            {
                shieldBarComponent.color = new Color(shieldBarComponent.color.r, shieldBarComponent.color.g, shieldBarComponent.color.b, 0);
            }
        }
        else
        {
            ShieldBar.transform.localScale = new Vector2(ScaleSCap, _originalShieldY);
        }

        if (Mathf.Approximately(shield, 0f)) 
        {
            if (health <= 20 && health > 0)
            {
                if (StrongCharge != null && !StrongCharge.isCharging)
                    {
                        LowHealthBlinking();
                    } 
                    else if (StrongCharge == null)
                    {
                        LowHealthBlinking();
                    }
                try
                {
                    healthBarComponent.color = Color.Lerp(healthBarComponent.color, Color.red, Time.deltaTime * 4f);
                }
                catch (Exception)
                {
                    try
                    {
                        healthBarChildComponent.color = Color.Lerp(healthBarChildComponent.color, Color.red, Time.deltaTime * 4f);
                    }
                    catch (Exception ex)
                    {
                        Debug.Log(ex + "Fail Safe has failed");
                        return;
                    }
                    return;
                }
            }    
            else if (health <= 50 && health > 20)
            {
                try
                {
                    healthBarComponent.color = Color.Lerp(healthBarComponent.color, Color.yellow, Time.deltaTime * 4f);
                }
                catch (Exception)
                {
                    try
                    {
                        healthBarChildComponent.color = Color.Lerp(healthBarChildComponent.color, Color.yellow, Time.deltaTime * 4f);
                    }
                    catch (Exception ex)
                    {
                        Debug.Log(ex + "Fail Safe has failed");
                        return;
                    }
                    return;
                }
                if (DamageGiven <= 0.1) DamageFlash.SetFlashAmount(0);
                if (DamageGiven <= 0.1) ShieldMatFlash.SetFlashAmount(0);
            }
            else if (health > 50)
            {
                try
                {
                    healthBarComponent.color = Color.Lerp(healthBarComponent.color, Color.green, Time.deltaTime * 4f);
                }
                catch (Exception)
                {
                    try
                    {
                        healthBarChildComponent.color = Color.Lerp(healthBarChildComponent.color, Color.green, Time.deltaTime * 4f);
                    }
                    catch (Exception ex)
                    {
                        Debug.Log(ex + "Fail Safe has failed");
                        return;
                    }
                    return;
                }
                if (DamageGiven <= 0.1) DamageFlash.SetFlashAmount(0);
                if (DamageGiven <= 0.1) ShieldMatFlash.SetFlashAmount(0);
            }
        }
        else 
        {
            try
            { 
                healthBarComponent.color = Color.cyan;
            } 
            catch (Exception)
            {
                healthBarChildComponent.color = Color.cyan;
            }
            if(!CompareTag("Player"))
            {
                shieldBarComponent.color = Color.Lerp(shieldBarComponent.color, new Color(shieldBarComponent.color.r, shieldBarComponent.color.g, shieldBarComponent.color.b, OriginalAlpha), Time.fixedDeltaTime*4f);
                if (Mathf.Approximately(shieldBarComponent.color.a, OriginalAlpha))
                {
                    shieldBarComponent.color = new Color(shieldBarComponent.color.r, shieldBarComponent.color.g, shieldBarComponent.color.b, OriginalAlpha);
                }
            }
        }
    }
    private void LowHealthBlinking()
    {
        DamageFlash.SetFlashColor(Color.red);
        HealthMatFlash.SetFlashColor(Color.white);
        if (Mathf.Approximately(DamageGiven, 0f))
        {
            if (health > 0)
            {
                float frequency = Mathf.Clamp(Mathf.PI * 20f / health, 1f, 6f * Mathf.PI);
                DamageFlash.SetFlashAmount(0.35f + 0.35f * Mathf.Sin(Time.time * frequency));
                HealthMatFlash.SetFlashAmount(0.35f + 0.35f * Mathf.Sin(Time.time * frequency));
            }
        }
    }

    private void ShieldingFlash()
    {
        DamageFlash.SetFlashColor(Color.gray);
        if (Mathf.Approximately(DamageGiven, 0f))
        {
            if (health > 0)
            {
                float frequency = Mathf.PI * 20f;
                DamageFlash.SetFlashAmount(0.15f + 0.15f * Mathf.Sin(Time.time * frequency));
            }
        }
    }

    public void ChooseFlash(Color color, float multiplier, float amplitude)
    {
        amplitude = Mathf.Clamp(amplitude, 0, 1f);
        CustomFlash.SetFlashColor(color);
        if (Mathf.Approximately(DamageGiven, 0f))
        {
            if (health > 0)
            {
                float frequency = Mathf.PI * multiplier;
                CustomFlash.SetFlashAmount(amplitude + amplitude * Mathf.Cos(Time.unscaledTime * frequency));
            }
        }
    }

    public void TakeDamage(float amount)
    {
        if (CompareTag("Player"))
        {
            if (shield > 0f && guardUp && (Movement.currAttackState == AttackState.DefensiveBlocking || Movement.currAttackState == AttackState.DefensiveDodge))
            {
                DamageFlash.SetFlashColor(Color.white);
                Debug.Log("Flash triggered with color: White Successfully");
                ShieldMatFlash.DmgFlash(0.5f, 0.9f);
                shieldBarComponent.color = new Color(shieldBarComponent.color.r,shieldBarComponent.color.b,shieldBarComponent.color.g,1f);
                ShieldMatFlash.SetFlashColor(Color.white);
            }
            else
            {
                HealthMatFlash.DmgFlash(0.5f, 0.9f);
                HealthMatFlash.SetFlashColor(Color.white);
                DamageFlash.SetFlashColor(Color.red);
                Debug.Log($"Flash triggered with color: Red Successfully");
            }
            DamageFlash.DmgFlash();
            
        }
        else if (!CompareTag("Player"))
        {
            if (shield > 0f)
            {
                DamageFlash.SetFlashColor(Color.white);
                ShieldMatFlash.DmgFlash(0.5f, 0.9f);
            }
            else
                DamageFlash.SetFlashColor(Color.red);
            DamageFlash.DmgFlash();
            ShieldMatFlash.SetFlashColor(Color.white);
            shieldBarComponent.color = new Color(shieldBarComponent.color.r,shieldBarComponent.color.b,shieldBarComponent.color.g,1f);
        }
        DamageGiven += amount;
        totalDmgInflicted += amount;
        DamageFlash.duration = amount * 0.09f;
        ShieldMatFlash.duration = amount * 0.09f;
        if (GetComponent<StrongCharge>() != null)
        {
            StrongCharge.AddCharge(amount * 0.4f);
        }
    }

    private void SlowDownTime(float timeScale, float duration)
    {
        if (slowRoutiune != null)
        {
            StopCoroutine(slowRoutiune);
        }
        slowRoutiune = StartCoroutine(ShieldBreakSlow(timeScale, duration));
    }
    private IEnumerator ShieldBreakSlow(float timeScale, float duration)
    {
        Debug.Log("SetTrue");
        Time.timeScale = timeScale;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1;
    }
}