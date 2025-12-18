using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerDisplayTag : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private CharacterSelectionSystem characterSelectionSystem;
    [SerializeField] private TMP_Text playerTag;
    [SerializeField] private Canvas playerTagCanvas;
    [SerializeField] private Movement2 movement;
    [SerializeField] bool isInitialized = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Initialize()
    {
        characterSelectionSystem = playerInput.GetComponentInParent<CharacterSelectionSystem>();
        playerTag.alpha = 0f;
        movement = playerInput.transform.GetComponentInChildren<Movement2>();
        isInitialized = true;
    }
    private void Update()
    {
        if (!isInitialized)
        {
            Initialize();
            return;
        }
        UpdateTagVisibility(characterSelectionSystem.PlayerPhase);
    }

    private void UpdateTagVisibility(PlayerGameState state)
    {
        if (state == PlayerGameState.IN_GAME)
        {
            UpdateVisibility();
            playerTag.alpha = 1f;
        }
        else
        {
            playerTag.alpha = 0f;
        }
    }
    private void UpdateVisibility()
    {
        string setText = $"{GetCharacterName(characterSelectionSystem.playerCharacter)} Player {playerInput.playerIndex + 1}: Velocity: {movement.RigBod.linearVelocity.magnitude:F2} m/s";
        //Make sure the text does not mirror when the player is facing left
        if(playerInput.transform.localScale.z == -1) playerTag.rectTransform.localScale = new Vector3(1f, 1f, -1f);
        else playerTag.rectTransform.localScale = new Vector3(1f, 1f, 1f);

        playerTag.text = setText;
    }

    private string GetCharacterName(CharacterChoice characterChoice)
    {
        return characterChoice switch
        {
            CharacterChoice.SPY_GUY => "Spy Guy",
            CharacterChoice.SHADY_LADY => "Shady Lady",
            CharacterChoice.PERRSON => "Perrson",
            CharacterChoice.STICKFIGURE => "Stick Figure",
            _ => "Unknown Character"
        };
    }
}
