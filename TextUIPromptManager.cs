using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[System.Serializable]
public enum TextState
{
    NONE,
    UI_TEXT_TYPING,
    AWAIT_INPUT_TO_END,
    PROMPT_CHOICES
}

[System.Serializable]
public enum CharacterSpeaking
{
    TRIVIAL,
    SPY_GUY,
    SHADY_LADY,
    SHOP_KEEPER,
    CONFIRMATION_PROMPT,
    LITERALLY_JUST_SOME_PERSON
}

public struct PromptEntry
{
    public string text;
    public float speakingRate;
    public CharacterSpeaking speaker;
    public bool awaitChoice;
}

public class TextUIPromptManager : MonoBehaviour
{
    private static readonly System.Lazy<TextUIPromptManager> _instance = new(() => FindFirstObjectByType<TextUIPromptManager>());

    public static TextUIPromptManager Instance => _instance.Value;

    [SerializeField] private TMP_Text textDisplay;
    [SerializeField] private TMP_Text displayName;
    [SerializeField] private SpriteRenderer UIBarRenderer;
    [SerializeField] private Movement2 _movement;
    [SerializeField] private GameObject UIBar;

    private Queue<PromptEntry> promptQueue = new();
    private HashSet<int> shakeIndices = new();

    [SerializeField] private PromptEntry currentPrompt;
    [SerializeField] private char[] characters;
    [SerializeField] private char[] displayArray;
    [SerializeField] private int displayIndex;
    [SerializeField] private float UITimer;
    [SerializeField] private float speakingRate;
    [SerializeField] private bool isUIBoxEnabled;
    private bool isUIAnimating;
    [SerializeField] private bool isPromptChoices;
    [SerializeField] private TextState textState = TextState.NONE;

    private readonly Vector3 UIBarTargetScale = new(1822f, 372f, 1f);
    private readonly Vector3 UIBarTargetShrinkScale = new(0f, 0f, 1f);

