// UIController.cs
// Updated to reliably wire bottom-bar buttons ("Character", "Items", "Menu") at runtime
// without adding new files. Brings HUD root to front and sets pickingMode to allow clicks.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using MyGameNamespace;

public class UIController : MonoBehaviour
{
    [Header("UI Document")]
    [SerializeField] private UIDocument gameUIDocument;

    [Header("Portrait Resources")]
    [SerializeField] private Texture2D earthPonyPortrait;
    [SerializeField] private Texture2D unicornPortrait;
    [SerializeField] private Texture2D pegasusPortrait;
    [SerializeField] private Texture2D batPonyPortrait;
    [SerializeField] private Texture2D griffonPortrait;
    [SerializeField] private Texture2D dragonPortrait;
    [SerializeField] private Texture2D humanPortrait;

    [Header("Visual Settings")]
    [SerializeField] private int headerFontSize = 22;
    [SerializeField] private int storyFontSize = 18;
    [SerializeField] private int choiceFontSize = 16;
    [SerializeField] private float barAnimationDuration = 0.6f;
    [SerializeField] private bool enableBarPulse = true;
    [SerializeField] private bool enableColorTransitions = true;

    [Header("Bar Colors")]
    [SerializeField] private Color healthColor = new Color(0.86f, 0.2f, 0.2f, 1f);
    [SerializeField] private Color healthLowColor = new Color(0.7f, 0.12f, 0.12f, 1f);
    [SerializeField] private Color energyColor = new Color(0.2f, 0.59f, 0.86f, 1f);
    [SerializeField] private Color magicColor = new Color(0.59f, 0.2f, 0.86f, 1f);
    [SerializeField] private Color friendshipColor = new Color(0.86f, 0.59f, 0.2f, 1f);
    [SerializeField] private Color discordColor = new Color(0.47f, 0.2f, 0.59f, 1f);
    [SerializeField] private Color discordHighColor = new Color(0.31f, 0.12f, 0.39f, 1f);

    [Header("Debug")]
    [SerializeField] private bool verboseLogging = true;

    // Core state
    private VisualElement root;
    private bool isInitialized = false;
    private PlayerCharacter currentPlayer;

    // UI Element Cache
    private Image characterPortrait;
    private Label characterName;
    private Label raceLabel;
    private Label levelLabel;
    private Label bitsValue;

    // Stats
    private Label healthValue;
    private Label staminaValue;
    private Label manaValue;
    private Label harmonyValue;
    private Label discordValue;
    private ProgressBar healthBar;
    private ProgressBar staminaBar;
    private ProgressBar manaBar;
    private ProgressBar harmonyBar;
    private ProgressBar discordBar;

    // Attributes
    private Label strengthValue;
    private Label dexterityValue;
    private Label constitutionValue;
    private Label intelligenceValue;
    private Label wisdomValue;
    private Label charismaValue;

    // Story Display
    private Label storyTitleLabel;
    private Label storyText;
    private Image storyImage;
    // Container for rich story content (inline images and formatted text)
    private VisualElement storyBody;
    private VisualElement choicesPanel;

    // Choice System
    private readonly List<Button> choiceButtons = new();
    private readonly string[] hotkeyLabels = { "[1]", "[2]", "[3]", "[4]", "[5]", "[6]", "[7]", "[8]" };

    // Animation System
    private readonly Dictionary<string, BarAnimationData> barAnimations = new();
    private Coroutine pulseCoroutine;
    private AnimationCurve barAnimationCurve;

    // Bottom bar buttons (wired at runtime)
    private Button bottomCharacterButton;
    private Button bottomItemsButton;
    private Button bottomMenuButton;

    // New map button (wired at runtime)
    private Button bottomMapButton;

    private class BarAnimationData
    {
        public ProgressBar bar;
        public VisualElement fill;
        public float currentValue;
        public float targetValue;
        public float maxValue;
        public Color normalColor;
        public Color lowColor;
        public Color highColor;
        public bool isAnimating;
        public Coroutine animationCoroutine;
        public float lowThreshold = 0.25f;
        public float highThreshold = 0.75f;
        public string cssClass;
    }

    #region Unity Lifecycle

    private void Awake()
    {
        if (barAnimationCurve == default)
            barAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        // Use a coroutine-based initialization to avoid threading issues with Unity APIs.
        StartCoroutine(InitializeCoroutine());
    }

    private void OnEnable()
    {
        if (!isInitialized)
            StartCoroutine(InitializeCoroutine());
    }

    #endregion

    #region Initialization

    public void Initialize()
    {
        if (!isInitialized)
            StartCoroutine(InitializeCoroutine());
    }

    private IEnumerator InitializeCoroutine()
    {
        if (isInitialized) yield break;

        if (!SetupUIDocument())
        {
            Debug.LogWarning("UIController: UIDocument not ready. Will retry next frame.");
            yield return null;
            if (!SetupUIDocument())
            {
                Debug.LogError("UIController: No UIDocument found. Initialization aborted.");
                yield break;
            }
        }

        // Ensure this root accepts pointer events and is front-most to avoid invisible overlays blocking bottom buttons
        try
        {
            root.pickingMode = PickingMode.Position;
            root.BringToFront();
        }
        catch { /* bring to front/pickingMode might not be critical on some Unity versions */ }

        CacheUIElements();
        SetupAnimatedBars();
        SetupChoiceButtons();
        SetupHotkeys();

        // Bring the bottom navigation container to the front and ensure it can
        // receive pointer events. Without this, other UI layers (e.g. story panel)
        // may sit above the bottom bar and intercept clicks, causing the Menu
        // button to appear non‑functional in the Game scene.
        try
        {
            var bottomNav = root.Q<VisualElement>("BottomNav");
            if (bottomNav != default)
            {
                bottomNav.BringToFront();
                bottomNav.pickingMode = PickingMode.Position;
            }
        }
        catch
        {
            // Some runtime profiles may not support BringToFront; ignore errors
        }

        // Wait for GameEventSystem to exist and then subscribe (avoid blocking)
        yield return StartCoroutine(WaitForGameEventSystemAndSubscribe());

        // Wire bottom-bar buttons (Character / Items / Menu)
        WireBottomBarButtons();

        ApplyUIStyles();

        isInitialized = true;
        if (verboseLogging) Debug.Log("UIController: Initialized successfully");

        // Update with current player if available
        var gm = GameManager.Instance;
        PlayerCharacter player = null;
        if (gm != default)
        {
            player = gm.GetPlayer();
        }
        // Fallback: use PlayerState.Current if GameManager has no player
        if (player == default)
        {
            player = MyGameNamespace.PlayerState.Current;
        }
        if (player != default)
        {
            // Ensure the player's stats are fully restored at the start of the game.
            // This prevents the UI from showing low or leftover health values from a
            // previous play session and gives players a fair starting point.
            if (player.gameStats != default)
            {
                player.gameStats.RestoreAll();
            }
            UpdatePlayerUI(player);
        }
    }

