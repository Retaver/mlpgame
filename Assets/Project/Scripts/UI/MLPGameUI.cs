using UnityEngine;
using UnityEngine.UIElements;

namespace MyGameNamespace
{
    public class MLPGameUI : MonoBehaviour
    {
        // Singleton instance (optional)
        public static MLPGameUI Instance { get; private set; }

        [SerializeField] private UIDocument uiDocument;

        // Cached UI elements
        private VisualElement root;
        private Button characterBtn, menuBtn, optionsBtn;

        private void Awake()
        {
            if (Instance != default && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (uiDocument == default) uiDocument = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            root = uiDocument != default ? uiDocument.rootVisualElement : null;
            if (root == default)
            {
                Debug.LogWarning("[MLPGameUI] Missing UIDocument/root.");
                return;
            }

            // Locate buttons by various possible names or class for compatibility
            characterBtn = root.Q<Button>("CharacterButton")
                          ?? root.Q<Button>(className: "character-button")
                          ?? root.Q<Button>("Character");  // fallback

            // If not found in this UIDocument, search other documents in scene
            if (characterBtn == default)
            {
                var docs = UnityEngine.Object.FindObjectsOfType<UIDocument>(FindObjectsSortMode.None);
                foreach (var d in docs)
                {
                    if (d == default || d.rootVisualElement == default) continue;
                    var b = d.rootVisualElement.Q<Button>("CharacterButton") ?? d.rootVisualElement.Q<Button>(className: "character-button") ?? d.rootVisualElement.Q<Button>("Character");
                    if (b != default) { characterBtn = b; break; }
                }
            }

            menuBtn = root.Q<Button>("MenuButton")
                      ?? root.Q<Button>(className: "menu-button");

            if (menuBtn == default)
            {
                var docs = UnityEngine.Object.FindObjectsOfType<UIDocument>(FindObjectsSortMode.None);
                foreach (var d in docs)
                {
                    if (d == default || d.rootVisualElement == default) continue;
                    var b = d.rootVisualElement.Q<Button>("MenuButton") ?? d.rootVisualElement.Q<Button>(className: "menu-button") ?? d.rootVisualElement.Q<Button>("Menu");
                    if (b != default) { menuBtn = b; break; }
                }
            }

            optionsBtn = root.Q<Button>("OptionsButton")
                         ?? root.Q<Button>(className: "options-button");

            // Bind button click events (avoid double-binding by removing first)
            if (characterBtn != default)
            {
                characterBtn.clicked -= OnCharacter;
                characterBtn.clicked += OnCharacter;
            }
            else Debug.LogWarning("[MLPGameUI] CharacterButton not found.");

            if (menuBtn != default)
            {
                menuBtn.clicked -= OnMenu;
                menuBtn.clicked += OnMenu;
            }
            else Debug.LogWarning("[MLPGameUI] MenuButton not found.");

            if (optionsBtn != default)
            {
                optionsBtn.clicked -= OnOptions;
                optionsBtn.clicked += OnOptions;
            }
            else Debug.Log("[MLPGameUI] OptionsButton not found (optional).");
        }

        private void OnDisable()
        {
            if (characterBtn != default) characterBtn.clicked -= OnCharacter;
            if (menuBtn != default) menuBtn.clicked -= OnMenu;
            if (optionsBtn != default) optionsBtn.clicked -= OnOptions;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // === UI Button Handlers ===

        private void OnCharacter()
        {
            // First, try using CharacterSheetController to show the sheet
            var csc = FindFirstObjectByType<CharacterSheetController>();
            if (csc != default)
            {
                csc.ShowCharacterSheet();  // always show (avoids toggle issues)
                return;
            }

            // Fallback: directly make any known character sheet container visible
            var docs = Object.FindObjectsByType<UIDocument>(FindObjectsSortMode.None);
            foreach (var d in docs)
            {
                if (d == default || d.rootVisualElement == default) continue;
                var sheet = d.rootVisualElement.Q<VisualElement>("character-sheet-container")
                          ?? d.rootVisualElement.Q<VisualElement>("character-sheet-modal")
                          ?? d.rootVisualElement.Q<VisualElement>("CharacterSheet")
                          ?? d.rootVisualElement.Q<VisualElement>("CharacterSheetRoot")
                          ?? d.rootVisualElement.Q<VisualElement>("CharacterPanel")
                          ?? d.rootVisualElement.Q<VisualElement>("Character_Sheet")
                          ?? d.rootVisualElement.Q<VisualElement>(className: "character-sheet");
                if (sheet != default)
                {
                    sheet.style.display = DisplayStyle.Flex;
                    sheet.style.visibility = Visibility.Visible;
                    sheet.style.opacity = 1f;
                    d.rootVisualElement.BringToFront();
                    if (d.panelSettings != default && d.panelSettings.sortingOrder < 200)
                        d.panelSettings.sortingOrder = 200;
                    return;
                }
            }

            Debug.LogWarning("[MLPGameUI] Character sheet not found in any UIDocument.");
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
    }
}
