using UnityEngine;
using UnityEngine.InputSystem;
public class GameControlsManager : MonoBehaviour
{
    [Header("Input Action Asset")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private InputActionAsset playerControls;

    [Header("Action map Name Refrences")]
    [SerializeField] private string actionMapName = "Player";

    [Header("String Refrences")]
    [SerializeField] private string move = "Move";
    [SerializeField] private string jump = "Jump";
    [SerializeField] private string sprint = "Sprint";
    [SerializeField] private string attack = "Attack";
    [SerializeField] private string strong = "Strong";
    [SerializeField] private string strongAxis = "StrongAxis";
    [SerializeField] private string block = "Block";
    [SerializeField] private string parry = "Parry";
    [SerializeField] private string joinGame = "JoinGame";
    [SerializeField] private string openInventory = "OpenInventory";
    [SerializeField] private string throwUseItem = "throwUseItem";
    [SerializeField] private string dropItem = "dropItem";
    
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction sprintAction;
    private InputAction attackAction;
    private InputAction strongAction;
    private InputAction strongAxisAction;
    private InputAction blockAction;
    private InputAction parryAction;
    private InputAction joinGameAction;
    private InputAction openInventoryAction;
    private InputAction throwUseItemAction;
    private InputAction dropItemAction;


    public Vector2 MoveInput { get; private set; }
    public bool JumpTriggered { get; private set; }
    public bool sprintTriggered { get; private set; }
    public bool attackTriggered { get; private set; }
    public bool blockTriggered { get; private set; }
    public bool parryTriggered { get; private set; }
    public bool strongTriggered { get; private set; }
    public Vector2 strongAxisTriggered { get; private set; }
    public bool JoinGameTriggered { get; private set; }
    public bool openInventoryTriggered { get; private set; }
    public bool throwUseItemTriggered { get; private set; }
    private bool isDropItemHeld;
    private float _dropItemTriggered;
    public float dropItemTriggered { get => _dropItemTriggered; private set { _dropItemTriggered = Mathf.Clamp(value, 0, 3f); } }
    public bool CanDropItem => dropItemTriggered >= 2f && isDropItemHeld;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        playerControls = playerInput.actions;
        moveAction = playerControls.FindActionMap(actionMapName).FindAction(move);
        jumpAction = playerControls.FindActionMap(actionMapName).FindAction(jump);
        sprintAction = playerControls.FindActionMap(actionMapName).FindAction(sprint);
        attackAction = playerControls.FindActionMap(actionMapName).FindAction(attack);
        strongAction = playerControls.FindActionMap(actionMapName).FindAction(strong);
        strongAxisAction = playerControls.FindActionMap(actionMapName).FindAction(strongAxis);
        blockAction = playerControls.FindActionMap(actionMapName).FindAction(block);
        parryAction = playerControls.FindActionMap(actionMapName).FindAction(parry);
        joinGameAction = playerControls.FindActionMap(actionMapName).FindAction(joinGame);
        openInventoryAction = playerControls.FindActionMap(actionMapName).FindAction(openInventory);
        throwUseItemAction = playerControls.FindActionMap(actionMapName).FindAction(throwUseItem);
        dropItemAction = playerControls.FindActionMap(actionMapName).FindAction(dropItem);
        RegisterInputs();
    }

    void RegisterInputs()
    {
        moveAction.performed += context => MoveInput = context.ReadValue<Vector2>();
        moveAction.canceled += context => MoveInput = Vector2.zero;

        strongAxisAction.performed += context => strongAxisTriggered = context.ReadValue<Vector2>();
        strongAxisAction.canceled += context => strongAxisTriggered = Vector2.zero;

        jumpAction.performed += context => JumpTriggered = true;
        jumpAction.canceled += context => JumpTriggered = false;

        sprintAction.performed += context => sprintTriggered = true;
        sprintAction.canceled += context => sprintTriggered = false;

        attackAction.performed += context => attackTriggered = true;
        attackAction.canceled += context => attackTriggered = false;

        strongAction.performed += context => strongTriggered = true;
        strongAction.canceled += context => strongTriggered = false;

        blockAction.performed += context => blockTriggered = true;
        blockAction.canceled += context => blockTriggered = false;

        parryAction.performed += context => parryTriggered = true;
        parryAction.canceled += context => parryTriggered = false;

        joinGameAction.performed += context => JoinGameTriggered = true;
        joinGameAction.canceled += context => JoinGameTriggered = false;

        openInventoryAction.performed += context => {
            openInventoryTriggered = true;
            Debug.Log("Open Inventory Triggered");
        };
        openInventoryAction.canceled += context => openInventoryTriggered = false;

        throwUseItemAction.performed += context => throwUseItemTriggered = true;
        throwUseItemAction.canceled += context => throwUseItemTriggered = false;
        
        dropItemAction.performed += context => isDropItemHeld = true;
        dropItemAction.canceled += context => { isDropItemHeld = false; dropItemTriggered = 0f; };
}


    private void OnEnable()
    {
        moveAction.Enable();
        jumpAction.Enable();
        sprintAction.Enable();
        attackAction.Enable();
        blockAction.Enable();
        parryAction.Enable();
        strongAction.Enable();
        strongAxisAction.Enable();
        joinGameAction.Enable();
        openInventoryAction.Enable();
        throwUseItemAction.Enable();
        dropItemAction.Enable();
        dropItemTriggered = 0f;
    }

    private void OnDisable()
    {
        moveAction.Disable();
        jumpAction.Disable();
        sprintAction.Disable();
        attackAction.Disable();
        blockAction.Disable();
        parryAction.Disable();
        strongAction.Disable();
        strongAxisAction.Disable();
        joinGameAction.Disable();
        openInventoryAction.Disable();
        throwUseItemAction.Disable();
        dropItemAction.Disable();
        dropItemTriggered = 0f;
    }

    private void Update()
    {
        if (isDropItemHeld)
        {
            dropItemTriggered += Time.deltaTime;
        }
    }
}
