using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public interface INotifyGameStateChange
{
    event Action<PlayerGameState> OnGameStateChange;
}

public enum PlayerGameState
{
    INITIALIZE,
    EXISTING_BUT_NOT_JOINED,
    CHARACTER_SELECTION,
    IN_GAME,
    IN_LOBBY,
    IN_PAUSE,
    IN_CUTSCENE,
    IN_DIALOGUE,
    IN_MISSION_COMPLETE,
    GAME_OVER,
    DOWNED,
    DEAD
}

[Flags]
public enum CharacterChoice
{
    NONE = 0,
    SPY_GUY = 1 << 0,
    SHADY_LADY = 1 << 1,
    PERRSON = 1 << 2,
    STICKFIGURE = 1 << 3,
}
/// <summary>
/// Keeps track of the state the game is in (i.e. In lobby or ), manages transitions between different states and tracks used characters.
/// </summary>
public class GameStateManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // This will keep track of the game state and manage transitions between different states
    // This will be a singleton instance and a dont destroy on load object
    public CharacterChoice availableCharacters = CharacterChoice.NONE;
    public PlayerGameState gameState = PlayerGameState.INITIALIZE; // Default state is EXISTING_BUT_NOT_JOINED
    public bool InGame => Instance.gameState is PlayerGameState.IN_GAME or PlayerGameState.IN_LOBBY;

    private enum GameOverState
    {
        None,
        Slowdown,
        SummonMenu,
        SelectOptions,
    }
    internal bool trackGameOver;
    public event Action OnGameOver;
    public List<Health> AlivePlayers;
    public int AlivePlayerCount => (Instance != null && Instance.InGame && CameraPositionController.Instance != null) ?
        CameraPositionController.Instance.playerInputs.Values.Count(p =>
                p != null
                && p.transform != null
                && p.transform.TryGetComponent<CharacterSelectionSystem>(out var cs)
                && cs.PlayerPhase is not (PlayerGameState.DEAD)
        ) : 1;
    bool GameOver => trackGameOver && InGame && AlivePlayerCount == 0;
    GameOverState gameOverState = GameOverState.None;
    public Volume PostProcessVolume;
    private float _timer;
    float Timer { get => _timer; set => Mathf.Clamp(value, 0, 4f); }
    void Update()
    {
        // GameOver Triggering
        float t = Time.unscaledDeltaTime;
        if (GameOver)
        {
            switch (gameOverState)
            {
                case GameOverState.Slowdown:
                    {
                        Time.timeScale = Mathf.MoveTowards(Time.timeScale, 0, t * 3f);
                        float tsc = Time.timeScale;
                        Timer += t;
                        if (Timer > 3f && tsc < 0.1f)
                        {
                            Time.timeScale = 0;
                            Timer = 0;
                            gameOverState = GameOverState.SummonMenu;
                        }
                    }
                    break;
                case GameOverState.SummonMenu:
                    {

                    }
                    break;
                default:
                    if (GameOver)
                    {
                        gameState = PlayerGameState.GAME_OVER;
                    }
                    break;
            }
        }
    }

    public static GameStateManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        SceneManager.sceneLoaded += (scene, mode) =>
        {
            PostProcessVolume = GameObject.Find("PostProcessVideo").GetComponent<Volume>();
        };
    }
    void Start()
    {
        gameState = PlayerGameState.IN_GAME;
    }


    public static void MarkCharacterAsUnavailable(CharacterChoice characterChoice)
    {
        // Mark the character as unavailable for selection
        // This will be handled in the GameControlsManager script, but this is a placeholder for the actual logic
        if (!Instance.availableCharacters.HasFlag(characterChoice)) Instance.availableCharacters |= characterChoice; // Add the character to the available choices
        Debug.Log($"Character {characterChoice} marked as unavailable.");
    }
    public static void MarkCharacterAsAvailable(CharacterChoice characterChoice)
    {
        // Mark the character as available for selection
        // This will be handled in the GameControlsManager script, but this is a placeholder for the actual logic
        Instance.availableCharacters &= ~characterChoice; // Remove the character from the available choices
        Debug.Log($"Character {characterChoice} marked as available.");
    }
    public static bool IsCharacterAvailable(CharacterChoice characterChoice) => !Instance.availableCharacters.HasFlag(characterChoice);
    
    public static bool IsCharacterUnavailable(CharacterChoice characterChoice) => Instance.availableCharacters.HasFlag(characterChoice);
    public static void FindNextAvailableCharacter(ref CharacterChoice playerCharacter)
    {
        // Find the next available character for selection from current player selection
        foreach (CharacterChoice characterChoice in Enum.GetValues(typeof(CharacterChoice)))
        {
            if (characterChoice != CharacterChoice.NONE && IsCharacterAvailable(characterChoice))
            {
                playerCharacter = characterChoice; // Set the character choice to the next available character
                Debug.Log($"Next available character: {characterChoice}");
                return;
            }
        }
        Debug.Log("No available characters found.");
    }
    public static void ResetCharacterChoices()
    {
        // Reset the character choices to available
        Instance.availableCharacters = CharacterChoice.NONE;
        Debug.Log("Character choices reset to available.");
    }
}
