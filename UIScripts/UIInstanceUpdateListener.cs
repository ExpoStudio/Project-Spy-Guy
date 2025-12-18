using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class UICharacterListener : MonoBehaviour
{
    [Range(0, 3)]
    [SerializeField] private int _assignedIndex;
    [SerializeField] private CanvasGroup _CharacterSelectCanvasGroup;
    [SerializeField] private CanvasGroup _PlayerHUDCanvasGroup;
    public PlayerInput _playerInput;
    private CharacterSelectionSystem _characterSelectionSystem;
    public float MaximumAlpha = 1f;

    private void OnEnable()
    {
        if (PlayerInputManager.instance == null)
        {
            return;
        }
        PlayerInputManager.instance.onPlayerJoined += OnPlayerJoined;
        PlayerInputManager.instance.onPlayerLeft += OnPlayerLeft;
    }

    private float GetCharacterSelectAlpha(PlayerGameState playerPhase)
    {
        return playerPhase switch
        {
            PlayerGameState.CHARACTER_SELECTION => 1f* MaximumAlpha,
            PlayerGameState.IN_GAME => 0f* MaximumAlpha,
            _ => _CharacterSelectCanvasGroup.alpha,
        };
    }

    private float GetHUDAlpha(PlayerGameState playerPhase)
    {
        return playerPhase switch
        {
            PlayerGameState.CHARACTER_SELECTION => 0f * MaximumAlpha,
            PlayerGameState.IN_GAME => 1f * MaximumAlpha,
            _ => _PlayerHUDCanvasGroup.alpha,
        };
    }

    // Update is called once per frame
    void Update()
    {
        // Early returns here prevent unnecessary checks and code execution if not needed.
        if (_playerInput == null || _characterSelectionSystem == null)
        {
            return; 
        }
        if (_playerInput.playerIndex != _assignedIndex) return;

        float t = 5f * Time.deltaTime;
        float targetCharAlpha = GetCharacterSelectAlpha(_characterSelectionSystem.PlayerPhase);
        float targetHUDAlpha = GetHUDAlpha(_characterSelectionSystem.PlayerPhase);

        if (!Mathf.Approximately(_CharacterSelectCanvasGroup.alpha, targetCharAlpha))
        {
            _CharacterSelectCanvasGroup.alpha = Mathf.MoveTowards(_CharacterSelectCanvasGroup.alpha, targetCharAlpha, t);
        }

        if (!Mathf.Approximately(_PlayerHUDCanvasGroup.alpha, targetHUDAlpha))
        {
            _PlayerHUDCanvasGroup.alpha = Mathf.MoveTowards(_PlayerHUDCanvasGroup.alpha, targetHUDAlpha, t);
        }
        

        _CharacterSelectCanvasGroup.interactable = _CharacterSelectCanvasGroup.alpha > (0.95f * MaximumAlpha);
        _CharacterSelectCanvasGroup.blocksRaycasts = _CharacterSelectCanvasGroup.alpha > (0.05f * MaximumAlpha);
        _PlayerHUDCanvasGroup.interactable = _PlayerHUDCanvasGroup.alpha > (0.95f * MaximumAlpha);
        _PlayerHUDCanvasGroup.blocksRaycasts = _PlayerHUDCanvasGroup.alpha > (0.05f * MaximumAlpha);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        MaximumAlpha = 0.45f;
    }
    void OnTriggerExit2D(Collider2D other)
    {
        MaximumAlpha = 1f;
    }

    private void OnPlayerJoined(PlayerInput playerInput)
    {
        //Check if the playerInput is the one we are looking for
        if (playerInput.playerIndex == _assignedIndex)
        {
            _playerInput = playerInput;
            _characterSelectionSystem = playerInput.transform.GetComponent<CharacterSelectionSystem>();
            //Unsubscribe from the event
            PlayerInputManager.instance.onPlayerJoined -= OnPlayerJoined;
        }
        else
        {
            return;
        }
    }

    private void OnPlayerLeft(PlayerInput playerInput)
    {
        //Check if the playerInput is the one we are looking for
        if (playerInput.playerIndex == _assignedIndex)
        {
            _playerInput = null;
            _characterSelectionSystem = null;
            //Unsubscribe from the event
            PlayerInputManager.instance.onPlayerLeft -= OnPlayerLeft;
            PlayerInputManager.instance.onPlayerJoined -= OnPlayerJoined;
        }
        else
        {
            return;
        }
    }

    private void OnDestroy()
    {
        if (PlayerInputManager.instance != null)
        {
            PlayerInputManager.instance.onPlayerJoined -= OnPlayerJoined;
            PlayerInputManager.instance.onPlayerLeft -= OnPlayerLeft;
        }
    }
}