    private bool SetupUIDocument()
    {
        if (gameUIDocument == default)
        {
            gameUIDocument = GetComponent<UIDocument>();
            if (gameUIDocument == default)
            {
                Debug.LogError("UIController: No UIDocument found!");
                return false;
            }
        }

        root = gameUIDocument.rootVisualElement;
        if (root == default)
        {
            Debug.LogError("UIController: No root visual element!");
            return false;
        }

        return true;
    }

    private void CacheUIElements()
    {
        // Character info
        characterPortrait = root.Q<Image>("CharacterPortrait");
        characterName = root.Q<Label>("CharacterName");
        raceLabel = root.Q<Label>("RaceLabel");
        levelLabel = root.Q<Label>("LevelLabel");
        bitsValue = root.Q<Label>("BitsValue");

        // Stats values
        healthValue = root.Q<Label>("HealthValue");
        staminaValue = root.Q<Label>("StaminaValue");
        manaValue = root.Q<Label>("ManaValue");
        harmonyValue = root.Q<Label>("HarmonyValue");
        discordValue = root.Q<Label>("DiscordValue");

        // Stats bars
        healthBar = root.Q<ProgressBar>("HealthBar");
        staminaBar = root.Q<ProgressBar>("StaminaBar");
        manaBar = root.Q<ProgressBar>("ManaBar");
        harmonyBar = root.Q<ProgressBar>("HarmonyBar");
        discordBar = root.Q<ProgressBar>("DiscordBar");

        // Attributes
        strengthValue = root.Q<Label>("StrengthValue");
        dexterityValue = root.Q<Label>("DexterityValue");
        constitutionValue = root.Q<Label>("ConstitutionValue");
        intelligenceValue = root.Q<Label>("IntelligenceValue");
        wisdomValue = root.Q<Label>("WisdomValue");
        charismaValue = root.Q<Label>("CharismaValue");

        // Story elements
        storyTitleLabel = root.Q<Label>("StoryTitle");
        // Optional legacy elements
        storyText = root.Q<Label>("StoryText");
        storyImage = root.Q<Image>("StoryImage");
        // Rich story body container (newer dynamic content)
        storyBody = root.Q<VisualElement>("StoryBody");
        choicesPanel = root.Q<VisualElement>("ChoicesPanel");

        if (verboseLogging)
        {
            Debug.Log($"UIController: Cached elements - Portrait: {characterPortrait != default}, Name: {characterName != default}, Choices: {choicesPanel != default}");
        }
    }

    #endregion

    #region Bottom-bar wiring (fix for nonresponsive bottom buttons)

    
    // --- Helper lookups for bottom-bar wiring ---
    private Button FindButtonInRootByNames(string[] nameOrTextCandidates)
    {
        if (root == default) return null;

        // First try by element name (ID)
        foreach (var candidate in nameOrTextCandidates)
        {
            var b = root.Q<Button>(candidate);
            if (b != default) return b;
        }

        // Next try by text/content (case-insensitive contains)
        var allButtons = root.Query<Button>().ToList();
        foreach (var candidate in nameOrTextCandidates)
        {
            var lower = candidate.ToLowerInvariant();
            var byText = allButtons.FirstOrDefault(btn => !string.IsNullOrEmpty(btn.text) && btn.text.ToLowerInvariant().Contains(lower));
            if (byText != default) return byText;
        }

        return null;
    }

    private Button FindButtonInDocumentByText(UIDocument doc, string partialText)
    {
        if (doc == default || doc.rootVisualElement == default) return null;
        var buttons = doc.rootVisualElement.Query<Button>().ToList();
        var lower = partialText.ToLowerInvariant();
        return buttons.FirstOrDefault(b => !string.IsNullOrEmpty(b.text) && b.text.ToLowerInvariant().Contains(lower));
    }

    private Button FindButtonInElement(VisualElement element, params string[] namesAndClasses)
    {
        if (element == default) return null;

        foreach (var name in namesAndClasses)
        {
            var btn = element.Q<Button>(name);
            if (btn != default) return btn;

            // Try as class name
            btn = element.Q<Button>(className: name);
            if (btn != default) return btn;
        }
        return null;
    }

    private void OnBottomCharacterClicked()
    {
        if (verboseLogging) Debug.Log("[UIController] Bottom Character clicked");
        OpenCharacterSheet();
    }

    private void OnBottomItemsClicked()
    {
        if (verboseLogging) Debug.Log("[UIController] Bottom Items clicked");
        OpenInventoryScreen();
    }

    private void OnBottomMenuClicked()
    {
        if (verboseLogging) Debug.Log("[UIController] Bottom Menu clicked");
        OpenPauseMenu();
    }




