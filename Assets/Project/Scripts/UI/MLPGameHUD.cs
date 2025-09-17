using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using MyGameNamespace;

namespace MyGameNamespace.UI
{
    /// <summary>
    /// Main HUD controller inspired by TiTS layout but adapted for MLP game
    /// Features: Character portrait, location display, minimap, system buttons
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class MLPGameHUD : MonoBehaviour
    {
        [Header("UI Document")]
        [SerializeField] private UIDocument uiDocument;

        [Header("HUD Settings")]
        [SerializeField] private bool showMinimap = true;
        [SerializeField] private bool showLocationInfo = true;
        [SerializeField] private bool showSystemButtons = true;

        // Main HUD elements
        private VisualElement root;
        private VisualElement leftPanel;
        private VisualElement rightPanel;
        private VisualElement centerPanel;

        // Left Panel Components
        private VisualElement characterPortrait;
        private Label locationName;
        private Label locationDescription;
        private SimpleMinimap minimap;
        private VisualElement systemButtons;

        // Right Panel Components
        private Label characterName;
        private VisualElement statsContainer;
        private VisualElement statusEffects;
        private VisualElement quickActions;

        // Center Panel Components
        private VisualElement mainDisplay;

        // System buttons
        private Button saveButton;
        private Button loadButton;
        private Button menuButton;
        private Button inventoryButton;
        private Button characterButton;
        private Button mapButton;

        // References
        private CharacterSystem characterSystem;
        private MapSystem mapSystem;
        private GameEventSystem eventSystem;

        private void Awake()
        {
            if (uiDocument == null)
                uiDocument = GetComponent<UIDocument>();
        }

        private void Start()
        {
            InitializeHUD();
            SetupEventListeners();
        }

        private void InitializeHUD()
        {
            if (uiDocument == null || uiDocument.rootVisualElement == null)
            {
                Debug.LogError("[MLPGameHUD] No UIDocument found!");
                return;
            }

            root = uiDocument.rootVisualElement;

            // Get system references
            characterSystem = FindFirstObjectByType<CharacterSystem>();
            mapSystem = FindFirstObjectByType<MapSystem>();
            eventSystem = GameEventSystem.Instance;

            // Create main layout structure
            CreateMainLayout();

            // Initialize components
            InitializeLeftPanel();
            InitializeRightPanel();
            InitializeCenterPanel();

            // Setup system buttons
            SetupSystemButtons();

            // Create notification system
            CreateNotificationSystem();

            // Initial data population
            RefreshHUD();

            Debug.Log("[MLPGameHUD] HUD initialized successfully");
        }

        private void CreateMainLayout()
        {
            // Create main container
            var mainContainer = new VisualElement();
            mainContainer.name = "hud-main-container";
            mainContainer.AddToClassList("hud-main-container");
            mainContainer.style.flexDirection = FlexDirection.Row;
            mainContainer.style.width = Length.Percent(100);
            mainContainer.style.height = Length.Percent(100);
            root.Add(mainContainer);

            // Left panel (character info, minimap)
            leftPanel = new VisualElement();
            leftPanel.name = "hud-left-panel";
            leftPanel.AddToClassList("hud-left-panel");
            leftPanel.style.flexDirection = FlexDirection.Column;
            leftPanel.style.width = 300;
            leftPanel.style.minWidth = 250;
            mainContainer.Add(leftPanel);

            // Center panel (main game display)
            centerPanel = new VisualElement();
            centerPanel.name = "hud-center-panel";
            centerPanel.AddToClassList("hud-center-panel");
            centerPanel.style.flexGrow = 1;
            centerPanel.style.flexDirection = FlexDirection.Column;
            mainContainer.Add(centerPanel);

            // Right panel (stats, actions)
            rightPanel = new VisualElement();
            rightPanel.name = "hud-right-panel";
            rightPanel.AddToClassList("hud-right-panel");
            rightPanel.style.flexDirection = FlexDirection.Column;
            rightPanel.style.width = 300;
            rightPanel.style.minWidth = 250;
            mainContainer.Add(rightPanel);
        }

        private void InitializeLeftPanel()
        {
            // Character portrait section
            var portraitSection = new VisualElement();
            portraitSection.name = "portrait-section";
            portraitSection.AddToClassList("hud-section");
            leftPanel.Add(portraitSection);

            characterPortrait = new VisualElement();
            characterPortrait.name = "character-portrait";
            characterPortrait.AddToClassList("character-portrait");
            characterPortrait.style.width = 120;
            characterPortrait.style.height = 120;
            characterPortrait.style.backgroundColor = new Color(108/255f, 92/255f, 110/255f); /* Twilight button border color for portrait background */
            characterPortrait.style.borderTopLeftRadius = 10;
            characterPortrait.style.borderTopRightRadius = 10;
            characterPortrait.style.borderBottomLeftRadius = 10;
            characterPortrait.style.borderBottomRightRadius = 10;
            portraitSection.Add(characterPortrait);

            // Location info section
            if (showLocationInfo)
            {
                var locationSection = new VisualElement();
                locationSection.name = "location-section";
                locationSection.AddToClassList("hud-section");
                leftPanel.Add(locationSection);

                locationName = new Label("Ponyville");
                locationName.name = "location-name";
                locationName.AddToClassList("location-name");
                locationSection.Add(locationName);

                locationDescription = new Label("A peaceful town in Equestria");
                locationDescription.name = "location-description";
                locationDescription.AddToClassList("location-description");
                locationDescription.style.fontSize = 12;
                locationDescription.style.color = new Color(180/255f, 170/255f, 190/255f); /* Twilight text color for location description */
                locationSection.Add(locationDescription);
            }

            // Minimap section
            if (showMinimap)
            {
                var minimapSection = new VisualElement();
                minimapSection.name = "minimap-section";
                minimapSection.AddToClassList("hud-section");
                leftPanel.Add(minimapSection);

                var minimapLabel = new Label("Mini Map");
                minimapLabel.name = "minimap-label";
                minimapLabel.AddToClassList("section-label");
                minimapSection.Add(minimapLabel);

                // Create and add the minimap
                minimap = new SimpleMinimap();
                minimapSection.Add(minimap);
            }

            // System buttons section
            if (showSystemButtons)
            {
                systemButtons = new VisualElement();
                systemButtons.name = "system-buttons";
                systemButtons.AddToClassList("hud-section");
                systemButtons.style.flexDirection = FlexDirection.Column;
                leftPanel.Add(systemButtons);
            }
        }

        private void InitializeRightPanel()
        {
            // Character name section
            var nameSection = new VisualElement();
            nameSection.name = "character-name-section";
            nameSection.AddToClassList("hud-section");
            rightPanel.Add(nameSection);

            characterName = new Label("Twilight Sparkle");
            characterName.name = "character-name";
            characterName.AddToClassList("character-name");
            characterName.style.fontSize = 18;
            characterName.style.unityFontStyleAndWeight = FontStyle.Bold;
            nameSection.Add(characterName);

            // Stats section
            var statsSection = new VisualElement();
            statsSection.name = "stats-section";
            statsSection.AddToClassList("hud-section");
            rightPanel.Add(statsSection);

            var statsLabel = new Label("Stats");
            statsLabel.name = "stats-label";
            statsLabel.AddToClassList("section-label");
            statsSection.Add(statsLabel);

            statsContainer = new VisualElement();
            statsContainer.name = "stats-container";
            statsContainer.AddToClassList("stats-container");
            statsContainer.style.flexDirection = FlexDirection.Column;
            statsSection.Add(statsContainer);

            // Status effects section
            var statusSection = new VisualElement();
            statusSection.name = "status-section";
            statusSection.AddToClassList("hud-section");
            rightPanel.Add(statusSection);

            var statusLabel = new Label("Status Effects");
            statusLabel.name = "status-label";
            statusLabel.AddToClassList("section-label");
            statusSection.Add(statusLabel);

            statusEffects = new VisualElement();
            statusEffects.name = "status-effects";
            statusEffects.AddToClassList("status-effects");
            statusEffects.style.flexDirection = FlexDirection.Column;
            statusSection.Add(statusEffects);

            // Quick actions section
            var actionsSection = new VisualElement();
            actionsSection.name = "actions-section";
            actionsSection.AddToClassList("hud-section");
            rightPanel.Add(actionsSection);

            var actionsLabel = new Label("Quick Actions");
            actionsLabel.name = "actions-label";
            actionsLabel.AddToClassList("section-label");
            actionsSection.Add(actionsLabel);

            quickActions = new VisualElement();
            quickActions.name = "quick-actions";
            quickActions.AddToClassList("quick-actions");
            quickActions.style.flexDirection = FlexDirection.Column;
            actionsSection.Add(quickActions);
        }

        private void InitializeCenterPanel()
        {
            // Main display area (for game content)
            mainDisplay = new VisualElement();
            mainDisplay.name = "main-display";
            mainDisplay.AddToClassList("main-display");
            mainDisplay.style.flexGrow = 1;
            mainDisplay.style.backgroundColor = new Color(22/255f, 18/255f, 24/255f, 0.92f); /* Twilight menu card background for main display */
            centerPanel.Add(mainDisplay);

            var welcomeLabel = new Label("Welcome to Equestria!");
            welcomeLabel.name = "welcome-label";
            welcomeLabel.AddToClassList("welcome-label");
            welcomeLabel.style.fontSize = 24;
            welcomeLabel.style.color = new Color(244/255f, 162/255f, 232/255f); /* Twilight title color for welcome label */
            welcomeLabel.style.alignSelf = Align.Center;
            welcomeLabel.style.marginTop = 50;
            mainDisplay.Add(welcomeLabel);
        }

        private void SetupSystemButtons()
        {
            if (systemButtons == null) return;

            // Create system buttons grid
            var buttonsGrid = new VisualElement();
            buttonsGrid.name = "buttons-grid";
            buttonsGrid.AddToClassList("buttons-grid");
            buttonsGrid.style.flexDirection = FlexDirection.Row;
            buttonsGrid.style.flexWrap = Wrap.Wrap;
            systemButtons.Add(buttonsGrid);

            // Save button
            saveButton = new Button();
            saveButton.name = "save-button";
            saveButton.text = "💾";
            saveButton.AddToClassList("system-button");
            saveButton.tooltip = "Save Game";
            saveButton.clicked += OnSaveClicked;
            buttonsGrid.Add(saveButton);

            // Load button
            loadButton = new Button();
            loadButton.name = "load-button";
            loadButton.text = "📁";
            loadButton.AddToClassList("system-button");
            loadButton.tooltip = "Load Game";
            loadButton.clicked += OnLoadClicked;
            buttonsGrid.Add(loadButton);

            // Menu button
            menuButton = new Button();
            menuButton.name = "menu-button";
            menuButton.text = "☰";
            menuButton.AddToClassList("system-button");
            menuButton.tooltip = "Main Menu";
            menuButton.clicked += OnMenuClicked;
            buttonsGrid.Add(menuButton);

            // Inventory button
            inventoryButton = new Button();
            inventoryButton.name = "inventory-button";
            inventoryButton.text = "🎒";
            inventoryButton.AddToClassList("system-button");
            inventoryButton.tooltip = "Inventory";
            inventoryButton.clicked += OnInventoryClicked;
            buttonsGrid.Add(inventoryButton);

            // Character button
            characterButton = new Button();
            characterButton.name = "character-button";
            characterButton.text = "👤";
            characterButton.AddToClassList("system-button");
            characterButton.tooltip = "Character Sheet";
            characterButton.clicked += OnCharacterClicked;
            buttonsGrid.Add(characterButton);

            // Map button
            mapButton = new Button();
            mapButton.name = "map-button";
            mapButton.text = "🗺️";
            mapButton.AddToClassList("system-button");
            mapButton.tooltip = "World Map";
            mapButton.clicked += OnMapClicked;
            buttonsGrid.Add(mapButton);
        }

        private void SetupEventListeners()
        {
            if (eventSystem != null)
            {
                eventSystem.OnPlayerStatsChanged += OnPlayerStatsChanged;
                eventSystem.OnInventoryChanged += OnInventoryChanged;
            }
        }

        private void RefreshHUD()
        {
            UpdateCharacterInfo();
            UpdateLocationInfo();
            UpdateStats();
            UpdateMinimap();
        }

        private void UpdateCharacterInfo()
        {
            if (characterSystem != null)
            {
                var player = characterSystem.GetPlayerCharacter();
                if (player != null)
                {
                    characterName.text = player.name ?? "Unknown Pony";
                    // Update portrait here when portrait system is implemented
                }
            }
        }

        private void UpdateLocationInfo()
        {
            if (locationName != null && mapSystem != null)
            {
                // Update with current location from map system
                locationName.text = "Current Location";
                locationDescription.text = "Location description";
            }
        }

        private void UpdateStats()
        {
            if (statsContainer != null && characterSystem != null)
            {
                statsContainer.Clear();

                var player = characterSystem.GetPlayerCharacter();
                if (player != null)
                {
                    // Add stat bars (health, energy, etc.)
                    AddStatBar("Health", player.health, player.maxHealth);
                    AddStatBar("Energy", player.energy, player.maxEnergy);
                    AddStatBar("Level", player.level, player.level + 1);
                }
            }
        }

        private void AddStatBar(string statName, float current, float max)
        {
            var statRow = new VisualElement();
            statRow.name = $"{statName.ToLower()}-row";
            statRow.AddToClassList("stat-row");
            statRow.style.flexDirection = FlexDirection.Row;
            statRow.style.alignItems = Align.Center;
            statsContainer.Add(statRow);

            var statLabel = new Label($"{statName}:");
            statLabel.name = $"{statName.ToLower()}-label";
            statLabel.AddToClassList("stat-label");
            statLabel.style.width = 80;
            statRow.Add(statLabel);

            var statBar = new ProgressBar();
            statBar.name = $"{statName.ToLower()}-bar";
            statBar.AddToClassList("stat-bar");
            statBar.value = current;
            statBar.highValue = max;
            statBar.style.flexGrow = 1;
            statRow.Add(statBar);

            var statValue = new Label($"{current}/{max}");
            statValue.name = $"{statName.ToLower()}-value";
            statValue.AddToClassList("stat-value");
            statValue.style.width = 80;
            statValue.style.unityTextAlign = TextAnchor.MiddleRight;
            statRow.Add(statValue);
        }

        private void UpdateMinimap()
        {
            if (minimap != null)
            {
                minimap.RefreshMinimap();
            }
        }

        // Event handlers
        private void OnPlayerStatsChanged()
        {
            UpdateStats();
        }

        private void OnInventoryChanged()
        {
            // Update inventory-related UI elements
        }

        // Button click handlers
        private void OnSaveClicked()
        {
            Debug.Log("[MLPGameHUD] Save button clicked");
            bool success = SaveLoadSystem.SaveGame();
            if (success)
            {
                ShowNotification("Game saved successfully!", Color.green);
            }
            else
            {
                ShowNotification("Failed to save game!", Color.red);
            }
        }

        private void OnLoadClicked()
        {
            Debug.Log("[MLPGameHUD] Load button clicked");
            if (SaveLoadSystem.SaveExists())
            {
                bool success = SaveLoadSystem.LoadGame();
                if (success)
                {
                    ShowNotification("Game loaded successfully!", Color.green);
                    RefreshHUD(); // Refresh all HUD elements after loading
                }
                else
                {
                    ShowNotification("Failed to load game!", Color.red);
                }
            }
            else
            {
                ShowNotification("No save file found!", Color.yellow);
            }
        }

        private void OnMenuClicked()
        {
            Debug.Log("[MLPGameHUD] Menu button clicked");
            // Show main menu
        }

        private void OnInventoryClicked()
        {
            Debug.Log("[MLPGameHUD] Inventory button clicked");
            // Show inventory screen
        }

        private void OnCharacterClicked()
        {
            Debug.Log("[MLPGameHUD] Character button clicked");
            // Show character sheet
        }

        private void OnMapClicked()
        {
            Debug.Log("[MLPGameHUD] Map button clicked");
            // Show world map
        }

        // Notification system
        private Label notificationLabel;
        private VisualElement notificationContainer;

        private void CreateNotificationSystem()
        {
            // Create notification container
            notificationContainer = new VisualElement();
            notificationContainer.name = "notification-container";
            notificationContainer.AddToClassList("notification-container");
            notificationContainer.style.position = Position.Absolute;
            notificationContainer.style.top = 20;
            notificationContainer.style.right = 20;
            notificationContainer.style.width = 300;
            notificationContainer.style.maxHeight = 100;
            notificationContainer.style.zIndex = 1000;
            root.Add(notificationContainer);

            // Create notification label
            notificationLabel = new Label();
            notificationLabel.name = "notification-label";
            notificationLabel.AddToClassList("notification-label");
            notificationLabel.style.display = DisplayStyle.None;
            notificationLabel.style.fontSize = 14;
            notificationLabel.style.color = Color.white;
            notificationLabel.style.backgroundColor = new Color(22/255f, 18/255f, 24/255f, 0.9f); /* Twilight menu card background for notifications */
            notificationLabel.style.paddingTop = 10;
            notificationLabel.style.paddingBottom = 10;
            notificationLabel.style.paddingLeft = 15;
            notificationLabel.style.paddingRight = 15;
            notificationLabel.style.borderTopLeftRadius = 8;
            notificationLabel.style.borderTopRightRadius = 8;
            notificationLabel.style.borderBottomLeftRadius = 8;
            notificationLabel.style.borderBottomRightRadius = 8;
            notificationLabel.style.alignSelf = Align.FlexEnd;
            notificationContainer.Add(notificationLabel);
        }

        private void ShowNotification(string message, Color color)
        {
            if (notificationLabel == null) return;

            notificationLabel.text = message;
            notificationLabel.style.backgroundColor = new Color(color.r, color.g, color.b, 0.9f);
            notificationLabel.style.borderTopColor = color;
            notificationLabel.style.borderBottomColor = color;
            notificationLabel.style.borderLeftColor = color;
            notificationLabel.style.borderRightColor = color;
            notificationLabel.style.display = DisplayStyle.Flex;

            // Auto-hide after 3 seconds
            StartCoroutine(HideNotificationAfterDelay(3.0f));
        }

        private System.Collections.IEnumerator HideNotificationAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (notificationLabel != null)
            {
                notificationLabel.style.display = DisplayStyle.None;
            }
        }

        private void OnDisable()
        {
            if (eventSystem != null)
            {
                eventSystem.OnPlayerStatsChanged -= OnPlayerStatsChanged;
                eventSystem.OnInventoryChanged -= OnInventoryChanged;
            }
        }
    }
}