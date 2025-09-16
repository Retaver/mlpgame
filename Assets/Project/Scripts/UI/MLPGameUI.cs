using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using System.Collections.Generic;

namespace MyGameNamespace
{
    public class MLPGameUI : MonoBehaviour
    {
        // Singleton instance (optional)
        public static MLPGameUI Instance { get; private set; }

        [SerializeField] private UIDocument uiDocument;

    // Cached UI elements
    private VisualElement root;
    private Button characterBtn, menuBtn, optionsBtn, inventoryBtn;
    private Button statsTabBtn, skillsTabBtn, perksTabBtn, effectsTabBtn;
    private VisualElement statsPanel, skillsPanel, perksPanel, effectsPanel;
    private Image characterPortrait;

    // Map System
    [SerializeField] private MapSystem mapSystem;        private void Awake()
        {
            if (Instance != default && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Debug.Log("[MLPGameUI] Singleton instance set");

            if (uiDocument == default) uiDocument = GetComponent<UIDocument>();
            if (uiDocument == default)
            {
                Debug.LogError("[MLPGameUI] No UIDocument found on this GameObject!");
            }
            else
            {
                Debug.Log("[MLPGameUI] UIDocument found and ready");
            }
        }

        private void OnEnable()
        {
            root = uiDocument != default ? uiDocument.rootVisualElement : null;
            if (root == default)
            {
                Debug.LogWarning("[MLPGameUI] Missing UIDocument/root.");
                return;
            }

            // Initialize HUD with current player data
            UpdateHUD();

            // Locate buttons by various possible names or class for compatibility
            characterBtn = FindButtonRobust("CharacterButton", "Character", "character-button");

            // If not found in this UIDocument, search other documents in scene
            if (characterBtn == default)
            {
                var docs = UnityEngine.Object.FindObjectsByType<UIDocument>(FindObjectsSortMode.None);
                foreach (var d in docs)
                {
                    if (d == default || d.rootVisualElement == default || d == uiDocument) continue;
                    var b = FindButtonInDocument(d, "CharacterButton", "Character", "character-button");
                    if (b != default) { characterBtn = b; break; }
                }
            }

            menuBtn = FindButtonRobust("MenuButton", "Menu", "menu-button");

            if (menuBtn == default)
            {
                var docs = UnityEngine.Object.FindObjectsByType<UIDocument>(FindObjectsSortMode.None);
                foreach (var d in docs)
                {
                    if (d == default || d.rootVisualElement == default || d == uiDocument) continue;
                    var b = FindButtonInDocument(d, "MenuButton", "Menu", "menu-button");
                    if (b != default) { menuBtn = b; break; }
                }
            }

            inventoryBtn = FindButtonRobust("InventoryButton", "ItemsButton", "Items", "items-button", "inventory-button");

            if (inventoryBtn == default)
            {
                var docs = UnityEngine.Object.FindObjectsByType<UIDocument>(FindObjectsSortMode.None);
                foreach (var d in docs)
                {
                    if (d == default || d.rootVisualElement == default || d == uiDocument) continue;
                    var b = FindButtonInDocument(d, "InventoryButton", "ItemsButton", "Items", "items-button", "inventory-button");
                    if (b != default) { inventoryBtn = b; break; }
                }
            }

            optionsBtn = root.Q<Button>("OptionsButton")
                         ?? root.Q<Button>(className: "options-button");

            // Bind button click events (avoid double-binding by removing first)
            if (characterBtn != default)
            {
                characterBtn.clicked -= OnCharacter;
                characterBtn.clicked += OnCharacter;
                characterBtn.SetEnabled(true);
                characterBtn.pickingMode = PickingMode.Position;
                Debug.Log($"[MLPGameUI] Wired Character button: {characterBtn.name} (text: {characterBtn.text})");
            }
            else Debug.LogWarning("[MLPGameUI] CharacterButton not found - character sheet will not work!");

            if (menuBtn != default)
            {
                menuBtn.clicked -= OnMenu;
                menuBtn.clicked += OnMenu;
                menuBtn.SetEnabled(true);
                menuBtn.pickingMode = PickingMode.Position;
                Debug.Log($"[MLPGameUI] Wired Menu button: {menuBtn.name}");
            }
            else Debug.LogWarning("[MLPGameUI] MenuButton not found.");

            if (inventoryBtn != default)
            {
                inventoryBtn.clicked -= OnInventory;
                inventoryBtn.clicked += OnInventory;
                inventoryBtn.SetEnabled(true);
                inventoryBtn.pickingMode = PickingMode.Position;
                Debug.Log($"[MLPGameUI] Wired Inventory button: {inventoryBtn.name}");
            }
            else Debug.Log("[MLPGameUI] InventoryButton not found (optional).");

            if (optionsBtn != default)
            {
                optionsBtn.clicked -= OnOptions;
                optionsBtn.clicked += OnOptions;
                optionsBtn.SetEnabled(true);
                optionsBtn.pickingMode = PickingMode.Position;
                Debug.Log($"[MLPGameUI] Wired Options button: {optionsBtn.name}");
            }
            else Debug.Log("[MLPGameUI] OptionsButton not found (optional).");

            // Wire character sheet close button
            var closeBtn = root.Q<Button>("close-button");
            if (closeBtn != default)
            {
                closeBtn.clicked -= OnCharacterClose;
                closeBtn.clicked += OnCharacterClose;
                closeBtn.SetEnabled(true);
                closeBtn.pickingMode = PickingMode.Position;
                Debug.Log("[MLPGameUI] Wired Character Sheet close button.");
            }
            else Debug.Log("[MLPGameUI] Character Sheet close button not found.");

            // Find and wire character sheet tabs
            statsTabBtn = root.Q<Button>("stats-tab");
            skillsTabBtn = root.Q<Button>("skills-tab");
            perksTabBtn = root.Q<Button>("perks-tab");
            effectsTabBtn = root.Q<Button>("effects-tab");

            statsPanel = root.Q<VisualElement>("stats-panel");
            skillsPanel = root.Q<VisualElement>("skills-panel");
            perksPanel = root.Q<VisualElement>("perks-panel");
            effectsPanel = root.Q<VisualElement>("effects-panel");

            // Find character portrait
            characterPortrait = root.Q<Image>("character-portrait");

            // Initialize Map System
            if (mapSystem == null)
            {
                mapSystem = gameObject.AddComponent<MapSystem>();
                Debug.Log("[MLPGameUI] MapSystem component added");
            }

            Debug.Log($"[MLPGameUI] Panel initialization - statsPanel: {(statsPanel != null ? "found" : "null")}, skillsPanel: {(skillsPanel != null ? "found" : "null")}, perksPanel: {(perksPanel != null ? "found" : "null")}, effectsPanel: {(effectsPanel != null ? "found" : "null")}");

            // Wire tab buttons
            if (statsTabBtn != default)
            {
                statsTabBtn.clicked -= () => SwitchToTab("stats");
                statsTabBtn.clicked += () => SwitchToTab("stats");
                Debug.Log("[MLPGameUI] Wired stats tab button");
            }

            if (skillsTabBtn != default)
            {
                skillsTabBtn.clicked -= () => SwitchToTab("skills");
                skillsTabBtn.clicked += () => SwitchToTab("skills");
                Debug.Log("[MLPGameUI] Wired skills tab button");
            }

            if (perksTabBtn != default)
            {
                perksTabBtn.clicked -= () => SwitchToTab("perks");
                perksTabBtn.clicked += () => SwitchToTab("perks");
                Debug.Log("[MLPGameUI] Wired perks tab button");
            }

            if (effectsTabBtn != default)
            {
                effectsTabBtn.clicked -= () => SwitchToTab("effects");
                effectsTabBtn.clicked += () => SwitchToTab("effects");
                Debug.Log("[MLPGameUI] Wired effects tab button");
            }

            // Initialize tabs (show stats by default)
            InitializeTabs();
        }

        private Button FindButtonRobust(params string[] namesAndClasses)
        {
            if (root == default) return null;

            foreach (var name in namesAndClasses)
            {
                var btn = root.Q<Button>(name);
                if (btn != default) return btn;

                // Try as class name
                btn = root.Q<Button>(className: name);
                if (btn != default) return btn;
            }
            return null;
        }

        private Button FindButtonInDocument(UIDocument doc, params string[] namesAndClasses)
        {
            if (doc == default || doc.rootVisualElement == default) return null;

            foreach (var name in namesAndClasses)
            {
                var btn = doc.rootVisualElement.Q<Button>(name);
                if (btn != default) return btn;

                // Try as class name
                btn = doc.rootVisualElement.Q<Button>(className: name);
                if (btn != default) return btn;
            }
            return null;
        }

        private void OnDisable()
        {
            if (characterBtn != default) characterBtn.clicked -= OnCharacter;
            if (menuBtn != default) menuBtn.clicked -= OnMenu;
            if (inventoryBtn != default) inventoryBtn.clicked -= OnInventory;
            if (optionsBtn != default) optionsBtn.clicked -= OnOptions;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // === UI Button Handlers ===

        private void OnCharacter()
        {
            Debug.Log("[MLPGameUI] Character button clicked - showing character sheet");

            // Find the character sheet modal in our own UIDocument
            if (root == default)
            {
                Debug.LogError("[MLPGameUI] No root visual element found!");
                return;
            }

            var sheetModal = root.Q<VisualElement>("character-sheet-modal");
            if (sheetModal == default)
            {
                Debug.LogError("[MLPGameUI] Character sheet modal not found in MLPGameUI!");
                return;
            }

            // Show the character sheet
            sheetModal.style.display = DisplayStyle.Flex;
            sheetModal.style.visibility = Visibility.Visible;
            sheetModal.style.opacity = 1f;

            // Bring to front
            root.BringToFront();
            if (uiDocument != default && uiDocument.panelSettings != default)
            {
                uiDocument.panelSettings.sortingOrder = 200;
            }

            // Populate character sheet with player data
            PopulateCharacterSheet();

            // Initialize tabs when showing character sheet
            InitializeTabs();

            Debug.Log("[MLPGameUI] Character sheet shown successfully");
        }

        private void OnCharacterClose()
        {
            Debug.Log("[MLPGameUI] Character sheet close button clicked");

            // Find the character sheet modal
            if (root == default)
            {
                Debug.LogError("[MLPGameUI] No root visual element found!");
                return;
            }

            var sheetModal = root.Q<VisualElement>("character-sheet-modal");
            if (sheetModal == default)
            {
                Debug.LogError("[MLPGameUI] Character sheet modal not found!");
                return;
            }

            // Hide the character sheet
            sheetModal.style.display = DisplayStyle.None;
            sheetModal.style.visibility = Visibility.Hidden;
            sheetModal.style.opacity = 0f;

            // Reset sorting order
            if (uiDocument != default && uiDocument.panelSettings != default)
            {
                uiDocument.panelSettings.sortingOrder = 0;
            }

            Debug.Log("[MLPGameUI] Character sheet hidden successfully");
        }

        public void PopulateCharacterSheet()
        {
            Debug.Log("[MLPGameUI] Populating character sheet with player data");

            // Get the current player character
            var player = GetCurrentPlayerCharacter();
            if (player == null)
            {
                Debug.LogWarning("[MLPGameUI] No player character found to populate character sheet");
                return;
            }

            Debug.Log($"[MLPGameUI] Found player: {player.name}, Level {player.level}, Skills: {player.skillPoints}, Perks: {player.perks?.Count ?? 0}");

            // Update character name and level in header
            var nameLabel = root.Q<Label>("CharacterName");
            var levelLabel = root.Q<Label>("LevelLabel");
            var raceLabel = root.Q<Label>("RaceLabel");
            var bitsLabel = root.Q<Label>("BitsValue");

            if (nameLabel != default)
            {
                nameLabel.text = player.name ?? "Unknown";
                Debug.Log($"[MLPGameUI] Updated character name to: {nameLabel.text}");
            }
            else
            {
                Debug.LogWarning("[MLPGameUI] CharacterName label not found");
            }

            if (levelLabel != default)
            {
                levelLabel.text = $"Level {player.level}";
                Debug.Log($"[MLPGameUI] Updated character level to: {levelLabel.text}");
            }
            else
            {
                Debug.LogWarning("[MLPGameUI] LevelLabel not found");
            }

            if (raceLabel != default)
            {
                raceLabel.text = $"Race: {player.race}";
                Debug.Log($"[MLPGameUI] Updated character race to: {raceLabel.text}");
            }
            else
            {
                Debug.LogWarning("[MLPGameUI] RaceLabel not found");
            }

            if (bitsLabel != default)
            {
                bitsLabel.text = $"{player.bits} Bits";
                Debug.Log($"[MLPGameUI] Updated character bits to: {bitsLabel.text}");
            }
            else
            {
                Debug.LogWarning("[MLPGameUI] BitsValue label not found");
            }

            // Update character portrait
            UpdateCharacterPortrait(player);

            // Populate skills panel
            PopulateSkillsPanel(player);

            // Populate perks panel
            PopulatePerksPanel(player);
        }

        public void UpdateHUD()
        {
            Debug.Log("[MLPGameUI] Updating HUD with current player data");

            // Get the current player character
            var player = GetCurrentPlayerCharacter();
            if (player == null)
            {
                Debug.LogWarning("[MLPGameUI] No player character found to update HUD");
                return;
            }

            Debug.Log($"[MLPGameUI] Updating HUD for player: {player.name}, Level {player.level}, {player.bits} Bits");

            // Update HUD elements
            var nameLabel = root.Q<Label>("CharacterName");
            var levelLabel = root.Q<Label>("LevelLabel");
            var raceLabel = root.Q<Label>("RaceLabel");
            var bitsLabel = root.Q<Label>("BitsValue");

            if (nameLabel != default)
            {
                nameLabel.text = player.name ?? "Unknown";
                Debug.Log($"[MLPGameUI] Updated HUD character name to: {nameLabel.text}");
            }

            if (levelLabel != default)
            {
                levelLabel.text = $"Level {player.level}";
                Debug.Log($"[MLPGameUI] Updated HUD character level to: {levelLabel.text}");
            }

            if (raceLabel != default)
            {
                raceLabel.text = $"Race: {player.race}";
                Debug.Log($"[MLPGameUI] Updated HUD character race to: {raceLabel.text}");
            }

            if (bitsLabel != default)
            {
                bitsLabel.text = $"{player.bits} Bits";
                Debug.Log($"[MLPGameUI] Updated HUD character bits to: {bitsLabel.text}");
            }

            // Update character portrait
            UpdateCharacterPortrait(player);
        }

        private void PopulateSkillsPanel(PlayerCharacter player)
        {
            if (skillsPanel == default)
            {
                Debug.LogWarning("[MLPGameUI] skillsPanel is null");
                return;
            }

            Debug.Log("[MLPGameUI] Populating skills panel");

            // Clear existing skill entries
            var skillsGrid = skillsPanel.Q<VisualElement>("skills-grid");
            if (skillsGrid != default)
            {
                Debug.Log($"[MLPGameUI] Found skills-grid, clearing {skillsGrid.childCount} existing elements");

                // Remove specific sample elements by name if they exist
                var sampleSkill = skillsGrid.Q<VisualElement>("sample-skill");
                if (sampleSkill != default)
                {
                    Debug.Log("[MLPGameUI] Removing sample-skill element");
                    skillsGrid.Remove(sampleSkill);
                }

                skillsGrid.Clear();
            }
            else
            {
                Debug.LogWarning("[MLPGameUI] skills-grid not found in skillsPanel");
            }

            // Update skill points label
            var skillPointsLabel = skillsPanel.Q<Label>("skill-points-label");
            if (skillPointsLabel != default)
            {
                skillPointsLabel.text = $"Skill Points: {player.skillPoints}";
            }

            // Get skill tree to access all skills
            var skillTree = CompatUtils.FindFirstObjectByTypeCompat<SkillTree>();
            if (skillTree == default)
            {
                Debug.LogWarning("[MLPGameUI] No SkillTree found, showing basic categories");
                // Fallback to basic categories if no skill tree
                var basicSkills = new[]
                {
                    ("Combat", "Basic combat training"),
                    ("Magic", "Magical ability"),
                    ("Social", "Social interaction"),
                    ("Nature", "Nature knowledge"),
                    ("Crafting", "Item creation"),
                    ("Leadership", "Group command")
                };

                if (skillsGrid != default)
                {
                    foreach (var (skillName, description) in basicSkills)
                    {
                        var skillCard = new VisualElement();
                        skillCard.AddToClassList("skill-card");

                        var skillNameLabel = new Label(skillName);
                        skillNameLabel.AddToClassList("skill-name");

                        var skillValueLabel = new Label("0"); // No skills learned
                        skillValueLabel.AddToClassList("skill-value");

                        skillCard.Add(skillNameLabel);
                        skillCard.Add(skillValueLabel);

                        skillsGrid.Add(skillCard);
                        Debug.Log($"[MLPGameUI] Added fallback skill: {skillName}");
                    }
                }
                return;
            }

            // Get all skills and filter to learned ones
            var allSkills = skillTree.GetAllSkills();
            var learnedSkills = allSkills.Where(s => s.IsUnlocked).ToList();

            Debug.Log($"[MLPGameUI] Found {allSkills.Count} total skills, {learnedSkills.Count} learned skills");

            if (learnedSkills.Count == 0)
            {
                // Show message if no skills learned
                var noSkillsLabel = new Label("No skills learned yet. Visit the skill tree to learn skills!");
                noSkillsLabel.AddToClassList("no-skills-message");
                if (skillsGrid != default)
                {
                    skillsGrid.Add(noSkillsLabel);
                }
                Debug.Log("[MLPGameUI] No learned skills found, showing message");
            }
            else
            {
                // Group skills by category
                var skillsByCategory = learnedSkills.GroupBy(s => s.category);

                if (skillsGrid != default)
                {
                    foreach (var categoryGroup in skillsByCategory.OrderBy(g => g.Key.ToString()))
                    {
                        Debug.Log($"[MLPGameUI] Adding category: {categoryGroup.Key} with {categoryGroup.Count()} skills");

                        // Create category header
                        var categoryHeader = new Label(categoryGroup.Key.ToString());
                        categoryHeader.AddToClassList("skill-category-header");
                        skillsGrid.Add(categoryHeader);

                        // Add skills in this category
                        foreach (var skill in categoryGroup.OrderBy(s => s.name))
                        {
                            Debug.Log($"[MLPGameUI] Adding skill: {skill.name} (Rank {skill.currentRank}/{skill.maxRank})");

                            var skillCard = new VisualElement();
                            skillCard.AddToClassList("skill-card");

                            var skillNameLabel = new Label($"{skill.name} (Rank {skill.currentRank}/{skill.maxRank})");
                            skillNameLabel.AddToClassList("skill-name");

                            var skillDescLabel = new Label(skill.description);
                            skillDescLabel.AddToClassList("skill-description");

                            skillCard.Add(skillNameLabel);
                            skillCard.Add(skillDescLabel);
                            skillsGrid.Add(skillCard);
                        }
                    }
                }
            }
        }

        private void PopulatePerksPanel(PlayerCharacter player)
        {
            if (perksPanel == default)
            {
                Debug.LogWarning("[MLPGameUI] perksPanel is null");
                return;
            }

            Debug.Log("[MLPGameUI] Populating perks panel");

            // Clear existing perk entries
            var perksGrid = perksPanel.Q<VisualElement>("perks-grid");
            if (perksGrid != default)
            {
                Debug.Log($"[MLPGameUI] Found perks-grid, clearing {perksGrid.childCount} existing elements");

                // Remove specific sample elements by name if they exist
                var samplePerk = perksGrid.Q<VisualElement>("sample-perk");
                if (samplePerk != default)
                {
                    Debug.Log("[MLPGameUI] Removing sample-perk element");
                    perksGrid.Remove(samplePerk);
                }

                perksGrid.Clear();
            }
            else
            {
                Debug.LogWarning("[MLPGameUI] perks-grid not found in perksPanel");
            }

            // Populate with player's actual perks
            if (player.perks != null && player.perks.Count > 0)
            {
                Debug.Log($"[MLPGameUI] Found {player.perks.Count} perks to populate");

                foreach (var perkType in player.perks)
                {
                    Debug.Log($"[MLPGameUI] Adding perk: {perkType} -> {FormatPerkName(perkType)}");

                    var perkCard = new VisualElement();
                    perkCard.AddToClassList("perk-card");

                    var perkNameLabel = new Label(FormatPerkName(perkType));
                    perkNameLabel.AddToClassList("perk-name");

                    var perkDescriptionLabel = new Label(GetPerkDescription(perkType));
                    perkDescriptionLabel.AddToClassList("perk-description");

                    perkCard.Add(perkNameLabel);
                    perkCard.Add(perkDescriptionLabel);

                    if (perksGrid != default)
                    {
                        perksGrid.Add(perkCard);
                    }
                }
            }
            else
            {
                Debug.Log("[MLPGameUI] No perks found to populate");

                // Show message if no perks
                var emptyLabel = new Label("No perks learned yet.");
                emptyLabel.AddToClassList("perks-empty");
                if (perksGrid != default)
                {
                    perksGrid.Add(emptyLabel);
                }
            }
        }

        private PlayerCharacter GetCurrentPlayerCharacter()
        {
            Debug.Log("[MLPGameUI] Getting current player character");

            // Try to get player from various sources
            var gameManager = UnityEngine.Object.FindFirstObjectByType<GameManager>();
            if (gameManager != null)
            {
                var player = gameManager.GetPlayer();
                if (player != null)
                {
                    Debug.Log($"[MLPGameUI] Found player via GameManager: {player.name}");
                    return player;
                }
                else
                {
                    Debug.Log("[MLPGameUI] GameManager found but GetPlayer() returned null");
                }
            }
            else
            {
                Debug.Log("[MLPGameUI] GameManager not found");
            }

            // Try UIController
            var uiController = UnityEngine.Object.FindFirstObjectByType<UIController>();
            if (uiController != null && uiController.GetType().GetField("currentPlayer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance) != null)
            {
                var player = uiController.GetType().GetField("currentPlayer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(uiController) as PlayerCharacter;
                if (player != null)
                {
                    Debug.Log($"[MLPGameUI] Found player via UIController reflection: {player.name}");
                    return player;
                }
                else
                {
                    Debug.Log("[MLPGameUI] UIController found but currentPlayer field is null");
                }
            }
            else
            {
                Debug.Log("[MLPGameUI] UIController not found or currentPlayer field not accessible");
            }

            Debug.Log("[MLPGameUI] No player character found");
            return null;
        }

        private string FormatPerkName(PerkType perkType)
        {
            // Convert enum names to readable format
            string name = perkType.ToString();

            // Handle special cases
            var specialNames = new Dictionary<string, string>
            {
                { "EarthPonyStrength", "Earth Pony Strength" },
                { "UnicornMagic", "Unicorn Magic" },
                { "PegasusWings", "Pegasus Wings" },
                { "BatPonyEcholocation", "Bat Pony Echolocation" },
                { "GriffonTalons", "Griffon Talons" },
                { "DragonBreath", "Dragon Breath" },
                { "HumanVersatility", "Human Versatility" },
                { "MagicalAptitude", "Magical Aptitude" },
                { "TeleportMaster", "Teleport Master" },
                { "MagicShield", "Magic Shield" },
                { "SpellCaster", "Spell Caster" },
                { "WeatherControl", "Weather Control" },
                { "SonicRainboom", "Sonic Rainboom" },
                { "CloudWalking", "Cloud Walking" },
                { "AerialAce", "Aerial Ace" },
                { "StormMaster", "Storm Master" },
                { "EarthConnection", "Earth Connection" },
                { "SuperStrength", "Super Strength" },
                { "PlantWhisperer", "Plant Whisperer" },
                { "IronHooves", "Iron Hooves" },
                { "NaturalHealer", "Natural Healer" },
                { "NightVision", "Night Vision" },
                { "SilentFlight", "Silent Flight" },
                { "EcholocationMaster", "Echolocation Master" },
                { "ShadowStep", "Shadow Step" },
                { "TalonStrike", "Talon Strike" },
                { "KeenEye", "Keen Eye" },
                { "PredatorInstinct", "Predator Instinct" },
                { "AerialCombat", "Aerial Combat" },
                { "DragonScales", "Dragon Scales" },
                { "FireBreath", "Fire Breath" },
                { "AncientWisdom", "Ancient Wisdom" },
                { "DragonFear", "Dragon Fear" },
                { "StrongBack", "Strong Back" },
                { "FastLearner", "Fast Learner" }
            };

            if (specialNames.ContainsKey(name))
            {
                return specialNames[name];
            }

            // Default formatting: insert spaces before capital letters
            return System.Text.RegularExpressions.Regex.Replace(name, "([a-z])([A-Z])", "$1 $2");
        }

        private string GetPerkDescription(PerkType perkType)
        {
            // Provide descriptions for perks
            var descriptions = new Dictionary<PerkType, string>
            {
                // Racial perks
                { PerkType.EarthPonyStrength, "Increases physical strength and endurance." },
                { PerkType.UnicornMagic, "Grants basic magical abilities." },
                { PerkType.PegasusWings, "Allows flight and aerial maneuvers." },
                { PerkType.BatPonyEcholocation, "Enhanced senses in darkness." },
                { PerkType.GriffonTalons, "Powerful claw attacks and grip." },
                { PerkType.DragonBreath, "Breathe fire and withstand heat." },
                { PerkType.HumanVersatility, "Adaptable to various situations." },

                // Universal perks
                { PerkType.Tough, "Increased resistance to damage." },
                { PerkType.StrongBack, "Can carry heavier loads." },
                { PerkType.Charming, "Better social interactions." },
                { PerkType.Hardy, "Resists environmental hazards." },
                { PerkType.FastLearner, "Learns skills more quickly." },

                // Unicorn perks
                { PerkType.MagicalAptitude, "Enhanced magical power." },
                { PerkType.TeleportMaster, "Master of teleportation spells." },
                { PerkType.MagicShield, "Create protective magical barriers." },
                { PerkType.Levitation, "Lift and move objects with magic." },
                { PerkType.SpellCaster, "Cast powerful spells." },

                // Pegasus perks
                { PerkType.WeatherControl, "Control weather patterns." },
                { PerkType.SonicRainboom, "Break the sound barrier in flight." },
                { PerkType.CloudWalking, "Walk on clouds." },
                { PerkType.AerialAce, "Expert aerial combatant." },
                { PerkType.StormMaster, "Command lightning and storms." },

                // Earth Pony perks
                { PerkType.EarthConnection, "Deep connection to the earth." },
                { PerkType.SuperStrength, "Exceptional physical strength." },
                { PerkType.PlantWhisperer, "Communicate with plants." },
                { PerkType.IronHooves, "Unbreakable hooves." },
                { PerkType.NaturalHealer, "Accelerate natural healing." },

                // Bat Pony perks
                { PerkType.NightVision, "See clearly in darkness." },
                { PerkType.SilentFlight, "Fly without making noise." },
                { PerkType.EcholocationMaster, "Perfect echolocation abilities." },
                { PerkType.ShadowStep, "Move between shadows instantly." },

                // Griffon perks
                { PerkType.TalonStrike, "Devastating talon attacks." },
                { PerkType.KeenEye, "Exceptional eyesight." },
                { PerkType.PredatorInstinct, "Heightened hunting abilities." },
                { PerkType.AerialCombat, "Master of aerial combat." },

                // Dragon perks
                { PerkType.DragonScales, "Tough, fire-resistant scales." },
                { PerkType.FireBreath, "Breathe devastating flames." },
                { PerkType.AncientWisdom, "Access to ancient knowledge." },
                { PerkType.DragonFear, "Instill fear in enemies." },

                // Human perks
                { PerkType.Adaptability, "Quickly adapt to new situations." },
                { PerkType.Innovation, "Create new tools and solutions." },
                { PerkType.Diplomacy, "Excel at negotiations." },
                { PerkType.QuickLearner, "Master new skills rapidly." }
            };

            if (descriptions.ContainsKey(perkType))
            {
                return descriptions[perkType];
            }

            return "A special ability or trait.";
        }

        private void InitializeTabs()
        {
            Debug.Log("[MLPGameUI] Initializing character sheet tabs");

            // Hide all panels except stats
            if (statsPanel != default) statsPanel.style.display = DisplayStyle.Flex;
            if (skillsPanel != default) skillsPanel.style.display = DisplayStyle.None;
            if (perksPanel != default) perksPanel.style.display = DisplayStyle.None;
            if (effectsPanel != default) effectsPanel.style.display = DisplayStyle.None;

            // Set active tab styling
            UpdateTabStyling("stats");
        }

        private void SwitchToTab(string tabName)
        {
            Debug.Log($"[MLPGameUI] Switching to tab: {tabName}");

            // Hide all panels
            if (statsPanel != default) statsPanel.style.display = DisplayStyle.None;
            if (skillsPanel != default) skillsPanel.style.display = DisplayStyle.None;
            if (perksPanel != default) perksPanel.style.display = DisplayStyle.None;
            if (effectsPanel != default) effectsPanel.style.display = DisplayStyle.None;

            // Show selected panel
            switch (tabName)
            {
                case "stats":
                    if (statsPanel != default) statsPanel.style.display = DisplayStyle.Flex;
                    break;
                case "skills":
                    if (skillsPanel != default)
                    {
                        skillsPanel.style.display = DisplayStyle.Flex;
                        // Refresh skills data when tab is shown
                        var player = GetCurrentPlayerCharacter();
                        if (player != null)
                        {
                            PopulateSkillsPanel(player);
                        }
                    }
                    break;
                case "perks":
                    if (perksPanel != default)
                    {
                        perksPanel.style.display = DisplayStyle.Flex;
                        // Refresh perks data when tab is shown
                        var player = GetCurrentPlayerCharacter();
                        if (player != null)
                        {
                            PopulatePerksPanel(player);
                        }
                    }
                    break;
                case "effects":
                    if (effectsPanel != default) effectsPanel.style.display = DisplayStyle.Flex;
                    break;
            }

            // Update tab button styling
            UpdateTabStyling(tabName);
        }

        private void UpdateTabStyling(string activeTab)
        {
            // Remove active class from all tabs
            if (statsTabBtn != default) statsTabBtn.RemoveFromClassList("active");
            if (skillsTabBtn != default) skillsTabBtn.RemoveFromClassList("active");
            if (perksTabBtn != default) perksTabBtn.RemoveFromClassList("active");
            if (effectsTabBtn != default) effectsTabBtn.RemoveFromClassList("active");

            // Add active class to selected tab
            switch (activeTab)
            {
                case "stats":
                    if (statsTabBtn != default) statsTabBtn.AddToClassList("active");
                    break;
                case "skills":
                    if (skillsTabBtn != default) skillsTabBtn.AddToClassList("active");
                    break;
                case "perks":
                    if (perksTabBtn != default) perksTabBtn.AddToClassList("active");
                    break;
                case "effects":
                    if (effectsTabBtn != default) effectsTabBtn.AddToClassList("active");
                    break;
            }
        }

        private void OnMenu()
        {
            var pause = FindFirstObjectByType<PauseMenuController>();
            if (pause != default)
            {
                pause.ShowPauseMenu();
                return;
            }

            // Fallback: try to show any pause menu panel in active UI Documents
            var docs = Object.FindObjectsByType<UIDocument>(FindObjectsSortMode.None);
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
                    if (d.panelSettings != default && d.panelSettings.sortingOrder < 200)
                        d.panelSettings.sortingOrder = 200;
                    return;
                }
            }

            Debug.LogWarning("[MLPGameUI] Pause menu not found in scene.");
        }

        private void OnInventory()
        {
            // First, try using InventoryScreenController to show the inventory
            var invController = FindFirstObjectByType<InventoryScreenController>();
            if (invController != default)
            {
                invController.Show();
                return;
            }

            // Fallback: directly make any known inventory container visible
            var docs = Object.FindObjectsByType<UIDocument>(FindObjectsSortMode.None);
            foreach (var d in docs)
            {
                if (d == default || d.rootVisualElement == default) continue;
                var inventory = d.rootVisualElement.Q<VisualElement>("inventory-container")
                              ?? d.rootVisualElement.Q<VisualElement>("InventoryRoot")
                              ?? d.rootVisualElement.Q<VisualElement>("inventory-root")
                              ?? d.rootVisualElement.Q<VisualElement>(className: "inventory")
                              ?? d.rootVisualElement.Q<VisualElement>(className: "inventory-root");
                if (inventory != default)
                {
                    inventory.style.display = DisplayStyle.Flex;
                    inventory.style.visibility = Visibility.Visible;
                    inventory.style.opacity = 1f;
                    d.rootVisualElement.BringToFront();
                    if (d.panelSettings != default && d.panelSettings.sortingOrder < 200)
                        d.panelSettings.sortingOrder = 200;
                    return;
                }
            }

            Debug.LogWarning("[MLPGameUI] Inventory not found in any UIDocument.");
        }

        private void OnOptions()
        {
            var pause = FindFirstObjectByType<PauseMenuController>();
            if (pause != default)
            {
                pause.ShowOptions();
                return;
            }
            Debug.LogWarning("[MLPGameUI] PauseMenuController not found in scene.");
        }

        /// <summary>
        /// Ensures the HUD is shown after character creation.
        /// </summary>
        public void ShowAfterCharacterCreation()
        {
            if (uiDocument == default) uiDocument = GetComponent<UIDocument>();
            if (root == default && uiDocument != default)
                root = uiDocument.rootVisualElement;
            if (root != default) root.style.display = DisplayStyle.Flex;
            gameObject.SetActive(true);
        }

        // Public helper to open the character sheet (same as pressing the button)
        public void OpenCharacterSheet() => OnCharacter();

        private void UpdateCharacterPortrait(PlayerCharacter player)
        {
            if (player == null || characterPortrait == null)
            {
                Debug.LogWarning("[MLPGameUI] Cannot update portrait - player or portrait element is null");
                return;
            }

            Debug.Log($"[MLPGameUI] Updating HUD portrait for {player.name} ({player.race}, {player.gender})");

            // Try to load a Sprite via PortraitLoader HUD portraits (returns Sprite -> use its texture)
            if (MyGameNamespace.UI.PortraitLoader.TryLoadHUDPortrait(player.race.ToString(), player.gender ?? string.Empty, out var sprite))
            {
                characterPortrait.sprite = sprite;
                characterPortrait.scaleMode = ScaleMode.ScaleToFit;
                Debug.Log($"[MLPGameUI] Loaded HUD portrait sprite for {player.race}");
            }
            else
            {
                // Fallback: try to load texture directly from HUD portraits
                var texture = Resources.Load<Texture2D>($"Portraits/HUD_Portraits/{player.race}/{player.gender ?? "Male"}/portrait");
                if (texture != null)
                {
                    characterPortrait.image = texture;
                    characterPortrait.scaleMode = ScaleMode.ScaleToFit;
                    Debug.Log($"[MLPGameUI] Loaded HUD portrait texture for {player.race}");
                }
                else
                {
                    Debug.LogWarning($"[MLPGameUI] No HUD portrait found for {player.race}/{player.gender ?? "Male"}, falling back to regular portrait");
                    // Final fallback to regular portrait system
                    if (MyGameNamespace.UI.PortraitLoader.TryLoadPortrait(player.race.ToString(), player.gender ?? string.Empty, out var fallbackSprite))
                    {
                        characterPortrait.sprite = fallbackSprite;
                        characterPortrait.scaleMode = ScaleMode.ScaleToFit;
                        Debug.Log($"[MLPGameUI] Loaded fallback portrait sprite for {player.race}");
                    }
                    else
                    {
                        Debug.LogWarning($"[MLPGameUI] No portrait found for {player.race}/{player.gender ?? "Male"}");
                        // Could set a default silhouette here
                    }
                }
            }
        }
    }
}
