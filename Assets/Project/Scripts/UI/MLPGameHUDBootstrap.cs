using UnityEngine;
using UnityEngine.UIElements;
using MyGameNamespace.UI;

namespace MyGameNamespace
{
    /// <summary>
    /// Bootstrap script for the MLP Game HUD
    /// Add this to your Game scene to enable the HUD
    /// </summary>
    public class MLPGameHUDBootstrap : MonoBehaviour
    {
        [Header("HUD Settings")]
        [SerializeField] private bool enableHUD = true;
        [SerializeField] private bool showMinimap = true;
        [SerializeField] private bool showLocationInfo = true;
        [SerializeField] private bool showSystemButtons = true;

        private MLPGameHUD hud;
        private UIDocument uiDocument;

        private void Awake()
        {
            if (!enableHUD) return;

            // Create UI Document if it doesn't exist
            uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null)
            {
                uiDocument = gameObject.AddComponent<UIDocument>();
                uiDocument.panelSettings = Resources.Load<PanelSettings>("DefaultPanelSettings");
            }

            // Load the UXML
            var visualTree = Resources.Load<VisualTreeAsset>("MLPGameHUD");
            if (visualTree != null)
            {
                uiDocument.visualTreeAsset = visualTree;
            }
            else
            {
                Debug.LogWarning("[MLPGameHUDBootstrap] MLPGameHUD.uxml not found in Resources. Make sure it's placed in Assets/Resources/");
            }

            // Load the USS
            var styleSheet = Resources.Load<StyleSheet>("MLPGameHUD");
            if (styleSheet != null)
            {
                uiDocument.rootVisualElement.styleSheets.Add(styleSheet);
            }
            else
            {
                Debug.LogWarning("[MLPGameHUDBootstrap] MLPGameHUD.uss not found in Resources. Make sure it's placed in Assets/Resources/");
            }
        }

        private void Start()
        {
            if (!enableHUD) return;

            // Add the HUD controller
            hud = gameObject.AddComponent<MLPGameHUD>();

            // Configure HUD settings
            var hudType = typeof(MLPGameHUD);
            var showMinimapField = hudType.GetField("showMinimap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var showLocationInfoField = hudType.GetField("showLocationInfo", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var showSystemButtonsField = hudType.GetField("showSystemButtons", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (showMinimapField != null) showMinimapField.SetValue(hud, showMinimap);
            if (showLocationInfoField != null) showLocationInfoField.SetValue(hud, showLocationInfo);
            if (showSystemButtonsField != null) showSystemButtonsField.SetValue(hud, showSystemButtons);

            Debug.Log("[MLPGameHUDBootstrap] MLP Game HUD initialized");
        }

        private void OnDisable()
        {
            if (hud != null)
            {
                Destroy(hud);
            }
        }
    }
}