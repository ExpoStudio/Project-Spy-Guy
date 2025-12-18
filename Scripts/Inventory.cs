using System;
using System.Collections.Generic;
using System.Linq;
using Items;
using UnityEngine;

public enum InventoryStatus
{
    Opening,
    Opened,
    Closing,
    Closed,
}
public class Inventory : MonoBehaviour
{
    public event Action<string> ItemAdded;
    public event Action<PlayerSlotHighlighted> OnSlotEquipped;
    public event Action SignalUpdates;
    [HideInInspector] public GameControlsManager _gameControlsManager;
    [SerializeField] private Movement2 _movement2;
    public List<ItemDefinition> itemDefinitions;
    public List<Slot> PlayerInventory;
    public Slot SelectedSlot;
    public Item EquippedItem;
    public ItemsDictionary ItemsDictionary;

    public InventoryStatus InventoryStatus = InventoryStatus.Closed;

    public enum PlayerSlotHighlighted
    {
        First,
        Second,
        Third,
        Armor,
        CaseObjective
    }

    public enum InventoryMoveAction
    {
        Left,
        Right,
        Up,
        Down
    }
    private event Action<InventoryMoveAction> OnMoveAction;
    public event Action<PlayerSlotHighlighted> NewSlotSelected;
    public event Action UpdateText;
    public event Action ItemRemoved;
    public event Action NewItemEquipped;

    private PlayerSlotHighlighted _playerSlotHighlighted;
    public PlayerSlotHighlighted SlotHighlighted
    {
        get => _playerSlotHighlighted;
        set
        {
            _playerSlotHighlighted = value;
            string slotName = value.ToString();
            int index = (int)_playerSlotHighlighted;
            if (Enum.IsDefined(typeof(PlayerSlotHighlighted), _playerSlotHighlighted) && index >= 0 && index < PlayerInventory.Count)
            {
                SelectedSlot = PlayerInventory[index];
                NewSlotSelected?.Invoke(value);
            }
            else
            {
                Debug.LogWarning($"Invalid index {index} for PlayerInventory. SlotHighlighted: {_playerSlotHighlighted}. Total slots in PlayerInventory: {PlayerInventory.Count}");
                SelectedSlot = FindFirstAvailableSlot(); // Handle invalid index gracefully
                if (SelectedSlot == null)
                {
                    Debug.LogError("No valid slot found in the inventory. SelectedSlot is set to null.");
                    Debug.Log($"SlotHighlighted set to {slotName} (Type: {SelectedSlot.Type}, Item: {SelectedSlot.Item?.Name ?? "None"})");
                    NewSlotSelected?.Invoke(value);
                }

                Debug.Log($"SlotHighlighted set to {slotName} ({SelectedSlot.Item?.Name ?? "None"})");
            }
        }
    }
    [Serializable]
    public enum SlotType
    {
        Main,
        Armor,
        CaseObjective
    }

    [Serializable]
    public class Slot
    {
        public SlotType Type;
        public Item Item;

        public Slot(SlotType type, Item item)
        {
            Type = type;
            Item = item;
        }
    }

    bool ArmorSlotOccupied => PlayerInventory.Find(slot => slot.Type == SlotType.Armor)?.Item != null;
    bool CaseObjectiveItemSlotOccupied => PlayerInventory.Find(slot => slot.Type == SlotType.CaseObjective)?.Item != null;
    private bool InventoryCapacityReached => SlotsOccupiedHelper();

    private bool SlotsOccupiedHelper()
    {
        int occupiedCount = 0;
        int maxInventorySize = PlayerInventory.Count;
        foreach (var item in PlayerInventory)
        {
            if (item.Item != null)
            {
                occupiedCount++;
            }
        }
        if (occupiedCount >= maxInventorySize)
        {
            Debug.LogWarning("Inventory is full. Cannot add more items.");
            return true;
        }
        else
        {
            return false;
        }
    }
    public Slot FindFirstAvailableSlot()
    {
        // Check if the slot is available (no item) and is not of type Armor or CaseObjective
        return PlayerInventory.FirstOrDefault(slot => slot is { Item: null, Type: not (SlotType.Armor or SlotType.CaseObjective) });
    }