    // --- Actions invoked by bottom bar buttons ---
    private void OpenCharacterSheet()
    {
        // Prefer controller if present
        var charController = CompatUtils.FindFirstObjectByTypeCompat<CharacterSheetController>();
        if (charController != default)
        {
            charController.ToggleCharacterSheet();
            if (verboseLogging) Debug.Log("[UIController] Toggled Character Sheet via CharacterSheetController.");
            return;
        }

        // Fallback: toggle known character-sheet containers in any UIDocument
        var docs = CompatUtils.FindObjectsOfTypeCompat<UIDocument>();
        foreach (var d in docs)
        {
            if (d == default || d.rootVisualElement == default) continue;
            var rootVE = d.rootVisualElement.Q<VisualElement>("character-sheet-container")
                        ?? d.rootVisualElement.Q<VisualElement>("character-sheet-modal")
                        ?? d.rootVisualElement.Q<VisualElement>("CharacterSheet")
                        ?? d.rootVisualElement.Q<VisualElement>(className: "character-sheet");
            if (rootVE != default)
            {
                bool isHidden = rootVE.resolvedStyle.display == DisplayStyle.None;
                rootVE.style.display = isHidden ? DisplayStyle.Flex : DisplayStyle.None;
                d.rootVisualElement.BringToFront();
                try { if (d.panelSettings != default && d.panelSettings.sortingOrder < 200) d.panelSettings.sortingOrder = 200; } catch {}
                if (verboseLogging) Debug.Log("[UIController] Toggled Character Sheet via UIDocument.");
                return;
            }
        }

        if (verboseLogging) Debug.LogWarning("[UIController] Character sheet UIDocument not found.");
    }

    private void OpenInventoryScreen()
    {
        // Try to locate a dedicated inventory panel by common IDs/classes
        var docs = CompatUtils.FindObjectsOfTypeCompat<UIDocument>();
        foreach (var d in docs)
        {
            if (d == default || d.rootVisualElement == default) continue;
            var panel = d.rootVisualElement.Q<VisualElement>("inventory-container")
                        ?? d.rootVisualElement.Q<VisualElement>("InventoryRoot")
                        ?? d.rootVisualElement.Q<VisualElement>(className: "inventory")
                        ?? d.rootVisualElement.Q<VisualElement>(className: "inventory-root");
            if (panel != default)
            {
                panel.style.display = DisplayStyle.Flex;
                d.rootVisualElement.BringToFront();
                try { if (d.panelSettings != default && d.panelSettings.sortingOrder < 200) d.panelSettings.sortingOrder = 200; } catch {}
                if (verboseLogging) Debug.Log("[UIController] Inventory opened via UIDocument.");
                return;
            }
        }

        if (verboseLogging) Debug.LogWarning("[UIController] Inventory UI not found.");
    }

    private void OpenPauseMenu()
    {
        // Prefer the dedicated controller if present
        var ctrl = CompatUtils.FindFirstObjectByTypeCompat<PauseMenuController>();
        if (ctrl != default)
        {
            PauseMenuController.OpenPauseMenu();
            if (verboseLogging) Debug.Log("[UIController] Pause menu opened via PauseMenuController.");
            return;
        }
        // Fallback: locate any UIDocument that appears to be the pause menu and show it
        var docs = CompatUtils.FindObjectsOfTypeCompat<UIDocument>();
        foreach (var d in docs)
        {
            if (d == default || d.rootVisualElement == default) continue;
            var panel = d.rootVisualElement.Q<VisualElement>("PauseMenu")
                        ?? d.rootVisualElement.Q<VisualElement>("PauseMenuRoot")
                        ?? d.rootVisualElement.Q<VisualElement>(className: "pause-menu")
                        ?? d.rootVisualElement.Q<VisualElement>(className: "pause-menu-root");
            if (panel != default)
            {
                panel.style.display = DisplayStyle.Flex;
                d.rootVisualElement.BringToFront();
                try { if (d.panelSettings != default && d.panelSettings.sortingOrder < 200) d.panelSettings.sortingOrder = 200; } catch {}
                if (verboseLogging) Debug.Log("[UIController] Opened Pause Menu via UIDocument fallback.");
                return;
            }
        }
        if (verboseLogging) Debug.LogWarning("[UIController] Pause menu not found via controller or UIDocument.");
    }

private void WireBottomBarButtons()
    {
        // Ensure we have a root
        if (root == default && gameUIDocument != default) root = gameUIDocument.rootVisualElement;

        // Attempt 1: look for well-known IDs/classes/text inside this UIDocument root
        if (root != default)
        {
            // Try common IDs first
            bottomCharacterButton = FindButtonInElement(root, "CharacterButton", "Character", "character-button", "nav-btn--character");
            bottomItemsButton = FindButtonInElement(root, "ItemsButton", "InventoryButton", "Items", "Inventory", "items-button", "nav-btn--inventory");
            bottomMenuButton = FindButtonInElement(root, "MenuButton", "Menu", "Pause", "menu-button", "nav-btn--settings");
        }

        // Fallback: look by button text within this root (contains match)
        bottomCharacterButton ??= FindButtonInRootByNames(new[] { "Character", "character" });
        bottomItemsButton ??= FindButtonInRootByNames(new[] { "Items", "items", "inventory", "Inventory" });
        bottomMenuButton ??= FindButtonInRootByNames(new[] { "Menu", "menu", "pause" });

        // Attempt 2: if any not found in this doc, search across all UIDocuments in the scene
        if (bottomCharacterButton == default || bottomItemsButton == default || bottomMenuButton == default)
        {
            var docs = CompatUtils.FindObjectsOfTypeCompat<UIDocument>();
            foreach (var d in docs)
            {
                if (d == default || d.rootVisualElement == default) continue;
                if (bottomCharacterButton == default)
                    bottomCharacterButton = d.rootVisualElement.Q<Button>("CharacterButton")
                                             ?? d.rootVisualElement.Q<Button>("Character")
                                             ?? d.rootVisualElement.Q<Button>(className: "character-button")
                                             ?? d.rootVisualElement.Q<Button>(className: "nav-btn--character")
                                             ?? FindButtonInDocumentByText(d, "character");

                if (bottomItemsButton == default)
                    bottomItemsButton = d.rootVisualElement.Q<Button>("ItemsButton")
                                        ?? d.rootVisualElement.Q<Button>("InventoryButton")
                                        ?? d.rootVisualElement.Q<Button>(className: "items-button")
                                        ?? d.rootVisualElement.Q<Button>(className: "nav-btn--inventory")
                                        ?? FindButtonInDocumentByText(d, "items")
                                        ?? FindButtonInDocumentByText(d, "inventory");

                if (bottomMenuButton == default)
                    bottomMenuButton = d.rootVisualElement.Q<Button>("MenuButton")
                                       ?? d.rootVisualElement.Q<Button>("Menu")
                                       ?? d.rootVisualElement.Q<Button>(className: "menu-button")
                                       ?? d.rootVisualElement.Q<Button>(className: "nav-btn--settings")
                                       ?? FindButtonInDocumentByText(d, "menu")
                                       ?? FindButtonInDocumentByText(d, "pause");

                if (bottomCharacterButton != default && bottomItemsButton != default && bottomMenuButton != default)
                {
                    if (verboseLogging) Debug.Log($"[UIController] Found bottom buttons in UIDocument: {d.name}");
                    break;
                }
            }
        }

        // Wire handlers (guard against double-binding with MLPGameUI)
        bool mlpOwnsBottomBar = MyGameNamespace.MLPGameUI.Instance != default;
        if (verboseLogging) Debug.Log($"[UIController] MLPGameUI.Instance present: {mlpOwnsBottomBar}");
        if (mlpOwnsBottomBar && verboseLogging) Debug.Log("[UIController] MLPGameUI owns all bottom buttons - UIController will not wire Character/Menu buttons but will still wire Items for redundancy.");

        if (verboseLogging)
        {
            Debug.Log($"[UIController] Bottom buttons found - Character: {(bottomCharacterButton!=default? bottomCharacterButton.name+"/"+bottomCharacterButton.text + " enabled:" + bottomCharacterButton.enabledSelf : "null")}, Items: {(bottomItemsButton!=default? bottomItemsButton.name+"/"+bottomItemsButton.text + " enabled:" + bottomItemsButton.enabledSelf : "null")}, Menu: {(bottomMenuButton!=default? bottomMenuButton.name+"/"+bottomMenuButton.text + " enabled:" + bottomMenuButton.enabledSelf : "null")}");
        }

        // Ensure buttons are enabled
        if (bottomCharacterButton != default) 
        {
            bottomCharacterButton.SetEnabled(true);
            bottomCharacterButton.pickingMode = PickingMode.Position;
        }
        if (bottomItemsButton != default) 
        {
            bottomItemsButton.SetEnabled(true);
            bottomItemsButton.pickingMode = PickingMode.Position;
        }
        if (bottomMenuButton != default) 
        {
            bottomMenuButton.SetEnabled(true);
            bottomMenuButton.pickingMode = PickingMode.Position;
        }

        // Character - only wire if MLPGameUI is not present
        if (!mlpOwnsBottomBar)
        {
            if (bottomCharacterButton != default)
            {
                bottomCharacterButton.clicked -= OnBottomCharacterClicked;
                bottomCharacterButton.clicked += OnBottomCharacterClicked;
                if (verboseLogging) Debug.Log("[UIController] Wired Character bottom button.");
            }
            else if (verboseLogging) Debug.LogWarning("[UIController] Character bottom button not found.");
        }

        // Items - always wire as fallback (MLPGameUI may not handle this)
        if (bottomItemsButton != default)
        {
            bottomItemsButton.clicked -= OnBottomItemsClicked;
            bottomItemsButton.clicked += OnBottomItemsClicked;
            if (verboseLogging) Debug.Log("[UIController] Wired Items bottom button.");
        }
        else if (verboseLogging) Debug.LogWarning("[UIController] Items bottom button not found.");

        // Menu - only wire if MLPGameUI is not present
        if (!mlpOwnsBottomBar)
        {
            if (bottomMenuButton != default)
            {
                bottomMenuButton.clicked -= OnBottomMenuClicked;
                bottomMenuButton.clicked += OnBottomMenuClicked;
                if (verboseLogging) Debug.Log("[UIController] Wired Menu bottom button.");
            }
            else if (verboseLogging) Debug.LogWarning("[UIController] Menu bottom button not found.");
        }
    }
    #endregion

