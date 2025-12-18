using System.Collections.Generic;
using UnityEngine;

public class UIRegistry : MonoBehaviour
{
    public static UIRegistry Instance { get; private set; }

    private Dictionary<int, PlayerUIElements> _playerUIRegistry = new();
    private Dictionary<int, UICharacterSelectManager> _characterSelectUIRegistry = new();

    private void Awake()
    {
        Debug.Log("[UIRegistry] Awake called.");
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[UIRegistry] Duplicate instance detected, destroying this instance.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("[UIRegistry] Instance set and marked as DontDestroyOnLoad.");
    }

    // Register health and character select UI elements
    public void RegisterPlayerUIElements(int playerIndex, PlayerUIElements elements)
    {
        Debug.Log($"[UIRegistry] RegisterPlayerUIElements called for playerIndex={playerIndex}, elements={elements}");
        _playerUIRegistry[playerIndex] = elements;
    }

    public bool TryGetPlayerUIElements(int playerIndex, out PlayerUIElements elements)
    {
        bool found = _playerUIRegistry.TryGetValue(playerIndex, out elements);
        Debug.Log($"[UIRegistry] TryGetPlayerUIElements called for playerIndex={playerIndex}, found={found}, elements={elements}");
        return found;
    }

    // Register character select UI logic manager (UICharacterSelectManager)
    public void RegisterCharacterSelectUI(int playerIndex, UICharacterSelectManager manager)
    {
        Debug.Log($"[UIRegistry] RegisterCharacterSelectUI called for playerIndex={playerIndex}, manager={manager}");
        _characterSelectUIRegistry[playerIndex] = manager;
    }

    public bool TryGetCharacterSelectUI(int playerIndex, out UICharacterSelectManager manager)
    {
        bool found = _characterSelectUIRegistry.TryGetValue(playerIndex, out manager);
        Debug.Log($"[UIRegistry] TryGetCharacterSelectUI called for playerIndex={playerIndex}, found={found}, manager={manager}");
        return found;
    }

    public void Clear()
    {
        Debug.Log("[UIRegistry] Clear called. Clearing all registries.");
        _playerUIRegistry.Clear();
        _characterSelectUIRegistry.Clear();
    }
}