    public Slot FindNextSlotWithItem()
    {
        int seekIndex = (int)_playerSlotHighlighted + 1;
        for (int i = 0; i < PlayerInventory.Count; i++)
        {
            int index = (seekIndex + i) % PlayerInventory.Count; // Wrap around the inventory
            if (PlayerInventory[index].Item != null)
            {
                _playerSlotHighlighted = (PlayerSlotHighlighted)index;
                SelectedSlot = PlayerInventory[index];
                OnSlotEquipped?.Invoke(_playerSlotHighlighted);
                return PlayerInventory[index]; // Return the first non-null slot found
            }
        }
        return null; // No non-null slot found
    }

    private float _actionTimer = 0f;
    public float ActionTimer { get => _actionTimer; set => _actionTimer = value; }
    private float _buttonHoldTimer = 0f;
    public float ButtonHoldTimer { get => _buttonHoldTimer; set => _buttonHoldTimer = Mathf.Clamp(value, 0, 3f); }
    public bool CanPerformAction => _actionTimer >= 0.25f;
    public float ActionDelay = 0.5f; // Delay in seconds before the next action can be performed

    void Awake()
    {
        _gameControlsManager = GetComponentInParent<GameControlsManager>();
        // Initialize personal inventory. Will be replaced later by save file's player-specific inventory.
        // For instance, if the player has a save file, it will load the inventory from that save file.
        // If the player does not have a save file, it will create a new inventory with default items.
        // The save file will load the inventory based on the player's ID or name. Player 2 recieves Player 2 inventory.


        List<Slot> slots = new()
        {
            new(SlotType.Main,
                ItemsDictionary.CommonItemDictionary[ItemsDictionary.Fist]
                ), // Default item
            new(SlotType.Main,
                ItemsDictionary.CommonItemDictionary[ItemsDictionary.Bottle]
                ), // Default throwable item
            new(SlotType.Main,
                ItemsDictionary.CommonItemDictionary[ItemsDictionary.Thingy]
                ), // Default miscellaneous item
            new(SlotType.Armor,
                null),
            new(SlotType.CaseObjective,
                null)
        };
        PlayerInventory = slots;
        SelectedSlot = PlayerInventory[0];
        EquippedItem = SelectedSlot.Item; // Set default selected item to the first item (Fist)
        _playerSlotHighlighted = PlayerSlotHighlighted.First; // Default highlighted slot

        OnMoveAction += (action) =>
        {
            switch (action)
            {
                case InventoryMoveAction.Left:
                    SlotHighlighted = (PlayerSlotHighlighted)(((int)SlotHighlighted - 1 + Enum.GetValues(typeof(PlayerSlotHighlighted)).Length) % Enum.GetValues(typeof(PlayerSlotHighlighted)).Length);
                    break;
                case InventoryMoveAction.Right:
                    SlotHighlighted = (PlayerSlotHighlighted)((Enum.GetValues(typeof(PlayerSlotHighlighted)).Length + ((int)SlotHighlighted + 1)) % Enum.GetValues(typeof(PlayerSlotHighlighted)).Length);
                    break;
                case InventoryMoveAction.Up:
                    // Handle Up action if needed
                    break;
                case InventoryMoveAction.Down:
                    // Handle Down action if needed
                    break;
            }
        };
    }

