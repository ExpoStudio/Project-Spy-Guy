using System;
using System.Collections.Generic;
using System.Collections;
using Items;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Animation state constants
public static class AnimState
{
    public const string CLOSED = "Closed";
    public const string OPENING = "Opening";
    public const string OPEN = "Open";
    public const string CLOSING = "Closing";
}

public static class FolderAnimState
{
    public const string FOLDER_CLOSED = "FolderClosed";
    public const string FOLDER_OPENING = "FolderOpening";
    public const string FOLDER_OPEN = "FolderOpen";
    public const string FOLDER_CLOSING = "FolderClosing";
}

enum PlayerSlotHighlighted
{
    First,
    Second,
    Third,
    Armor,
    CaseObjective
}
public class UIInventoryListener : MonoBehaviour
{
    [Range(0, 3)]
    [SerializeField] private int _assignedIndex;
    [SerializeField] private Inventory _inventory;
    [SerializeField] private PlayerInput _playerInput;
    [SerializeField] private Animator _suitCaseAnimator;
    [SerializeField] private Animator _folderAnimator;


    [SerializeField] private CharacterSelectionSystem _characterSelectionSystem;
    [SerializeField] private PlayerSlotHighlighted _playerSlotHighlighted;
    [SerializeField] private Image _slot1Image, _slot2Image, _slot3Image, _armorImage, _caseObjectiveImage, _equippedItemImage;
    [SerializeField] private CanvasGroup _inventoryCanvasGroup;
    [SerializeField] private TMP_Text Slot1Text, Slot2Text, Slot3Text, ArmorText, CaseObjectiveText;
    [SerializeField] private List<Image> SlotIconImages;
    [SerializeField] private Image EquippedItemIcon;


    [SerializeField] private Image EquippedHudIcon;
    [SerializeField] private Image EquippedHudDurabilityDisplay;
    [SerializeField] private TMP_Text HudItemCount;

    [SerializeField] private Image NoteBackgroundImage;
    [SerializeField] private TMP_Text ItemNameText;
    [SerializeField] private TMP_Text ItemDescriptionText;

    private bool _isFadingIn = false;
    private bool _isFadingOut = false;

    private string currentState;
    private string currentFolderState;


    // Update is called once per frame
    private void Start()
    {
        _inventoryCanvasGroup.alpha = 0f; // Start with the canvas group hidden
        _inventoryCanvasGroup.interactable = false; // Disable interaction
        _inventoryCanvasGroup.blocksRaycasts = false; // Disable raycasts


        if (PlayerInputManager.instance == null)
        {
            Debug.Log("PlayerInputManager.instance is null, cannot subscribe to events.");
            return;
        }
        PlayerInputManager.instance.onPlayerJoined += OnPlayerJoined;
        PlayerInputManager.instance.onPlayerLeft += OnPlayerLeft;
        Debug.Log("Subscribed to PlayerInputManager events in Awake.");
    }

    private void OnPlayerLeft(PlayerInput playerInput)
    {
        //Check if the playerInput is the one we are looking for
        if (playerInput.playerIndex == _assignedIndex)
        {
            _inventory.NewSlotSelected -= UpdateInventoryUI;
            _playerInput = null;
            _characterSelectionSystem = null;
            _inventory = null;
            // Unsubscribe from the event
            PlayerInputManager.instance.onPlayerLeft -= OnPlayerLeft;
            PlayerInputManager.instance.onPlayerJoined -= OnPlayerJoined;

            _inventory.UpdateText -= UpdateTexts;
            _inventory.NewItemEquipped -= ChosenItemAnimation;

            Debug.Log("Unsubscribed from PlayerInputManager events in OnPlayerLeft.");
        }
        else
        {
            Debug.Log("OnPlayerLeft called, but playerIndex did not match _assignedIndex.");
            return;
        }
    }

    void OnDestroy()
    {
        _inventory.NewSlotSelected -= UpdateInventoryUI;
        _playerInput = null;
        _characterSelectionSystem = null;
        //Unsubscribe from the event
        if (PlayerInputManager.instance != null)
        {

            PlayerInputManager.instance.onPlayerLeft -= OnPlayerLeft;
            PlayerInputManager.instance.onPlayerJoined -= OnPlayerJoined;

            _inventory.UpdateText -= UpdateTexts;
            _inventory.NewItemEquipped -= ChosenItemAnimation;

            Debug.Log("Unsubscribed from PlayerInputManager events in OnDestroy.");
        }
        else
        {
            Debug.LogWarning("PlayerInputManager.instance was null in OnDestroy, could not unsubscribe.");
        }
    }