    #region Player UI Updates

    public void UpdatePlayerUI(PlayerCharacter player)
    {
        if (player == default) return;
        if (!isInitialized)
        {
            // If not initialized yet, cache for later update
            currentPlayer = player;
            return;
        }

        currentPlayer = player;
        UpdateCharacterInfo();
        UpdateStats();
        UpdateAttributes();
        UpdatePortrait();

        if (verboseLogging) Debug.Log($"UIController: Updated UI for {player.name}");
    }

    public void UpdateInventoryUI(List<InventoryItem> items)
    {
        // This UIController doesn't handle inventory display - that's handled by InventoryScreenController
        if (verboseLogging) Debug.Log($"UIController: Inventory update notification ({items?.Count ?? 0} items)");
    }

    private void UpdateCharacterInfo()
    {
        if (currentPlayer == default) return;

        if (characterName != default) characterName.text = currentPlayer.name ?? "Unnamed";
        if (raceLabel != default) raceLabel.text = $"Race: {GetDisplayRaceName(currentPlayer.race)}";
        if (levelLabel != default) levelLabel.text = $"Level {currentPlayer.level}";
        if (bitsValue != default) bitsValue.text = currentPlayer.bits.ToString();
    }

    private void UpdateStats()
    {
        if (currentPlayer?.gameStats == default) return;

        var stats = currentPlayer.gameStats;

        // Update text values
        if (healthValue != default) healthValue.text = $"{stats.health}/{stats.maxHealth}";
        if (staminaValue != default) staminaValue.text = $"{stats.energy}/{stats.maxEnergy}";
        if (manaValue != default) manaValue.text = $"{stats.magic}/{stats.maxMagic}";
        if (harmonyValue != default) harmonyValue.text = $"{stats.friendship}/{stats.maxFriendship}";
        if (discordValue != default) discordValue.text = $"{stats.corruption}/{stats.maxCorruption}";

        // Animate bars
        AnimateBarToValue("health", stats.health, stats.maxHealth);
        AnimateBarToValue("energy", stats.energy, stats.maxEnergy);
        AnimateBarToValue("magic", stats.magic, stats.maxMagic);
        AnimateBarToValue("friendship", stats.friendship, stats.maxFriendship);
        AnimateBarToValue("discord", stats.corruption, stats.maxCorruption);
    }