    public void Update()
    {

        float deltaTime = Time.deltaTime; // Store Time.deltaTime in a local variable

        // Controls
        switch (InventoryStatus)
        {
            case InventoryStatus.Closed:
                if (_gameControlsManager.openInventoryTriggered)
                {
                    InventoryStatus = InventoryStatus.Opening;
                    Debug.Log("Opening inventory...");
                    ActionTimer = 0;
                    _movement2.currAttackState = AttackState.InInventory;
                }
                break;
            case InventoryStatus.Opening:
                ActionTimer += deltaTime;
                if (ActionTimer >= ActionDelay)
                {
                    InventoryStatus = InventoryStatus.Opened;
                    Debug.Log("Inventory opened.");
                    ActionTimer = 0; // Reset action timer after opening
                }
                break;
            case InventoryStatus.Opened:
                ActionTimer += deltaTime;
                if (CanPerformAction)
                {
                    if (_gameControlsManager.MoveInput.x > 0.1f)
                    {
                        OnMoveAction?.Invoke(InventoryMoveAction.Right);
                        ActionTimer = 0;
                        ButtonHoldTimer = 0;
                    }
                    else if (_gameControlsManager.MoveInput.x < -0.1f)
                    {
                        OnMoveAction?.Invoke(InventoryMoveAction.Left);
                        ActionTimer = 0;
                        ButtonHoldTimer = 0;
                    }
                    else
                    {
                        ButtonHoldTimer = 0; // Reset only when stick is neutral
                    }
                }

                if (_gameControlsManager.CanDropItem && CanPerformAction)
                {
                    if (SelectedSlot.Item != null)
                    {
                        Debug.Log($"Selected item: {SelectedSlot.Item.Name}");
                        if (SelectedSlot.Item.CanDrop)
                        {
                            ActionTimer = 0; // Reset action timer after selecting an item
                            Debug.Log($"Dropping item: {SelectedSlot.Item.Name}");
                            Debug.Log($"Dropped item: {SelectedSlot.Item.Name}");
                            RemoveItem(SelectedSlot, SelectedSlot.Item, 1);
                        }
                        else
                        {
                            ActionTimer = 0;
                            Debug.LogWarning($"Cannot drop item: {EquippedItem.Name} (CanDrop is false)");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("No item selected.");
                    }
                }


                if (!_gameControlsManager.openInventoryTriggered && CanPerformAction)
                {
                    InventoryStatus = InventoryStatus.Closing;
                    if (SelectedSlot.Item != null && SelectedSlot.Type == SlotType.Main && SelectedSlot.Item != EquippedItem)
                    {
                        EquippedItem = SelectedSlot.Item;
                        NewItemEquipped?.Invoke();
                        NewSlotSelected?.Invoke(SlotHighlighted);
                    } // Set the equipped item to the currently selected item
                    Debug.Log("Closing inventory...");
                    ActionTimer = 0;
                }
                break;
            case InventoryStatus.Closing:
                ActionTimer += deltaTime;
                if (ActionTimer >= ActionDelay)
                {
                    InventoryStatus = InventoryStatus.Closed;
                    Debug.Log("Inventory closed.");
                    ActionTimer = 0; // Reset action timer after closing
                    _movement2.currAttackState = AttackState.notAttacking;
                    _movement2.canmove = true;
                    _movement2.canjump = false; // Allow movement again after closing the inventory
                }
                break;
        }
    }

    /// <summary>
    /// Adds an item to the inventory. Use new Item() to create an item.
    /// </summary>
    /// <remarks>
    /// - If the inventory is full, the item will not be added, and a warning will be logged.
    /// - If duplicates are not allowed and the item already exists, the item will not be added, and a warning will be logged.
    /// - If duplicates are allowed and the item is stackable, the item's value will be increased up to its maximum allowable value.
    /// - If duplicates are allowed but the item is not stackable, the item will be added as a new entry in the inventory.
    /// </remarks>
    public void AddItem(Slot slot, Item item)
    {
        bool nameExists = PlayerInventory.Exists(i => i.Item != null && i.Item.Name == item.Name);
        Item item1 = PlayerInventory.Find(i => i.Item != null && i.Item.Name == item.Name)?.Item;
        if (item == null)
        {
            Debug.LogWarning("Attempted to add a null item to the inventory.");
            return;
        }
        else if (InventoryCapacityReached)
        {
            Debug.LogWarning("Inventory is full. Cannot add more items.");
            return;
        }
        else if (item.Type == ItemType.Armor && ArmorSlotOccupied)
        {
            Debug.LogWarning("Cannot add more than one armor item.");
            return;
        }
        else if (item.Type == ItemType.CaseObjectiveItem && CaseObjectiveItemSlotOccupied)
        {
            Debug.LogWarning("Cannot add more than one Case Objective Item.");
            return;
        }
        else if (!item.DuplicatesAllowed && PlayerInventory.Exists(i => i.Item != null && i.Item.Name == item.Name))
        {
            var duplicateSlot = PlayerInventory.Find(i => i.Item != null && i.Item.Name == item.Name).Item;
            Debug.LogWarning($"Item '{item.Name}' already exists in the inventory in slot '{duplicateSlot?.Type}' and duplicates are not allowed.");
            return;
        }
        else if (item.DuplicatesAllowed && nameExists && item.IsStackable)
        {
            Debug.LogWarning($"Item '{item.Name}' already exists in the inventory but duplicates are allowed. Increasing Item amount.");
            var existingItem = item1; // Find the existing item in the inventory
            if (existingItem != null)
            {
                int newValue = existingItem.Value + item.Value;
                existingItem.Value = Mathf.Min(newValue, existingItem.MaxValue); // Ensure the value does not exceed its maximum allowable value
                Debug.Log($"Updated item '{existingItem.Name}' value to {existingItem.Value} (Max: {existingItem.MaxValue})");
            }
        }
        else if (item.DuplicatesAllowed && nameExists && !item.IsStackable)
        {
            Debug.LogWarning($"Item '{item.Name}' already exists in the inventory but duplicates are allowed. Adding as a new item.");
            slot.Item = item; // Add as a new item
        }
        else
        {
            slot.Item = item; // Add as a new item
            Debug.Log($"Added item: {item}");
            ItemAdded?.Invoke(item.Name);
        }
        UpdateText?.Invoke();
    }

    /// </summary>
    /// Removes a specific amount of an item from the inventory.
    /// If the item is not found, a warning is logged and no action is taken.
    /// If duplicates are allowed, the specified amount is removed, and the item is entirely removed if the amount exceeds or equals its current value.
    /// </summary>
    public void RemoveItem(Slot slot, Item item, int amount, bool findAnyItemInInventory = false)
    {
        if (slot.Item == null)
        {
            Debug.LogWarning("Attempted to remove an item from an empty slot.");
            return;
        }

        Item existing = null;
        Slot findAnyItem = PlayerInventory.Find(i => i.Item != null && i.Item.Name == item.Name);
        if (findAnyItem == null)
        {
            Debug.LogWarning($"No matching slot found for item: {item.Name}");
            return;
        }
        if (findAnyItemInInventory) existing = findAnyItem.Item;
        else existing = PlayerInventory[(int)SlotHighlighted].Item;

        if (existing == null)
        {
            Debug.LogWarning($"Item not found in inventory: {item}");
            return;
        }

        if (existing.DuplicatesAllowed && existing.IsStackable)
        {
            if (amount >= existing.Value)
            {
                existing = null;
                slot.Item = existing; // Remove the item entirely if the amount to remove is greater than or equal to the current value
                Debug.Log($"Removed all of item: {item}");
                if (EquippedItem == item)
                {
                    EquippedItem = null; // Clear the equipped item if it was removed
                    EquippedItem = FindNextSlotWithItem()?.Item;
                } // Clear the equipped item if it was removed
            }
            else
            {
                existing.Value -= amount;
                Debug.Log($"Removed {amount} of item: {item}. Remaining: {existing.Value}");
                if (existing.Value <= 0)
                {
                    existing = null;
                    slot.Item = existing;
                    if (EquippedItem == item)
                    {
                        EquippedItem = null; // Clear the equipped item if it was removed
                        EquippedItem = FindNextSlotWithItem()?.Item;
                    } // Clear the equipped item if it was removed // Clear the equipped item if it was removed

                    Debug.Log($"Removed item: {item} as its value is now 0.");
                }
            }
        }
        else
        {
            existing = null;
            slot.Item = existing; // Remove the item entirely if it is not stackable or duplicates are not allowed
            Debug.Log($"Removed item: {item}");
            if (EquippedItem == item)
            {
                EquippedItem = null; // Clear the equipped item if it was removed
                EquippedItem = FindNextSlotWithItem()?.Item;
            } // Clear the equipped item if it was removed // Clear the equipped item if it was removed
        }
        UpdateText?.Invoke();
        ItemRemoved?.Invoke();
    }
    public void LogInventory()
    {
        Debug.Log("Inventory Items:");
        foreach (var item in PlayerInventory)
        {
            Debug.Log($"Name: {item.Item?.Name}, Type: {item.Item?.Type}, Value: {item.Item?.Value}/{item.Item?.MaxValue}, Stackable: {item.Item?.IsStackable}");
        }
    }

    public void OnDisable()
    {
        // Unsubscribe from events to prevent memory leaks
        ItemAdded = null;
        OnSlotEquipped = null;
        SignalUpdates = null;
        NewSlotSelected = null;
        UpdateText = null;
        NewItemEquipped = null;
        OnMoveAction = null;

        // Find the object who sent the command to disbale this script
        Debug.Log("Inventory script was disabled!\n" + Environment.StackTrace);
    }
}