    private void OnPlayerJoined(PlayerInput playerInput)
    {
        //Check if the playerInput is the one we are looking for
        if (playerInput.playerIndex == _assignedIndex)
        {
            _playerInput = playerInput;
            _characterSelectionSystem = playerInput.transform.GetComponent<CharacterSelectionSystem>();
            _inventory = playerInput.transform.GetChild(0).GetComponent<Inventory>();
            // Subscribe to the event only if _inventory is not null
            if (_inventory != null)
            {
                _inventory.NewSlotSelected += UpdateInventoryUI;
                _inventory.UpdateText += UpdateTexts;
                _inventory.ItemRemoved += () => { UpdateSlotIcons(); UpdateEquippedIcon(); UpdateTexts(); };
                _inventory.NewItemEquipped += ChosenItemAnimation;
            }
            PlayerInputManager.instance.onPlayerJoined -= OnPlayerJoined;

        }
        else
        {
            return;
        }
    }

    private float _fadeTarget = 0f;
    private float _fadeSpeed = 5f; // Adjust for desired fade speed (higher = faster)
    public void Update()
    {
        FadeInInventoryUI();
        if (_inventory == null || _characterSelectionSystem == null || _playerInput == null)
        {
            return;
        }
        string newState = DetermineAnimationState();
        ChangeAnimationStateCrossFade(newState, 0f);

        string newFolderState = DetermineFolderAnimationState();
        ChangeFolderAnimationStateCrossFade(newFolderState, 0f);
    }

    private void FadeInInventoryUI()
    {
        if (_inventory == null)
        {
            return;
        }

        _fadeTarget = _inventory._gameControlsManager.openInventoryTriggered && (_inventory.InventoryStatus == InventoryStatus.Opening || _inventory.InventoryStatus == InventoryStatus.Opened)
            ? 1f
            : 0f;

        // Smoothly move alpha towards target
        float prevAlpha = _inventoryCanvasGroup.alpha;
        _inventoryCanvasGroup.alpha = Mathf.MoveTowards(_inventoryCanvasGroup.alpha, _fadeTarget, _fadeSpeed * Time.unscaledDeltaTime);

        // Set interactable/raycast/NoteBackgroundImage when fade completes
        if (_inventoryCanvasGroup.alpha == 1f && prevAlpha != 1f)
        {
            _inventoryCanvasGroup.interactable = true;
            _inventoryCanvasGroup.blocksRaycasts = true;
        }
        else if (_inventoryCanvasGroup.alpha == 0f && prevAlpha != 0f)
        {
            _inventoryCanvasGroup.interactable = false;
            _inventoryCanvasGroup.blocksRaycasts = false;
        }
    }

    private void UpdateInventoryUI(Inventory.PlayerSlotHighlighted playerSlotHighlighted)
    {
        UpdateTexts();
        UpdateSlotIcons();
        UpdateEquippedIcon();
        UpdateSlotHighlighting(playerSlotHighlighted);
        //Make inventory canvas group visible
        _inventoryCanvasGroup.alpha = 1f;
        _inventoryCanvasGroup.interactable = true;
        _inventoryCanvasGroup.blocksRaycasts = true;
    }

    private void UpdateSlotIcons()
    {
        for (int imageNumber = 0; imageNumber < _inventory.PlayerInventory.Count; imageNumber++)
        {
            if (imageNumber < SlotIconImages.Count &&
                _inventory.PlayerInventory[imageNumber]?.Item != null)
            {
                var item = _inventory.PlayerInventory[imageNumber].Item;
                if (item == null)
                {
                    Debug.LogWarning($"Item in slot {imageNumber} is null.");
                    SlotIconImages[imageNumber].sprite = null;
                    SlotIconImages[imageNumber].enabled = false;
                    continue;
                }
                if (item.Icon != null)
                {
                    SlotIconImages[imageNumber].sprite = item.Icon;
                    SlotIconImages[imageNumber].enabled = true;
                }
                else
                {
                    SlotIconImages[imageNumber].sprite = null;
                    SlotIconImages[imageNumber].enabled = false;
                }
            }
            else if (imageNumber < SlotIconImages.Count && _inventory.PlayerInventory[imageNumber] == null)
            {
                // If the slot is empty, disable the icon
                SlotIconImages[imageNumber].sprite = null;
                SlotIconImages[imageNumber].enabled = false;
            }
            else if (imageNumber >= SlotIconImages.Count)
            {
                // If there are more slots than images, disable the extra images
                Debug.LogWarning($"SlotIconImages does not have enough images for slot {imageNumber}. Disabling.");
            }
            else
            {
                SlotIconImages[imageNumber].sprite = null;
                SlotIconImages[imageNumber].enabled = false;
            }
        }
    }

