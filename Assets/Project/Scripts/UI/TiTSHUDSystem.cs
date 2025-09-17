using UnityEngine;
using UnityEngine.UIElements;
using MyGameNamespace.UI;

namespace MyGameNamespace.UI
{
    /// <summary>
    /// Complete TiTS-inspired HUD system integration for Unity 6.2
    /// Combines side bars, enhanced minimap, notifications, and quick actions
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class TiTSHUDSystem : MonoBehaviour
    {
        [Header("UI Document")]
        [SerializeField] private UIDocument uiDocument;

        [Header("HUD Components")]
        [SerializeField] private bool enableSideBars = true;
        [SerializeField] private bool enableEnhancedMinimap = true;
        [SerializeField] private bool enableNotifications = true;
        [SerializeField] private bool enableQuickActions = true;

        [Header("Side Bars Settings")]
        [SerializeField] private float healthValue = 100f;
        [SerializeField] private float maxHealthValue = 100f;
        [SerializeField] private float energyValue = 80f;
        [SerializeField] private float maxEnergyValue = 100f;
        [SerializeField] private float magicValue = 60f;
        [SerializeField] private float maxMagicValue = 100f;
        [SerializeField] private float friendshipValue = 75f;
        [SerializeField] private float maxFriendshipValue = 100f;
        [SerializeField] private float discordValue = 25f;
        [SerializeField] private float maxDiscordValue = 100f;

        // Public properties for external access
        public bool EnableSideBars { get => enableSideBars; set => enableSideBars = value; }
        public bool EnableEnhancedMinimap { get => enableEnhancedMinimap; set => enableEnhancedMinimap = value; }
        public bool EnableNotifications { get => enableNotifications; set => enableNotifications = value; }
        public bool EnableQuickActions { get => enableQuickActions; set => enableQuickActions = value; }

        public float HealthValue { get => healthValue; set { healthValue = value; UpdateHealthBar(); } }
        public float MaxHealthValue { get => maxHealthValue; set { maxHealthValue = value; UpdateHealthBar(); } }
        public float EnergyValue { get => energyValue; set { energyValue = value; UpdateEnergyBar(); } }
        public float MaxEnergyValue { get => maxEnergyValue; set { maxEnergyValue = value; UpdateEnergyBar(); } }
        public float MagicValue { get => magicValue; set { magicValue = value; UpdateMagicBar(); } }
        public float MaxMagicValue { get => maxMagicValue; set { maxMagicValue = value; UpdateMagicBar(); } }
        public float FriendshipValue { get => friendshipValue; set { friendshipValue = value; UpdateFriendshipBar(); } }
        public float MaxFriendshipValue { get => maxFriendshipValue; set { maxFriendshipValue = value; UpdateFriendshipBar(); } }
        public float DiscordValue { get => discordValue; set { discordValue = value; UpdateDiscordBar(); } }
        public float MaxDiscordValue { get => maxDiscordValue; set { maxDiscordValue = value; UpdateDiscordBar(); } }

        [Header("Minimap Settings")]
        [SerializeField] private Vector2 playerPosition = Vector2.zero;
        [SerializeField] private float playerRotation = 0f;

        // HUD Components
        private TiTSSideBars sideBars;
        private TiTSMinimap minimap;
        private TiTSNotifications notifications;
        private TiTSQuickActions quickActions;

        // Root elements
        private VisualElement root;
        private VisualElement gameWorldOverlay;

        private void Awake()
        {
            InitializeHUD();
        }

        private void Start()
        {
            // Initialize with default values
            UpdateAllStats();
            UpdatePlayerPosition();

            // Demo notifications (call directly instead of coroutine)
            ShowDemoNotifications();
        }

        private void InitializeHUD()
        {
            if (uiDocument == null)
            {
                Debug.LogError("TiTSHUDSystem: UIDocument is not assigned!");
                return;
            }

            root = uiDocument.rootVisualElement;

            // Create game world overlay (renders over the 3D world)
            gameWorldOverlay = new VisualElement();
            gameWorldOverlay.name = "game-world-overlay";
            gameWorldOverlay.style.position = Position.Absolute;
            gameWorldOverlay.style.top = 0;
            gameWorldOverlay.style.left = 0;
            gameWorldOverlay.style.right = 0;
            gameWorldOverlay.style.bottom = 0;
            gameWorldOverlay.pickingMode = PickingMode.Ignore;
            root.Add(gameWorldOverlay);

            // Initialize components
            if (enableSideBars)
            {
                InitializeSideBars();
            }

            if (enableEnhancedMinimap)
            {
                InitializeMinimap();
            }

            if (enableNotifications)
            {
                InitializeNotifications();
            }

            if (enableQuickActions)
            {
                InitializeQuickActions();
            }

            Debug.Log("TiTS-inspired HUD system initialized successfully!");
        }

        private void InitializeSideBars()
        {
            sideBars = new TiTSSideBars();
            gameWorldOverlay.Add(sideBars);
            Debug.Log("TiTS Side Bars initialized");
        }

        private void InitializeMinimap()
        {
            // Create minimap container
            var minimapContainer = new VisualElement();
            minimapContainer.name = "minimap-container";
            minimapContainer.style.position = Position.Absolute;
            minimapContainer.style.bottom = 20;
            minimapContainer.style.right = 20;
            gameWorldOverlay.Add(minimapContainer);

            minimap = new TiTSMinimap();
            minimapContainer.Add(minimap);
            Debug.Log("TiTS Enhanced Minimap initialized");
        }

        private void InitializeNotifications()
        {
            notifications = new TiTSNotifications();
            gameWorldOverlay.Add(notifications);
            Debug.Log("TiTS Notifications initialized");
        }

        private void InitializeQuickActions()
        {
            // Create quick actions container
            var quickActionsContainer = new VisualElement();
            quickActionsContainer.name = "quick-actions-container";
            quickActionsContainer.style.position = Position.Absolute;
            quickActionsContainer.style.bottom = 20;
            quickActionsContainer.style.left = 20;
            gameWorldOverlay.Add(quickActionsContainer);

            quickActions = new TiTSQuickActions();
            quickActionsContainer.Add(quickActions);
            Debug.Log("TiTS Quick Actions initialized");
        }

        /// <summary>
        /// Update all stat bars
        /// </summary>
        public void UpdateAllStats()
        {
            if (sideBars != null)
            {
                sideBars.SetAllStats(
                    healthValue, maxHealthValue,
                    energyValue, maxEnergyValue,
                    magicValue, maxMagicValue,
                    friendshipValue, maxFriendshipValue,
                    discordValue, maxDiscordValue
                );
            }
        }

        /// <summary>
        /// Update individual stat
        /// </summary>
        public void UpdateStat(string statName, float currentValue, float maxValue)
        {
            if (sideBars != null)
            {
                sideBars.UpdateStat(statName, currentValue, maxValue);
            }
        }

        /// <summary>
        /// Update player position on minimap
        /// </summary>
        public void UpdatePlayerPosition()
        {
            if (minimap != null)
            {
                minimap.UpdatePlayerPosition(playerPosition, playerRotation);
            }
        }

        /// <summary>
        /// Update player position with specific values
        /// </summary>
        public void UpdatePlayerPosition(Vector2 position, float rotation = 0f)
        {
            playerPosition = position;
            playerRotation = rotation;
            UpdatePlayerPosition();
        }

        /// <summary>
        /// Show a notification
        /// </summary>
        public void ShowNotification(string title, string message,
                                   TiTSNotifications.NotificationType type = TiTSNotifications.NotificationType.Info,
                                   float duration = 3.0f)
        {
            if (notifications != null)
            {
                notifications.ShowNotification(title, message, type, duration);
            }
        }

        /// <summary>
        /// Discover a location on the minimap
        /// </summary>
        public void DiscoverLocation(string locationName)
        {
            if (minimap != null)
            {
                minimap.DiscoverLocation(locationName);
            }
        }

        /// <summary>
        /// Visit a location on the minimap
        /// </summary>
        public void VisitLocation(string locationName)
        {
            if (minimap != null)
            {
                minimap.VisitLocation(locationName);
            }
        }

        /// <summary>
        /// Highlight a location on the minimap
        /// </summary>
        public void HighlightLocation(string locationName, bool highlight = true)
        {
            if (minimap != null)
            {
                minimap.HighlightLocation(locationName, highlight);
            }
        }

        /// <summary>
        /// Center minimap on player
        /// </summary>
        public void CenterMinimapOnPlayer()
        {
            if (minimap != null)
            {
                minimap.CenterOnPlayer();
            }
        }

        // Demo functionality
        private System.Collections.IEnumerator DemoNotifications()
        {
            yield return new WaitForSeconds(2f);

            if (notifications != null)
            {
                notifications.ShowSuccess("Welcome to Equestria!", "Your adventure begins now!");
                yield return new WaitForSeconds(1f);

                notifications.ShowAchievement("First Steps", "You've taken your first steps in Ponyville!");
                yield return new WaitForSeconds(1f);

                notifications.ShowQuestUpdate("Welcome Quest", "Visit the Town Square to meet your friends!");
                yield return new WaitForSeconds(1f);

                notifications.ShowItemReceived("Welcome Gift", 1);
            }
        }

        // Public accessors for components
        public TiTSSideBars SideBars => sideBars;
        public TiTSMinimap Minimap => minimap;
        public TiTSNotifications Notifications => notifications;
        public TiTSQuickActions QuickActions => quickActions;

        // Update methods for individual bars
        private void UpdateHealthBar()
        {
            if (sideBars != null)
            {
                sideBars.UpdateStat("Health", healthValue, maxHealthValue);
            }
        }

        private void UpdateEnergyBar()
        {
            if (sideBars != null)
            {
                sideBars.UpdateStat("Energy", energyValue, maxEnergyValue);
            }
        }

        private void UpdateMagicBar()
        {
            if (sideBars != null)
            {
                sideBars.UpdateStat("Magic", magicValue, maxMagicValue);
            }
        }

        private void UpdateFriendshipBar()
        {
            if (sideBars != null)
            {
                sideBars.UpdateStat("Friendship", friendshipValue, maxFriendshipValue);
            }
        }

        private void UpdateDiscordBar()
        {
            if (sideBars != null)
            {
                sideBars.UpdateStat("Discord", discordValue, maxDiscordValue);
            }
        }

        private void ShowDemoNotifications()
        {
            if (notifications != null)
            {
                notifications.ShowNotification("Welcome to MLP Game!", "Enjoy your adventure!", NotificationType.Info, 3f);
                // Add more demo notifications as needed
            }
        }

        private void OnValidate()
        {
            // Update stats when values change in inspector
            if (Application.isPlaying && sideBars != null)
            {
                UpdateAllStats();
            }
        }

        private void Update()
        {
            // Handle keyboard shortcuts
            if (Input.GetKeyDown(KeyCode.M))
            {
                CenterMinimapOnPlayer();
            }

            if (Input.GetKeyDown(KeyCode.N))
            {
                ShowNotification("Test Notification", "This is a test notification!",
                               TiTSNotifications.NotificationType.Info);
            }

            // Handle quick actions input
            if (quickActions != null)
            {
                quickActions.HandleInput();
            }
        }
    }
}