using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class StrongCharge : MonoBehaviour
{
    public float strongCharge = 0;
    public float damageGiven = 0f;
    public float damage;
    private float OgScalex;
    private float OgScaley;
    private bool isInvokingInc;
    [SerializeField] private StrongFullFlash StrongFlash;
    [SerializeField] private StrongFullFlashImage StrongFlashImage;
    [SerializeField] private Attacks attacks;

    [SerializeField] private GameObject _assignedUI;
    [SerializeField] public RectTransform StrongBar;
    [SerializeField] private Image StrongBarImage;
    private PlayerInput _playerInput;
    private CharacterSelectionSystem _characterSelectionSystem;
    private int playerIndex;
    [SerializeField] private PlayerInputManager _playerInputManager;


    private bool _isAssigned = false;
    private bool fullCharge;
    [HideInInspector] public bool isCharging;


    private bool AvailablePlayerState => _characterSelectionSystem.PlayerPhase != PlayerGameState.EXISTING_BUT_NOT_JOINED && _characterSelectionSystem.PlayerPhase != PlayerGameState.CHARACTER_SELECTION;
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

    void AssignUIElements()
    {
        _playerInput = transform.GetComponentInParent<PlayerInput>();

        if (_playerInput == null)
        {
            Debug.LogError("PlayerInput component missing!");
            return;
        }

        playerIndex = _playerInput.playerIndex;
        Debug.Log($"Player Index: {playerIndex}");

        _characterSelectionSystem = _playerInput.transform.GetComponentInParent<CharacterSelectionSystem>();
        _assignedUI = GameObject.Find($"Player{playerIndex}HUD");
        StrongBar = (RectTransform)_assignedUI.transform.GetChild(8).transform;
        StrongBarImage = _assignedUI.transform.GetChild(8).GetChild(0).GetComponent<Image>();
        StrongFlashImage = _assignedUI.transform.GetChild(8).transform.GetChild(0).GetComponent<StrongFullFlashImage>();

        OgScalex = StrongBar.transform.localScale.x;
        OgScaley = StrongBar.transform.localScale.y;

        if (StrongBarImage == null)
        {
            Debug.LogError("StrongBarImage is null!");
            return;
        }
        if (StrongBar == null)
        {
            Debug.LogError("StrongBar is null!");
            return;
        }
        if (StrongFlashImage == null)
        {
            Debug.LogError("StrongFlashImage is null!");
            return;
        }
        if (_characterSelectionSystem == null)
        {
            Debug.LogError("CharacterSelectionSystem is null!");
            return;
        }
        if (attacks == null)
        {
            Debug.LogError("Attacks is null!");
            return;
        }

        _isAssigned = true;

    }
    void Update()
    {
        if (!_isAssigned)
        {
            AssignUIElements();
            return;
            
        }

        if (strongCharge < 100 && attacks.attackMove != AttackMove.strongGround)
        {
            if (!isInvokingInc)
            {
                InvokeRepeating(nameof(GraduallyInc), 4f, 1f);
                isInvokingInc = true;
            }
        }
        else
        {
            CancelInvoke(nameof(GraduallyInc));
            isInvokingInc = false;
        }

        if (strongCharge == 100f && !fullCharge && attacks.attackMove != AttackMove.strongGround)
        {
            StrongFlash.SetFlashColor(Color.yellow);
            StrongFlash.DmgFlash(0.8f, 0.5f);

            StrongFlashImage.SetFlashColor(Color.white);
            StrongFlashImage.DmgFlash(0.8f, 0.5f);
            fullCharge = true;
        }

        if (attacks.attackMove == AttackMove.strongGround && attacks.strongPhase == StrongPhase.Charging)
        {
            StrongFlash.SetFlashColor(Color.white);
            StrongFlash.SetFlashAmount(0.3f + 0.3f * Mathf.Cos(Time.unscaledTime * 14f * Mathf.PI));

            StrongFlashImage.SetFlashColor(Color.white);
            StrongFlashImage.SetFlashAmount(0.3f + 0.3f * Mathf.Cos(Time.unscaledTime * 14f * Mathf.PI));
        }
        else if (attacks.attackMove == AttackMove.strongGround && attacks.strongPhase == StrongPhase.Canceled)
        {
            StrongFlash.SetFlashColor(Color.gray);
            StrongFlash.SetFlashAmount(0.3f + 0.3f * Mathf.Cos(Time.unscaledTime * 6f * Mathf.PI));

            StrongFlashImage.SetFlashColor(Color.gray);
            StrongFlashImage.SetFlashAmount(0.3f + 0.3f * Mathf.Cos(Time.unscaledTime * 6f * Mathf.PI));

            Invoke(nameof(SetFalse), 0.5f);
        }
        else if (attacks.attackMove == AttackMove.strongGround && attacks.strongPhase == StrongPhase.releaseReady)
        {
            isCharging = true;

            StrongFlash.SetFlashColor(Color.yellow);
            StrongFlash.SetFlashAmount(0.3f + 0.3f * Mathf.Cos(Time.time * 2f * Mathf.PI));

            StrongFlashImage.SetFlashColor(Color.white);
            StrongFlashImage.SetFlashAmount(0.3f + 0.3f * Mathf.Cos(Time.time * 5f * Mathf.PI));
        }
        else if (attacks.strongPhase != StrongPhase.releaseReady && isCharging)
        {
            StrongFlash.SetFlashColor(Color.white);
            StrongFlash.SetFlashAmount(0.3f + 0.3f * Mathf.Cos(Time.unscaledTime * 26f * Mathf.PI));

            StrongFlashImage.SetFlashColor(Color.white);
            StrongFlashImage.SetFlashAmount(0.3f + 0.3f * Mathf.Cos(Time.time * 5f * Mathf.PI));

            Invoke(nameof(SetFalse), 0.1f);
        }

        if (strongCharge < 100f && fullCharge && attacks.attackMove == AttackMove.strongGround && attacks.strongPhase == StrongPhase.Released)
        {
            fullCharge = false;
        }

        float damageToApply = Mathf.Min(damageGiven, 30f * Time.deltaTime);
        if (damageGiven > 0f && attacks.attackMove != AttackMove.strongGround)
        {
            strongCharge += damageToApply;
            damageGiven -= damageToApply;
            damage = 0;
        }

        strongCharge = Mathf.Clamp(strongCharge, 0f, 100f);
        damageGiven = Mathf.Clamp(damageGiven, 0f, 1000f);
        float ScaleCap = Mathf.Lerp(Mathf.Abs(StrongBar.localScale.x), Mathf.Abs(OgScalex * (strongCharge * 0.01f)), Time.deltaTime * 4f);
        StrongBar.transform.localScale = new Vector2(ScaleCap, OgScaley);
    }
    public void AddCharge(float amount)
    {
        damageGiven += amount;
    }
    private void GraduallyInc()
    {
        strongCharge += 1;
    }
    void SetFalse()
    {
        isCharging = false;
        StrongFlash.SetFlashColor(Color.yellow);
        StrongFlash.SetFlashAmount(0);

        StrongFlashImage.SetFlashColor(Color.white);
        StrongFlashImage.SetFlashAmount(0);
    }
}