    private void UpdateEquippedIcon()
    {
        if (EquippedItemIcon != null)
        {
            if (_inventory.EquippedItem != null)
            {
                var equippedSprite = _inventory.EquippedItem.Icon;
                EquippedItemIcon.sprite = equippedSprite;
                EquippedItemIcon.enabled = true;
            }
            else
            {
                EquippedItemIcon.sprite = null;
                EquippedItemIcon.enabled = false;
            }
        }
    }


    private Image FindSelectedImage => _inventory.SlotHighlighted switch
    {
        Inventory.PlayerSlotHighlighted.First => _slot1Image,
        Inventory.PlayerSlotHighlighted.Second => _slot2Image,
        Inventory.PlayerSlotHighlighted.Third => _slot3Image,
        _ => null
    };
    private void UpdateSlotHighlighting(Inventory.PlayerSlotHighlighted playerSlotHighlighted)
    {
        static void ScaleImage(Image img, bool selected, float scaleTarget = 1)
        {
            float targetScale = selected ? scaleTarget : 1f;
            if (img == null)
            {
                return;
            }

            LeanTween.scale(img.gameObject, Vector3.one * targetScale, 0.15f).setEase(LeanTweenType.easeOutBack);
        }


        ScaleImage(_slot1Image, playerSlotHighlighted == Inventory.PlayerSlotHighlighted.First, 1.4f);
        ScaleImage(_slot2Image, playerSlotHighlighted == Inventory.PlayerSlotHighlighted.Second, 1.4f);
        ScaleImage(_slot3Image, playerSlotHighlighted == Inventory.PlayerSlotHighlighted.Third, 1.4f);
        ScaleImage(_armorImage, playerSlotHighlighted == Inventory.PlayerSlotHighlighted.Armor, 1.4f);
        ScaleImage(_caseObjectiveImage, playerSlotHighlighted == Inventory.PlayerSlotHighlighted.CaseObjective, 1.4f);

        Slot1Text.color = playerSlotHighlighted == Inventory.PlayerSlotHighlighted.First ? Color.yellow : Color.white;
        Slot2Text.color = playerSlotHighlighted == Inventory.PlayerSlotHighlighted.Second ? Color.yellow : Color.white;
        Slot3Text.color = playerSlotHighlighted == Inventory.PlayerSlotHighlighted.Third ? Color.yellow : Color.white;
    }

    private void ChosenItemAnimation()
    {
        Image selectedImage = FindSelectedImage;
        Image HudIcon = EquippedHudIcon;
        if (selectedImage == null)
        {
            return;
        }

        float moveDistance = 60f;
        float animationDuration = 0.3f;

        Vector2 originalPos = selectedImage.rectTransform.anchoredPosition;
        Vector2 targetPos = originalPos + Vector2.up * moveDistance;

        Color originalColor = selectedImage.color;
        Color originalHudColor = HudIcon.color;

        LeanTween.moveY(selectedImage.rectTransform, targetPos.y, animationDuration)
            .setEase(LeanTweenType.easeOutBack)
            .setOnComplete(() =>
            {
                LeanTween.moveY(selectedImage.rectTransform, originalPos.y, 0.2f).setOnComplete(() =>
                selectedImage.rectTransform.anchoredPosition = originalPos);
            });
        //Animate Color
        LeanTween.value(selectedImage.gameObject, originalColor, Color.yellow, 0.03f)
            .setEase(LeanTweenType.easeInOutSine)
            .setLoopPingPong(5)
            .setOnUpdate(col => selectedImage.color = col)
            .setOnComplete(() => selectedImage.color = originalColor);

        //Animate HUD Icon Indicator
        LeanTween.moveY(selectedImage.rectTransform, targetPos.y, animationDuration)
            .setEase(LeanTweenType.easeOutBack)
            .setOnComplete(() =>
            {
                LeanTween.moveY(selectedImage.rectTransform, originalPos.y, 0.2f).setOnComplete(() =>
                selectedImage.rectTransform.anchoredPosition = originalPos);
            });

        //Animate Color of Hud Icon Indicator
        LeanTween.value(HudIcon.gameObject, originalColor, Color.yellow, 0.5f)
            .setEase(LeanTweenType.easeInOutSine)
            .setLoopPingPong(3)
            .setOnUpdate(col => HudIcon.color = col)
            .setOnComplete(() => HudIcon.color = originalHudColor);
    }