    [SerializeField] private float UIBeforeTypingDelay = 0.3f;
    [SerializeField] private bool initiateExample = false;
    [SerializeField] private float canPressTimer;
    [SerializeField] private float slowDownPunctuationRate = 10f;
    private void Awake()
    {
        if (_instance.IsValueCreated && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (initiateExample)
        {
            _movement = FindFirstObjectByType<Movement2>();
            if (_movement == null)
            {
                Debug.LogWarning("Movement2 component not found. Cannot initiate example prompts.");
                return;
            }
            List<PromptEntry> examplePrompts = new()
            {
                new PromptEntry { text = "...", speakingRate = 10f, speaker = CharacterSpeaking.SPY_GUY, awaitChoice = false },
                new PromptEntry { text = "I see you would like to maintain the current thing", speakingRate = 18f, speaker = CharacterSpeaking.SHOP_KEEPER, awaitChoice = false },
                new PromptEntry { text = "We gotta make it at <shake>LEAST</shake> ten thousand!!!", speakingRate = 30f, speaker = CharacterSpeaking.SHADY_LADY, awaitChoice = false },
                new PromptEntry { text = "I don't know what you're talking about... ten thousand??", speakingRate = 18f, speaker = CharacterSpeaking.SHOP_KEEPER, awaitChoice = false },
                new PromptEntry { text = "Ugh... whatever...", speakingRate = 14f, speaker = CharacterSpeaking.SHOP_KEEPER, awaitChoice = false },
                new PromptEntry { text = "I guess I can give you a discount...", speakingRate = 18f, speaker = CharacterSpeaking.SHOP_KEEPER, awaitChoice = false },
                new PromptEntry { text = "But only if you can beat me in a game of Rock, Paper, Scissors!", speakingRate = 18f, speaker = CharacterSpeaking.SHOP_KEEPER, awaitChoice = false },
                new PromptEntry { text = "...", speakingRate = 10f, speaker = CharacterSpeaking.SPY_GUY, awaitChoice = false },
                new PromptEntry { text = "We should TOOOOOTALLY do that. It would be really fun!!!", speakingRate = 30f, speaker = CharacterSpeaking.SHADY_LADY, awaitChoice = false },
                
            };
            if (examplePrompts != null && examplePrompts.Count > 0)
            {
                StartPromptSequence(examplePrompts);
                Debug.Log("Example prompts initiated.");
            }
            else
            {
                Debug.LogWarning("Example prompts are null or empty. Skipping prompt initiation.");
            }
            initiateExample = false;
        }

        AnimateUIBox();

        switch (textState)
        {
            case TextState.UI_TEXT_TYPING:
                canPressTimer += Time.deltaTime;
                RunTypingLogic();
                break;

            case TextState.AWAIT_INPUT_TO_END:
                canPressTimer += Time.deltaTime;
                canPressTimer = Mathf.Clamp(canPressTimer, 0f, 0.6f);
                if (_movement.inputHandler.attackTriggered && canPressTimer >= 0.5f) 
                {
                    AdvancePrompt();
                    canPressTimer = 0f;
                }
                break;

            case TextState.PROMPT_CHOICES:
                // Handle choices here
                break;

            case TextState.NONE:
            default:
                break;
        }
    }

    private string ParseShakeTags(string input, out HashSet<int> shakePositions)
    {
        shakePositions = new();
        List<char> finalChars = new();
        bool shaking = false;
        int realIndex = 0;

        for (int i = 0; i < input.Length;)
        {
            if (input[i..].StartsWith("<shake>"))
            {
                shaking = true;
                i += "<shake>".Length;
                continue;
            }
            else if (input[i..].StartsWith("</shake>"))
            {
                shaking = false;
                i += "</shake>".Length;
                continue;
            }

            finalChars.Add(input[i]);
            if (shaking) shakePositions.Add(realIndex);
            realIndex++;
            i++;
        }

        return new string(finalChars.ToArray());
    }


    public static void StartPromptSequence(List<PromptEntry> prompts)
    {
        Instance.promptQueue.Clear();
        foreach (PromptEntry prompt in prompts)
        {
            Instance.promptQueue.Enqueue(prompt);
        }

        Instance.AdvancePrompt();
    }

    private void AdvancePrompt()
    {
        Debug.Log($"Advancing prompt. Remaining prompts in queue: {promptQueue.Count}");

        if (promptQueue.Count == 0)
        {
            Debug.Log("No more prompts in queue. Ending all prompts.");
            EndAllPrompts();
            return;
        }
        
        currentPrompt = promptQueue.Dequeue();
        Debug.Log($"Starting next prompt: {currentPrompt.text}");
        StartCoroutine(DelayedStartPrompt(currentPrompt));
    }

    private IEnumerator DelayedStartPrompt(PromptEntry entry)
    {
        if (!isUIAnimating) EnableUIBox(); // Start animating the UI
        yield return new WaitForSeconds(UIBeforeTypingDelay);
        StartTypingPrompt(entry); // Only after delay
    }

    private void StartTypingPrompt(PromptEntry entry)
    {
        string cleanedText = ParseShakeTags(entry.text, out shakeIndices);
        characters = cleanedText.ToCharArray();
        displayArray = new char[characters.Length];
        displayIndex = 0;
        UITimer = 0f;
        displayName.text = GetSpeakerName(entry.speaker);
        textDisplay.text = string.Empty;
        speakingRate = entry.speakingRate;
        speakingRate = Mathf.Clamp(speakingRate, 0.5f, 60f);
        isPromptChoices = entry.awaitChoice;

        SetTextState(TextState.UI_TEXT_TYPING);
    }

    private bool PunctuationDetected => displayIndex > 0 && characters[displayIndex-1] is '.' or ',' or '!' or '?' or ':' or ';' or '-';
    private void RunTypingLogic()
    {
        UITimer += Time.deltaTime;
        canPressTimer = Mathf.Clamp(canPressTimer, 0f, 0.5f);

        if (_movement.inputHandler.attackTriggered && canPressTimer >= 0.5f)
        {
            displayIndex = characters.Length;
            textDisplay.text = new string(characters);
            ApplyShakeToVisibleCharacters();
            canPressTimer = 0f;
            SetTextState(TextState.AWAIT_INPUT_TO_END);
            return;
        }

        if (displayIndex < characters.Length)
        {
            float delay = 1 / speakingRate;
            if (PunctuationDetected)
            {
                delay *= slowDownPunctuationRate;
            }

            if (UITimer >= delay)
            {
                UITimer -= delay;
                displayArray[displayIndex] = characters[displayIndex];
                textDisplay.text = new string(displayArray);
                ApplyShakeToVisibleCharacters();
                displayIndex++;
            }
        }
        else if (displayIndex >= characters.Length)
        {
            SetTextState(isPromptChoices ? TextState.PROMPT_CHOICES : TextState.AWAIT_INPUT_TO_END);
            canPressTimer = 0f;
        }
    }


    void ApplyShakeToVisibleCharacters()
    {
        TMP_TextInfo textInfo = textDisplay.textInfo;
        textDisplay.ForceMeshUpdate();
        
        for (int i = 0; i < displayIndex; i++)
        {
            if (!shakeIndices.Contains(i)) continue;

            int charIndex = i;
            if (!textInfo.characterInfo[charIndex].isVisible) continue;

            int vertexIndex = textInfo.characterInfo[charIndex].vertexIndex;
            int materialIndex = textInfo.characterInfo[charIndex].materialReferenceIndex;

            Color32[] newVertexColors = textInfo.meshInfo[materialIndex].colors32;

            for (int j = 0; j < 4; j++)
            {
                newVertexColors[vertexIndex + j] = new Color32(255, 0, 0, 255); // Red = shake flag
            }
        }

        textDisplay.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }


    private void SetTextState(TextState newState)
    {
        Debug.Log($"Changing text state from {textState} to {newState}");
        textState = newState;
        UITimer = 0f;
    }

    private string GetSpeakerName(CharacterSpeaking speaker)
    {
        return speaker switch
        {
            CharacterSpeaking.TRIVIAL => "!!!",
            CharacterSpeaking.SPY_GUY => "Spy-Guy",
            CharacterSpeaking.SHADY_LADY => "Shady-Lady",
            CharacterSpeaking.SHOP_KEEPER => "Shop-Keeper",
            CharacterSpeaking.CONFIRMATION_PROMPT => "Confirmation",
            _ => "???"
        };
    }

    private void EnableUIBox()
    {
        _movement.canmove = false;
        isUIBoxEnabled = true;
        isUIAnimating = true;

        UIBarRenderer.color = new Color(UIBarRenderer.color.r, UIBarRenderer.color.g, UIBarRenderer.color.b, 0);
        UIBar.transform.localScale = UIBarTargetShrinkScale;
    }

    private void AnimateUIBox()
    {
        if (isUIBoxEnabled)
        {
            Color targetAlpha = new(UIBarRenderer.color.r, UIBarRenderer.color.g, UIBarRenderer.color.b, 1);
            UIBarRenderer.color = Color.Lerp(UIBarRenderer.color, targetAlpha, 8f * Time.deltaTime);
            UIBar.transform.localScale = Vector3.Lerp(UIBar.transform.localScale, UIBarTargetScale, 8f * Time.deltaTime);
        }
        else
        {
            Color targetAlpha = new(UIBarRenderer.color.r, UIBarRenderer.color.g, UIBarRenderer.color.b, 0);
            UIBarRenderer.color = Color.Lerp(UIBarRenderer.color, targetAlpha, 8f * Time.deltaTime);
            UIBar.transform.localScale = Vector3.Lerp(UIBar.transform.localScale, UIBarTargetShrinkScale, 8f * Time.deltaTime);
        }
    }

    private void EndAllPrompts()
    {
        Debug.Log("Ending all prompts. Clearing UI box.");
        isUIBoxEnabled = false;
        isUIAnimating = false;
        textDisplay.text = "";
        displayName.text = "";
        _movement.canmove = true;
        promptQueue.Clear();
        SetTextState(TextState.NONE);
    }
}

