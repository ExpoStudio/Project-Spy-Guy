using UnityEngine;
using UnityEngine.InputSystem;
using System;
/// <summary>
/// This script handles the character selection system for a player in a game. It manages the player's state, character selection, and input handling during the character selection phase.
/// </summary>
public enum MoveDirection
{
    Left,
    Right
}

public class CharacterSelectionSystem : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // If the character state is NOT IN_GAME, the player prefab (childed object) will be disabled 
    // and the character selection UI, handled from another script, will be enabled
    // Note: Implementation of the other character playerPhase states is not necessarily implemented or required in this script.

    public event Action<PlayerGameState> OnGameStateChange; // Event to notify subscribers of game state changes
    public event Action<int, int> OnSelectionChanged;
    public event Action<MoveDirection, int> SelectionDirection; // Event to notify subscribers of selection changes
    public event Action<int> playerDestroyed;

    public Inventory assignedInventory; // Reference to the player's inventory, if applicable

    [SerializeField] private PlayerGameState _playerPhase = PlayerGameState.EXISTING_BUT_NOT_JOINED;
    public PlayerGameState PlayerPhase
    {
        get => _playerPhase;
        set
        {
            if (_playerPhase != value)
            {
                _playerPhase = value;
                OnGameStateChange?.Invoke(_playerPhase); // Notify subscribers of game state changes
            }
        }
    } // Property to access playerPhase from other scripts
    public CharacterChoice playerCharacter = CharacterChoice.NONE;

    private GameControlsManager inputHandler; // For Tracking how many players in game and their characters
    private PlayerInput _playerInput; // Referencing this particular player

    private float _actionTimer = 0f; // Backing field for actionTimer property
    public float ActionTimer 
    { 
        get => _actionTimer;
        set => _actionTimer = Mathf.Clamp(value, 0, actionTimerMax);
    } // Timer for character selection phase

    private int _selectionNumber = 0; // Backing field for selectionNumber property
    // Array of all character choices for selection
    public int SelectionNumber 
    { 
        get => _selectionNumber; 
        set 
        {
            int index = Mathf.Clamp(value, 0, CharacterOrder.Length - 1);
            _selectionNumber = index;

            var choice = CharacterOrder[index];
            if (IsCharacterAvailable(choice))
            {
                playerCharacter = choice;
                ResetActionTimer();
            }
            else
            {
                // Optional: wrap around or skip ahead
                _selectionNumber = (index + 1) % CharacterOrder.Length;
            }
            // Notify subscribers of selection change
            OnSelectionChanged?.Invoke(_selectionNumber, _playerInput.playerIndex); // Notify subscribers of selection change
        }
    } // Selection index for UI elements

    private bool IsCharacterAvailable(CharacterChoice testCharacter) => GameStateManager.IsCharacterAvailable(testCharacter); // Check if the character is available for selection
    public static readonly CharacterChoice[] CharacterOrder = new[]
    {
        CharacterChoice.SPY_GUY,
        CharacterChoice.SHADY_LADY,
        CharacterChoice.PERRSON,
        CharacterChoice.STICKFIGURE,
    }; 
    readonly float actionTimerMax = 10f; // Max time for character selection phase
    void Awake()
    {
        // This system is parented and part of the player prefab relative to each player, code everything relatively to the player prefab
        // Set the player phase to CHARACTER_SELECTION Upon instantiation (joining)
        // Other scripts will read the player phase to handle the appearance of UI elements and character choice to determine the state of the player prefab and UI

        PlayerPhase = PlayerGameState.EXISTING_BUT_NOT_JOINED;
        playerCharacter = CharacterChoice.NONE;
        _playerInput = GetComponent<PlayerInput>();
        inputHandler = GetComponent<GameControlsManager>();
        assignedInventory = GetComponentInChildren<Inventory>(); // Get the player's inventory component, if applicable
        // Player prefab will be disabled upon instantiation, and the character selection UI will be enabled (handled in another script)

    }

    public float ActionCooldown = 0.5f; // Cooldown time for join game action
    private bool ActionIsReady => ActionTimer > ActionCooldown && ActionTimer < actionTimerMax; 
    private bool LeftAction => inputHandler.MoveInput.x < 0 && ActionIsReady; // Check if the player is ready to select a character
    private bool RightAction => inputHandler.MoveInput.x > 0 && ActionIsReady; // Check if the player is ready to select a character
    // Update is called once per frame

    void Update()
    {
        //Have an action timer to prevent speeding through the character selection phase and UI elements

        switch (PlayerPhase)
        {
            case PlayerGameState.EXISTING_BUT_NOT_JOINED:
                ActionTimer += Time.deltaTime;
                // Handle existing but not joined logic here
                // Prompt press join button to join game, if not pressed in timely manner, disable the player prefab and destroy it
                // For example, if the player presses the join button, set the player phase to CHARACTER_SELECTION
                float joinGameActionCooldown = 0.5f; // Cooldown time for join game action
                if (inputHandler.JoinGameTriggered && ActionTimer > joinGameActionCooldown)
                {
                    // Player pressed the join button, set the player phase to CHARACTER_SELECTION
                    PlayerPhase = PlayerGameState.CHARACTER_SELECTION;
                    ResetActionTimer(); // Reset the action timer for character selection phase
                    Debug.Log("Player joined the game. Transitioning to CHARACTER_SELECTION phase.");
                    
                }
                else if (ActionTimer >= actionTimerMax)
                {
                    // Disable the player prefab and destroy it if the player has not pressed the join button in time
                    // This is a placeholder for the actual logic to disable and destroy the player prefab
                    Debug.Log("Action timer expired. Destroying player instance.");
                    ResetActionTimer(); // Reset the action timer before destroying the player prefab
                    playerDestroyed?.Invoke(_playerInput.playerIndex); // Notify subscribers of player destruction
                    Destroy(_playerInput.gameObject);
                    return;
                }
                break;
            case PlayerGameState.CHARACTER_SELECTION:
                ActionTimer += Time.deltaTime;

                switch (playerCharacter)
                {
                    case CharacterChoice.NONE:
                        // Handle character selection logic here
                        // For example, if the player selects a character, set the playerCharacter variable and change the player phase
                        GameStateManager.FindNextAvailableCharacter(ref playerCharacter);
                        break;
                    default:
                        // Handle character selection logic here
                        // For example, if the player selects a character, set the playerCharacter variable and change the player phase
                        if (LeftAction)
                        {
                            SelectionNumber -= 1; // Move left in the character selection UI
                            SelectionDirection?.Invoke(MoveDirection.Left, _playerInput.playerIndex);
                            
                            ResetActionTimer(); // Reset the action timer before destroying the player prefab
                        }
                        else if (RightAction)
                        {
                            SelectionNumber += 1; // Move right in the character selection UI
                            SelectionDirection?.Invoke(MoveDirection.Right, _playerInput.playerIndex);

                            ResetActionTimer(); // Reset the action timer before destroying the player prefab
                        }
                        else if (ActionTimer >= actionTimerMax)
                        {
                            // Disable the player prefab and destroy it if the player has not pressed the join button in time
                            // This is a placeholder for the actual logic to disable and destroy the player prefab
                            Debug.Log("Action timer expired. Destroying player instance.");
                            ResetActionTimer(); // Reset the action timer before destroying the player prefab
                            playerDestroyed?.Invoke(_playerInput.playerIndex); // Notify subscribers of player destruction
                            Destroy(_playerInput.gameObject);
                        }

                        if (inputHandler.attackTriggered && ActionIsReady && ActionTimer <= actionTimerMax)
                        {
                            // Player pressed the attack button, mark the character as unavailable for selection and confirm entry in game
                            GameStateManager.MarkCharacterAsUnavailable(playerCharacter);
                            Debug.Log($"Character {playerCharacter} marked as unavailable. Successfully chosen");
                            PlayerPhase = GameStateManager.Instance.gameState; // Set the player phase equal to the game state
                            if (GameStateManager.Instance.gameState == PlayerGameState.IN_GAME) transform.position = GameObject.Find("PlayerJoinSpawnPoint").transform.position;
                            // Player prefab will be enabled and the character selection UI will be disabled (handled in another script)
                            transform.GetChild(0).transform.gameObject.SetActive(true); // Enable the player prefab
                            ResetActionTimer(); // Reset the action timer for in-game phase
                        }
                        break;
                }
                // Find the next available character for selection

                // Handle character selection logic here
                // For example, if the player selects a character, set the characterChoice variable and change the player phase
                break;
        }
    }

    private void ResetActionTimer()
    {
        ActionTimer = 0f;
        Debug.Log("Action timer reset.");
    }
}