    private void UpdateTexts()
    {
        if (_inventory == null) return;

        // Update selected item info
        ItemDescriptionText.text = _inventory.SelectedSlot?.Item?.Description ?? "No item selected.";
        ItemNameText.text = _inventory.SelectedSlot?.Item?.Name ?? "No item selected.";

        // Update slot quantities
        UpdateSlotText(Slot1Text, 0);
        UpdateSlotText(Slot2Text, 1);
        UpdateSlotText(Slot3Text, 2);
    }

    private void UpdateSlotText(TMP_Text slotText, int slotIndex)
    {
        if (_inventory.PlayerInventory.Count > slotIndex && 
            _inventory.PlayerInventory[slotIndex]?.Item != null)
        {
            int value = _inventory.PlayerInventory[slotIndex].Item.Value;
            slotText.text = value > 1 ? value.ToString() : "";
        }
        else
        {
            slotText.text = "";
        }
    }

    private string DetermineFolderAnimationState()
    {
        if (_inventory == null)
        {
            return currentFolderState;
        }

        switch (_inventory.InventoryStatus)
        {
            case InventoryStatus.Closed:
                if (currentFolderState != FolderAnimState.FOLDER_CLOSED && IsAnimFinished(_suitCaseAnimator, AnimState.CLOSING))
                {
                    return FolderAnimState.FOLDER_CLOSED;
                }
                break;
            case InventoryStatus.Opening:
                if (currentFolderState != FolderAnimState.FOLDER_OPENING && !IsAnimFinished(_suitCaseAnimator, AnimState.OPENING))
                {
                    UpdateTexts();
                    UpdateSlotIcons();
                    UpdateEquippedIcon();
                    UpdateSlotHighlighting(_inventory.SlotHighlighted);

                    return FolderAnimState.FOLDER_OPENING;
                }
                break;
            case InventoryStatus.Opened:
                if (currentFolderState != FolderAnimState.FOLDER_OPEN && IsAnimFinished(_suitCaseAnimator, AnimState.OPENING))
                {
                    return FolderAnimState.FOLDER_OPEN;
                }
                break;
            case InventoryStatus.Closing:
                if (currentFolderState != FolderAnimState.FOLDER_CLOSING && !IsAnimFinished(_suitCaseAnimator, AnimState.CLOSING))
                {
                    return FolderAnimState.FOLDER_CLOSING;
                }
                break;
        }
        return currentFolderState; // Return the current state if no conditions are met
    }


    private string DetermineAnimationState()
    {
        if (_inventory == null)
        {
            return currentState;
        }

        switch (_inventory.InventoryStatus)
        {
            case InventoryStatus.Closed:
                if (currentState != AnimState.CLOSED)
                {
                    return AnimState.CLOSED;
                }
                break;
            case InventoryStatus.Opening:
                if (currentState != AnimState.OPENING)
                {
                    return AnimState.OPENING;
                }
                break;
            case InventoryStatus.Opened:
                if (currentState != AnimState.OPEN)
                {
                    return AnimState.OPEN;
                }
                break;
            case InventoryStatus.Closing:
                if (currentState != AnimState.CLOSING)
                {
                    return AnimState.CLOSING;
                }
                break;
        }
        return currentState;// Return the current state if no conditions are met
    }

    void ChangeAnimationState(string newState)
    {
        if (_suitCaseAnimator.GetCurrentAnimatorStateInfo(0).IsName(newState))
        {
            return;
        }

        _suitCaseAnimator.Play(newState);
        currentState = newState;
    }

    void ChangeFolderAnimationState(string newState)
    {
        if (_folderAnimator.GetCurrentAnimatorStateInfo(0).IsName(newState))
        {
            return;
        }

        _folderAnimator.Play(newState);
        currentFolderState = newState;
    }

    void ChangeAnimationStateCrossFade(string newState, float transitionDuration = 1)
    {
        if (_suitCaseAnimator.GetCurrentAnimatorStateInfo(0).IsName(newState))
        {
            return;
        }

        _suitCaseAnimator.CrossFade(newState, transitionDuration);
        currentState = newState;
    }

    void ChangeFolderAnimationStateCrossFade(string newState, float transitionDuration = 1)
    {
        if (_folderAnimator.GetCurrentAnimatorStateInfo(0).IsName(newState))
        {
            return;
        }

        _folderAnimator.CrossFade(newState, transitionDuration);
        currentFolderState = newState;
    }

    bool IsAnimFinished(Animator animator, string stateName)
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName(stateName) && animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1f) 
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
