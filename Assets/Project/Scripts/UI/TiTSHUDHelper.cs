using UnityEngine;
using UnityEngine.UIElements;
using MyGameNamespace.UI;

namespace MyGameNamespace.UI
{
    /// <summary>
    /// Helper script to easily add TiTS HUD system to existing GameHUD GameObject
    /// Attach this to your GameHUD GameObject that already has UIController and UIDocument
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class TiTSHUDHelper : MonoBehaviour
    {
        [Header("TiTS HUD System")]
        [SerializeField] private bool addTiTSHUDSystem = true;
        [SerializeField] private bool removeAfterSetup = true;

        [Header("HUD Settings")]
        [SerializeField] private bool enableSideBars = true;
        [SerializeField] private bool enableEnhancedMinimap = true;
        [SerializeField] private bool enableNotifications = true;
        [SerializeField] private bool enableQuickActions = true;

        [Header("Stat Values")]
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

        private void Awake()
        {
            if (addTiTSHUDSystem)
            {
                SetupTiTSHUD();
            }

            if (removeAfterSetup)
            {
                Destroy(this);
            }
        }

        private void SetupTiTSHUD()
        {
            // Check if TiTSHUDSystem already exists
            var existingHUD = GetComponent<TiTSHUDSystem>();
            if (existingHUD != null)
            {
                Debug.Log("TiTSHUDSystem already exists on this GameObject!");
                return;
            }

            // Add TiTSHUDSystem component
            var titsHUD = gameObject.AddComponent<TiTSHUDSystem>();

            // Configure the component
            titsHUD.EnableSideBars = enableSideBars;
            titsHUD.EnableEnhancedMinimap = enableEnhancedMinimap;
            titsHUD.EnableNotifications = enableNotifications;
            titsHUD.EnableQuickActions = enableQuickActions;

            // Set stat values
            titsHUD.HealthValue = healthValue;
            titsHUD.MaxHealthValue = maxHealthValue;
            titsHUD.EnergyValue = energyValue;
            titsHUD.MaxEnergyValue = maxEnergyValue;
            titsHUD.MagicValue = magicValue;
            titsHUD.MaxMagicValue = maxMagicValue;
            titsHUD.FriendshipValue = friendshipValue;
            titsHUD.MaxFriendshipValue = maxFriendshipValue;
            titsHUD.DiscordValue = discordValue;
            titsHUD.MaxDiscordValue = maxDiscordValue;

            Debug.Log("TiTS HUD System successfully added to GameHUD GameObject!");
            Debug.Log("Available hotkeys: I (Inventory), C (Character), M (Map), Q (Quests), S (Spells), Tab (Items), T (Social), Esc (Settings)");
        }
    }
}