    private void UpdateAttributes()
    {
        if (currentPlayer?.stats == default) return;

        var s = currentPlayer.stats;

        if (strengthValue != default) strengthValue.text = s.GetTotalStat(StatType.Strength).ToString();
        if (dexterityValue != default) dexterityValue.text = s.GetTotalStat(StatType.Dexterity).ToString();
        if (constitutionValue != default) constitutionValue.text = s.GetTotalStat(StatType.Constitution).ToString();
        if (intelligenceValue != default) intelligenceValue.text = s.GetTotalStat(StatType.Intelligence).ToString();
        if (wisdomValue != default) wisdomValue.text = s.GetTotalStat(StatType.Wisdom).ToString();
        if (charismaValue != default) charismaValue.text = s.GetTotalStat(StatType.Charisma).ToString();
    }

    private void UpdatePortrait()
    {
        if (currentPlayer == default || characterPortrait == default) return;

        // Prefer centralized portrait loader (handles Sprites and multiple folder conventions)
        try
        {
            // Try to load a Sprite via PortraitLoader (returns Sprite -> use its texture)
            if (MyGameNamespace.UI.PortraitLoader.TryLoadPortrait(currentPlayer.race.ToString(), currentPlayer.gender ?? string.Empty, out var sprite))
            {
                if (sprite != null && sprite.texture != null)
                {
                    characterPortrait.scaleMode = ScaleMode.ScaleToFit;
                    characterPortrait.image = sprite.texture;
                    return;
                }
            }
        }
        catch { /* don't fail UI if loader misbehaves */ }

        // Fallback: check any manually-assigned Texture2D fields first
        var portraitTexture = GetPortraitForRace(currentPlayer.race);
        if (portraitTexture != default)
        {
            characterPortrait.scaleMode = ScaleMode.ScaleToFit;
            characterPortrait.image = portraitTexture;
            return;
        }

        // Final fallback: try direct Resources lookups (race_gender, race)
        string raceLower = currentPlayer.race.ToString().ToLowerInvariant();
        string genderLower = string.IsNullOrEmpty(currentPlayer.gender) ? "unknown" : currentPlayer.gender.ToLowerInvariant();

        var resourceTexture = Resources.Load<Texture2D>($"Portraits/{raceLower}_{genderLower}") ??
                             Resources.Load<Texture2D>($"Portraits/{raceLower}");

        if (resourceTexture != default)
        {
            characterPortrait.scaleMode = ScaleMode.ScaleToFit;
            characterPortrait.image = resourceTexture;
        }
    }

    private string GetDisplayRaceName(RaceType race)
    {
        return race switch
        {
            RaceType.EarthPony => "Earth Pony",
            RaceType.BatPony => "Bat Pony",
            _ => race.ToString()
        };
    }

    private Texture2D GetPortraitForRace(RaceType race)
    {
        return race switch
        {
            RaceType.EarthPony => earthPonyPortrait,
            RaceType.Unicorn => unicornPortrait,
            RaceType.Pegasus => pegasusPortrait,
            RaceType.BatPony => batPonyPortrait,
            RaceType.Griffon => griffonPortrait,
            RaceType.Dragon => dragonPortrait,
            RaceType.Human => humanPortrait,
            _ => null
        };
    }

    /// <summary>
    /// Applies simple template tokens in the story text based on the current player's attributes.
    /// Supported tokens: {name}, {race}, {gender}, {unicorn_horn}, {pegasus_wings}.
    /// Unicorn horn text is inserted only if the player's race is Unicorn; otherwise it is removed.
    /// Pegasus wings text is inserted only if the player's race is Pegasus; otherwise it is removed.
    /// </summary>
    private string ApplyStoryTokens(string rawContent)
    {
        if (string.IsNullOrEmpty(rawContent)) return rawContent;

        var player = currentPlayer;
        if (player == default) return rawContent;

        string output = rawContent;

        // Name token
        if (!string.IsNullOrEmpty(player.name))
            output = output.Replace("{name}", player.name);
        else
            output = output.Replace("{name}", "Anon");

        // Race token
        output = output.Replace("{race}", player.race.ToString());

        // Gender token (may be empty)
        string gender = player.gender ?? "";
        output = output.Replace("{gender}", gender);

        // Racial features: horns and wings
        // Insert a short descriptive snippet if the player's race has the feature, otherwise remove the token completely.
        if (player.race == RaceType.Unicorn)
        {
            output = output.Replace("{unicorn_horn}", "A spiral horn catches the light. ");
        }
        else
        {
            output = output.Replace("{unicorn_horn}", "");
        }

        if (player.race == RaceType.Pegasus)
        {
            output = output.Replace("{pegasus_wings}", "Feathers flex at your sides. ");
        }
        else
        {
            output = output.Replace("{pegasus_wings}", "");
        }

        return output;
    }

    #endregion

    #region Story Display

    public void UpdateStoryDisplay(StoryNode node)
    {
        if (node == default || !isInitialized) return;

        if (storyTitleLabel != default)
            storyTitleLabel.text = string.IsNullOrEmpty(node.title) ? "" : node.title;

        // Render the story content into the appropriate container.  If a rich story body is present,
        // use the RichStoryRenderer to support inline images and formatted text; otherwise fall back
        // to the legacy storyText label.
        string processed = ApplyStoryTokens(node.content);
        if (storyBody != default)
        {
            // Clear legacy elements to avoid overlap
            if (storyImage != default)
            {
                storyImage.image = null;
                storyImage.style.display = DisplayStyle.None;
            }
            if (storyText != default)
            {
                storyText.text = string.Empty;
                storyText.style.display = DisplayStyle.None;
            }
            // Populate the story body using the renderer
            MyGameNamespace.RichStoryRenderer.Render(storyBody, processed);
        }
        else if (storyText != default)
        {
            // Legacy fallback: no rich body container available
            storyText.text = processed;
        }

        // Update story image only when storyBody is absent; otherwise images are rendered inline
        if (storyBody == default)
        {
            UpdateStoryImage(node);
        }
        UpdateChoiceButtons(node.choices ?? new List<StoryChoice>());
    }

