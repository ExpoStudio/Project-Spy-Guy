using UnityEngine;
using UnityEngine.InputSystem;

public class UICharacterSelectManager : MonoBehaviour
{
    [SerializeField] private GameObject[] characterSelectImages = new GameObject[4];
    [SerializeField] private GameObject characterSelectImageParent;
    [SerializeField] private GameObject LeftArrow;
    [SerializeField] private GameObject RightArrow;

    private CharacterSelectionSystem assignedPlayer;
    private int assignedPlayerIndex = 0;

    private void Awake()
    {
        Debug.Log("[UICharacterSelectManager] Awake called.");
        characterSelectImageParent.transform.localScale = Vector3.one;
        LeftArrow.transform.localScale = Vector3.one;
        RightArrow.transform.localScale = Vector3.one;
    }

    public void Setup(int playerIndex)
    {
        Debug.Log($"[UICharacterSelectManager] Setup called for playerIndex={playerIndex}");
        if (playerIndex < 0) return;

        assignedPlayerIndex = playerIndex;

        PlayerInput[] allInputs = FindObjectsByType<PlayerInput>(FindObjectsSortMode.None);
        foreach (var input in allInputs)
        {
            if (input.playerIndex == playerIndex &&
                input.TryGetComponent(out CharacterSelectionSystem selectionSystem))
            {
                assignedPlayer = selectionSystem;
                break;
            }
        }

        if (assignedPlayer == null)
        {
            Debug.LogWarning($"UICharacterSelectManager: No CharacterSelectionSystem found for player {playerIndex}");
            return;
        }

        assignedPlayer.OnSelectionChanged += SelectionChangedHandler;
        assignedPlayer.SelectionDirection += ArrowAnimationHandler;
        assignedPlayer.playerDestroyed += OnPlayerDestroyed;

        Debug.Log($"[UICharacterSelectManager] Registered event handlers for player {playerIndex}.");
        UIRegistry.Instance.RegisterCharacterSelectUI(playerIndex, this);
    }

    public void ShowCharacterSelectUI()
    {
        Debug.Log("[UICharacterSelectManager] ShowCharacterSelectUI called.");
        if (characterSelectImageParent == null) return;

        characterSelectImageParent.SetActive(true);
        LeanTween.cancel(characterSelectImageParent);
        characterSelectImageParent.transform.localScale = Vector3.zero;
        LeanTween.scale(characterSelectImageParent, Vector3.one, 0.25f).setEaseOutBack();
    }

    public void HideCharacterSelectUI()
    {
        Debug.Log("[UICharacterSelectManager] HideCharacterSelectUI called.");
        if (characterSelectImageParent == null) return;

        LeanTween.cancel(characterSelectImageParent);
        LeanTween.scale(characterSelectImageParent, Vector3.zero, 0.25f)
            .setEaseInBack()
            .setOnComplete(() => characterSelectImageParent.SetActive(false));
    }

    private void SelectionChangedHandler(int selectedCharacter, int playerIndex)
    {
        Debug.Log($"[UICharacterSelectManager] SelectionChangedHandler called. selectedCharacter={selectedCharacter}, playerIndex={playerIndex}");
        if (playerIndex != assignedPlayerIndex) return;

        for (int i = 0; i < characterSelectImages.Length; i++)
        {
            if (characterSelectImages[i] != null)
                characterSelectImages[i].SetActive(i == selectedCharacter);
        }

        LeanTween.cancel(characterSelectImageParent);
        characterSelectImageParent.transform.localScale = Vector3.one;
        LeanTween.scale(characterSelectImageParent, Vector3.one * 1.1f, 0.4f)
            .setEaseInOutSine()
            .setLoopPingPong();
    }

    private void ArrowAnimationHandler(MoveDirection direction, int playerIndex)
    {
        Debug.Log($"[UICharacterSelectManager] ArrowAnimationHandler called. direction={direction}, playerIndex={playerIndex}");
        if (playerIndex != assignedPlayerIndex) return;

        LeanTween.cancel(LeftArrow);
        LeanTween.cancel(RightArrow);

        switch (direction)
        {
            case MoveDirection.Left:
                LeanTween.scale(LeftArrow, Vector3.one * 1.2f, 0.5f).setEaseInOutSine().setLoopPingPong();
                LeanTween.scale(RightArrow, Vector3.one, 0.5f).setEaseInOutSine().setLoopPingPong();
                break;

            case MoveDirection.Right:
                LeanTween.scale(RightArrow, Vector3.one * 1.2f, 0.5f).setEaseInOutSine().setLoopPingPong();
                LeanTween.scale(LeftArrow, Vector3.one, 0.5f).setEaseInOutSine().setLoopPingPong();
                break;
        }
    }

    private void OnDisable()
    {
        Debug.Log("[UICharacterSelectManager] OnDisable called.");
        if (assignedPlayer != null)
        {
            assignedPlayer.OnSelectionChanged -= SelectionChangedHandler;
            assignedPlayer.SelectionDirection -= ArrowAnimationHandler;
            assignedPlayer.playerDestroyed -= OnPlayerDestroyed;
        }

        LeanTween.cancel(characterSelectImageParent);
        characterSelectImageParent.transform.localScale = Vector3.one;
        LeanTween.cancel(LeftArrow);
        LeftArrow.transform.localScale = Vector3.one;
        LeanTween.cancel(RightArrow);
        RightArrow.transform.localScale = Vector3.one;
    }

    private void OnDestroy()
    {
        Debug.Log("[UICharacterSelectManager] OnDestroy called.");
        OnDisable();
    }

    private void OnPlayerDestroyed(int playerIndex)
    {
        Debug.Log($"[UICharacterSelectManager] OnPlayerDestroyed called for playerIndex={playerIndex}");
        if (playerIndex != assignedPlayerIndex) return;

        Debug.Log($"Player {playerIndex} destroyed. Attempting to rebind...");

        PlayerInput[] allInputs = FindObjectsByType<PlayerInput>(FindObjectsSortMode.None);
        foreach (var input in allInputs)
        {
            if (input.playerIndex == playerIndex &&
                input.TryGetComponent(out CharacterSelectionSystem newSystem))
            {
                assignedPlayer = newSystem;
                assignedPlayer.OnSelectionChanged += SelectionChangedHandler;
                assignedPlayer.SelectionDirection += ArrowAnimationHandler;
                assignedPlayer.playerDestroyed += OnPlayerDestroyed;

                Debug.Log($"Player {playerIndex} re-bound to UICharacterSelectManager.");
                return;
            }
        }

        Debug.LogWarning($"Failed to rebind UICharacterSelectManager for player {playerIndex}");
    }
}