    private void UpdateStoryImage(StoryNode node)
    {
        if (storyImage == default) return;

        if (string.IsNullOrEmpty(node?.imagePath))
        {
            storyImage.image = null;
            storyImage.style.display = DisplayStyle.None;
            return;
        }

        var texture = Resources.Load<Texture2D>(node.imagePath);
        if (texture != default)
        {
            storyImage.image = texture;
            storyImage.style.display = DisplayStyle.Flex;
        }
        else
        {
            storyImage.image = null;
            storyImage.style.display = DisplayStyle.None;
        }
    }

    private void UpdateChoiceButtons(List<StoryChoice> choices)
    {
        for (int i = 0; i < choiceButtons.Count; i++)
        {
            var btn = choiceButtons[i];

            if (i < choices.Count && choices[i].isEnabled)
            {
                string label = choices[i].text ?? "";
                btn.text = $"{hotkeyLabels[i]} {label}";
                btn.SetEnabled(true);
                btn.style.display = DisplayStyle.Flex;
                btn.tooltip = GetChoiceTooltip(choices[i]);
            }
            else
            {
                btn.text = hotkeyLabels[i];
                btn.SetEnabled(false);
                btn.style.display = DisplayStyle.None;
                btn.tooltip = "";
            }
        }
    }

    private string GetChoiceTooltip(object choice)
    {
        if (choice == default) return "";

        var type = choice.GetType();
        var properties = type.GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);

        foreach (var prop in properties)
        {
            if ((prop.Name.ToLower().Contains("tooltip") || prop.Name.ToLower().Contains("description"))
                && prop.PropertyType == typeof(string))
            {
                var value = prop.GetValue(choice) as string;
                if (!string.IsNullOrEmpty(value)) return value;
            }
        }

        return "";
    }

    #endregion

    #region Choice System

    private void SetupChoiceButtons()
    {
        choiceButtons.Clear();

        for (int i = 1; i <= 8; i++)
        {
            var button = root.Q<Button>($"Choice{i}");
            if (button == default && choicesPanel != default)
            {
                button = new Button { name = $"Choice{i}", text = hotkeyLabels[i - 1] };
                button.AddToClassList("choice-btn");
                choicesPanel.Add(button);
            }

            if (button != default)
            {
                int idx = i - 1;
                int capturedIdx = idx; // capture for lambda
                button.clicked += () => OnChoiceClicked(capturedIdx);
                choiceButtons.Add(button);
            }
        }

        // Hide buttons initially
        foreach (var btn in choiceButtons)
        {
            btn.style.display = DisplayStyle.None;
            btn.SetEnabled(false);
        }

        if (verboseLogging) Debug.Log($"UIController: Setup {choiceButtons.Count} choice buttons");
    }

    private void SetupHotkeys()
    {
        if (root != default)
            root.RegisterCallback<KeyDownEvent>(OnKeyDown);
    }

    private void OnKeyDown(KeyDownEvent evt)
    {
        int? index = evt.keyCode switch
        {
            KeyCode.Alpha1 => 0,
            KeyCode.Alpha2 => 1,
            KeyCode.Alpha3 => 2,
            KeyCode.Alpha4 => 3,
            KeyCode.Alpha5 => 4,
            KeyCode.Alpha6 => 5,
            KeyCode.Alpha7 => 6,
            KeyCode.Alpha8 => 7,
            _ => null
        };

        if (index.HasValue)
        {
            int i = index.Value;
            if (i >= 0 && i < choiceButtons.Count && choiceButtons[i].enabledSelf)
            {
                OnChoiceClicked(i);
                evt.StopImmediatePropagation();
            }
        }
    }

    private void OnChoiceClicked(int choiceIndex)
    {
        var storyManager = StoryManager.Instance;
        var currentNode = storyManager?.CurrentNode;

        if (storyManager != default && currentNode != default &&
            choiceIndex >= 0 && choiceIndex < currentNode.choices.Count)
        {
            var choice = currentNode.choices[choiceIndex];

            // Let GameManager handle the choice effects
            GameManager.Instance?.OnStoryChoiceMade(choice);

            if (verboseLogging) Debug.Log($"Choice selected: {choice.text}");
        }
    }

    #endregion

    #region Animation System

    private void SetupAnimatedBars()
    {
        barAnimations.Clear();

        SetupBarAnimation("health", healthBar, healthColor, healthLowColor, healthColor, 0.25f, 0.75f);
        SetupBarAnimation("energy", staminaBar, energyColor, MultiplyColor(energyColor, 0.7f), energyColor, 0.25f, 0.75f);
        SetupBarAnimation("magic", manaBar, magicColor, MultiplyColor(magicColor, 0.7f), magicColor, 0.25f, 0.75f);
        SetupBarAnimation("friendship", harmonyBar, friendshipColor, MultiplyColor(friendshipColor, 0.7f), BrightenColor(friendshipColor, 1.2f), 0.25f, 0.8f);
        SetupBarAnimation("discord", discordBar, discordColor, discordColor, discordHighColor, 0.25f, 0.6f);

        if (enableBarPulse && pulseCoroutine == default)
            pulseCoroutine = StartCoroutine(PulseEffectCoroutine());

        if (verboseLogging) Debug.Log($"Setup {barAnimations.Count} animated bars");
    }

    private void SetupBarAnimation(string key, ProgressBar bar, Color normalColor, Color lowColor, Color highColor, float lowThreshold, float highThreshold)
    {
        if (bar == default) return;

        var data = new BarAnimationData
        {
            bar = bar,
            fill = bar.Q(className: "unity-progress-bar__progress"),
            normalColor = normalColor,
            lowColor = lowColor,
            highColor = highColor,
            lowThreshold = lowThreshold,
            highThreshold = highThreshold
        };

        data.currentValue = bar.value;
        data.maxValue = bar.highValue > 0 ? bar.highValue : 100f;
        barAnimations[key] = data;

        SetupBarVisuals(data);
    }

    private void SetupBarVisuals(BarAnimationData data)
    {
        if (data.bar == default) return;

        var barStyle = data.bar.style;
        barStyle.backgroundColor = new Color(0, 0, 0, 0.3f);
        barStyle.borderTopLeftRadius = barStyle.borderTopRightRadius =
        barStyle.borderBottomLeftRadius = barStyle.borderBottomRightRadius = 8;

        var borderColor = new Color(1f, 1f, 1f, 0.2f);
        barStyle.borderLeftColor = barStyle.borderRightColor =
        barStyle.borderTopColor = barStyle.borderBottomColor = borderColor;
        barStyle.borderLeftWidth = barStyle.borderRightWidth =
        barStyle.borderTopWidth = barStyle.borderBottomWidth = 1;

        if (data.fill == default)
        {
            data.bar.schedule.Execute(() =>
            {
                data.fill = data.bar.Q(className: "unity-progress-bar__progress");
                if (data.fill != default)
                    SetupFillVisuals(data);
            });
        }
        else
        {
            SetupFillVisuals(data);
        }
    }

    private void SetupFillVisuals(BarAnimationData data)
    {
        var fillStyle = data.fill.style;
        fillStyle.backgroundColor = data.normalColor;
        fillStyle.borderTopLeftRadius = fillStyle.borderTopRightRadius =
        fillStyle.borderBottomLeftRadius = fillStyle.borderBottomRightRadius = 6;
    }

    private void AnimateBarToValue(string barKey, float newValue, float maxValue)
    {
        if (!barAnimations.TryGetValue(barKey, out var data)) return;

        data.targetValue = newValue;
        data.maxValue = Mathf.Max(1f, maxValue);

        if (data.animationCoroutine != default)
            StopCoroutine(data.animationCoroutine);

        data.animationCoroutine = StartCoroutine(AnimateBarCoroutine(data));
    }

    private IEnumerator AnimateBarCoroutine(BarAnimationData data)
    {
        data.isAnimating = true;
        float startValue = data.currentValue;
        float elapsedTime = 0f;

        if (data.fill == default)
            data.fill = data.bar.Q(className: "unity-progress-bar__progress");

        while (elapsedTime < barAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / barAnimationDuration);
            float curveValue = barAnimationCurve?.Evaluate(progress) ?? progress;

            data.currentValue = Mathf.Lerp(startValue, data.targetValue, curveValue);
            data.bar.highValue = data.maxValue;
            data.bar.value = data.currentValue;

            if (enableColorTransitions)
                UpdateBarColor(data);

            yield return null;
        }

        data.currentValue = data.targetValue;
        data.bar.highValue = data.maxValue;
        data.bar.value = data.targetValue;

        if (enableColorTransitions)
            UpdateBarColor(data);

        data.isAnimating = false;
        data.animationCoroutine = null;
    }

    private void UpdateBarColor(BarAnimationData data)
    {
        if (data.fill == default || data.maxValue <= 0f) return;

        float percentage = data.currentValue / data.maxValue;
        Color targetColor;

        if (percentage <= data.lowThreshold)
        {
            float t = percentage / Mathf.Max(0.0001f, data.lowThreshold);
            targetColor = Color.Lerp(data.lowColor, data.normalColor, t);
        }
        else if (percentage >= data.highThreshold)
        {
            float t = (percentage - data.highThreshold) / Mathf.Max(0.0001f, (1f - data.highThreshold));
            targetColor = Color.Lerp(data.normalColor, data.highColor, t);
        }
        else
        {
            targetColor = data.normalColor;
        }

        data.fill.style.backgroundColor = targetColor;
    }

    private IEnumerator PulseEffectCoroutine()
    {
        while (true)
        {
            float time = Time.time * 2f;
            float pulse = 0.85f + 0.15f * Mathf.Sin(time);

            foreach (var kv in barAnimations)
            {
                var key = kv.Key;
                var data = kv.Value;
                if (data.fill == default || data.maxValue <= 0f) continue;

                float percentage = data.currentValue / data.maxValue;

                bool critical = key switch
                {
                    "health" => percentage <= data.lowThreshold,
                    "discord" => percentage >= data.highThreshold,
                    _ => false
                };

                data.fill.style.opacity = critical ? pulse : 1f;
            }

            yield return null;
        }
    }

    private static Color MultiplyColor(Color color, float multiplier)
    {
        return new Color(color.r * multiplier, color.g * multiplier, color.b * multiplier, color.a);
    }

    private static Color BrightenColor(Color color, float multiplier)
    {
        return new Color(Mathf.Clamp01(color.r * multiplier), Mathf.Clamp01(color.g * multiplier), Mathf.Clamp01(color.b * multiplier), color.a);
    }

    #endregion

    #region Event System

    private IEnumerator WaitForGameEventSystemAndSubscribe()
    {
        int attempts = 0;
        while (GameEventSystem.Instance == default && attempts < 30)
        {
            attempts++;
            yield return new WaitForSeconds(0.05f);
        }

        if (GameEventSystem.Instance != default)
        {
            // Subscribe to GameEventSystem events using the += operator. The events
            // defined in GameEventSystem are plain Action types, so AddListener
            // cannot be used here.
            GameEventSystem.Instance.OnPlayerStatsChanged += OnPlayerStatsChangedEmpty;
            GameEventSystem.Instance.OnStoryNodeChanged += OnStoryNodeChanged;
            GameEventSystem.Instance.OnPlayerCharacterChanged += OnPlayerCharacterChangedObj;
            if (verboseLogging) Debug.Log("UIController: Subscribed to GameEventSystem events");

            // If a story node already exists (e.g., StoryManager set it in Awake), update
            // the display immediately. Otherwise the first node change may have been
            // missed before this subscription. We cannot assume StoryManager.Instance is
            // ready here, so check for nulls.  Use PlayerState.Current to provide
            // player context for token replacement if available.
            try
            {
                // Ensure a StoryManager exists.  If one is not present in the scene,
                // create a new GameObject with a StoryManager component.  This
                // guarantees that story data is loaded and a start node is set.
                var sm = global::StoryManager.Instance;
                if (sm == default)
                {
                    var storyObj = new GameObject("StoryManager");
                    sm = storyObj.AddComponent<global::StoryManager>();
                }

                if (sm != default)
                {
                    // If a current node already exists, update the display immediately.
                    if (sm.CurrentNode != default)
                    {
                        UpdateStoryDisplay(sm.CurrentNode);
                    }
                    // Otherwise, if story data is loaded but no node is active yet, set the
                    // current node to the start node.  This ensures the first story node
                    // appears in the HUD even if StoryManager has not yet called SetCurrentNode.
                    else if (sm.Data != default)
                    {
                        var start = sm.Data.GetStartNode();
                        sm.SetCurrentNode(start);
                    }
                }
            }
            catch
            {
                // ignore exceptions retrieving story data
            }
        }
        else
        {
            if (verboseLogging) Debug.LogWarning("UIController: GameEventSystem.Instance was not found during initialization");
        }
    }

    private void OnPlayerStatsChanged(PlayerCharacter player) => UpdatePlayerUI(player);
    private void OnPlayerCharacterChanged(PlayerCharacter player) => UpdatePlayerUI(player);

    private void OnStoryNodeChanged(StoryNode node)
    {
        if (verboseLogging)
        {
            var count = node?.choices?.Count ?? 0;
            Debug.Log($"UIController: Story node changed to '{node?.id}' with {count} choices");
        }
        UpdateStoryDisplay(node);
    }

    // Bridging handlers for GameEventSystem events.  GameEventSystem defines
    // untyped events (Action and Action<object>), so we need to convert them
    // into calls to our typed handlers.  These methods subscribe via
    // WaitForGameEventSystemAndSubscribe and are unsubscribed in OnDisable/OnDestroy.
    private void OnPlayerStatsChangedEmpty()
    {
        // When player stats change, refresh the UI for the current player.  We
        // fetch the player from the GameManager because the event carries no
        // payload in the canonical GameEventSystem.
        var player = GameManager.Instance?.GetPlayer();
        if (player == default)
        {
            player = MyGameNamespace.PlayerState.Current;
        }
        if (player != default)
        {
            UpdatePlayerUI(player);
        }
    }

    private void OnPlayerCharacterChangedObj(object payload)
    {
        // Cast the payload to PlayerCharacter if possible and refresh the UI.
        if (payload is PlayerCharacter pc)
        {
            UpdatePlayerUI(pc);
        }
    }

    #endregion

    #region Styling and Utilities

    private void ApplyUIStyles()
    {
        if (characterPortrait != default)
        {
            var style = characterPortrait.style;
            style.width = style.height = 150;
            style.borderTopLeftRadius = style.borderTopRightRadius =
            style.borderBottomLeftRadius = style.borderBottomRightRadius = 12;

            var borderColor = new Color(0.7f, 0.5f, 1f, 1f);
            style.borderTopColor = style.borderBottomColor =
            style.borderLeftColor = style.borderRightColor = borderColor;
            style.borderTopWidth = style.borderBottomWidth =
            style.borderLeftWidth = style.borderRightWidth = 3;
        }

        if (storyTitleLabel != default) storyTitleLabel.style.fontSize = headerFontSize;
        if (storyText != default) storyText.style.fontSize = storyFontSize;

        foreach (var btn in choiceButtons)
            btn.style.fontSize = choiceFontSize;
    }

    public void Show() { if (root != default) root.style.display = DisplayStyle.Flex; }
    public void Hide() { if (root != default) root.style.display = DisplayStyle.None; }

    #endregion

    #region Cleanup

    private void OnDisable()
    {
        if (pulseCoroutine != default)
        {
            StopCoroutine(pulseCoroutine);
            pulseCoroutine = null;
        }

        foreach (var data in barAnimations.Values)
        {
            if (data.animationCoroutine != default)
            {
                StopCoroutine(data.animationCoroutine);
                data.animationCoroutine = null;
            }
        }

        if (root != default)
            root.UnregisterCallback<KeyDownEvent>(OnKeyDown);

        if (GameEventSystem.Instance != default)
        {
            // Unsubscribe from GameEventSystem events using the -= operator
            GameEventSystem.Instance.OnPlayerStatsChanged -= OnPlayerStatsChangedEmpty;
            GameEventSystem.Instance.OnStoryNodeChanged -= OnStoryNodeChanged;
            GameEventSystem.Instance.OnPlayerCharacterChanged -= OnPlayerCharacterChangedObj;
        }
    }

    private void OnDestroy()
    {
        if (GameEventSystem.Instance != default)
        {
            GameEventSystem.Instance.OnPlayerStatsChanged -= OnPlayerStatsChangedEmpty;
            GameEventSystem.Instance.OnStoryNodeChanged -= OnStoryNodeChanged;
            GameEventSystem.Instance.OnPlayerCharacterChanged -= OnPlayerCharacterChangedObj;
        }
    }

    #endregion

    #region Debug Methods

    [ContextMenu("Test UI Elements")]
    public void TestUIElements()
    {
        Debug.Log("=== UI ELEMENTS TEST ===");
        Debug.Log($"Root: {root != default}");
        Debug.Log($"Character Portrait: {characterPortrait != default}");
        Debug.Log($"Character Name: {characterName != default}");
        Debug.Log($"Health Bar: {healthBar != default}");
        Debug.Log($"Story Text: {storyText != default}");
        Debug.Log($"Choices Panel: {choicesPanel != default}");
        Debug.Log($"Choice Buttons: {choiceButtons.Count}");
        Debug.Log($"Current Player: {currentPlayer?.name ?? "None"}");
        Debug.Log($"BottomCharBtn: {(bottomCharacterButton != default ? bottomCharacterButton.name + " / " + bottomCharacterButton.text : "null")}");
        Debug.Log($"BottomItemsBtn: {(bottomItemsButton != default ? bottomItemsButton.name + " / " + bottomItemsButton.text : "null")}");
        Debug.Log($"BottomMenuBtn: {(bottomMenuButton != default ? bottomMenuButton.name + " / " + bottomMenuButton.text : "null")}");
    }

    [ContextMenu("Force Player Update")]
    public void ForcePlayerUpdate()
    {
        var player = GameManager.Instance?.GetPlayer();
        if (player == default)
        {
            player = MyGameNamespace.PlayerState.Current;
        }
        if (player != default)
        {
            UpdatePlayerUI(player);
            Debug.Log("Forced player UI update");
        }
        else
        {
            Debug.LogWarning("No player available for update");
        }
    }

    #endregion
}