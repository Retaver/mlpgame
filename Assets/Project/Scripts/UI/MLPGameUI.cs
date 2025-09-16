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
        private Button characterBtn, menuBtn, optionsBtn, inventoryBtn;

        private void Awake()
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

            // Ensure character sheet is hidden initially
            var sheetModal = root.Q<VisualElement>("character-sheet-modal");
            if (sheetModal != default)
            {
                sheetModal.style.display = DisplayStyle.None;
                Debug.Log("[MLPGameUI] Character sheet modal initialized as hidden");
            }
            else
            {
                Debug.LogWarning("[MLPGameUI] Character sheet modal not found in UXML!");
            }

            // Locate buttons by various possible names or class for compatibility
            characterBtn = FindButtonRobust("CharacterButton", "Character", "character-button");

            // If not found in this UIDocument, search other documents in scene
            if (characterBtn == default)
            {
                var docs = UnityEngine.Object.FindObjectsByType<UIDocument>(FindObjectsInactive.Include, FindObjectsSortMode.None);
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
                var docs = UnityEngine.Object.FindObjectsByType<UIDocument>(FindObjectsInactive.Include, FindObjectsSortMode.None);
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
                var docs = UnityEngine.Object.FindObjectsByType<UIDocument>(FindObjectsInactive.Include, FindObjectsSortMode.None);
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

            Debug.Log("[MLPGameUI] Character sheet shown successfully");
        }

        private void OnCharacterClose()
        {
            Debug.Log("[MLPGameUI] Closing character sheet");

            if (root == default)
            {
                Debug.LogError("[MLPGameUI] No root visual element found for closing!");
                return;
            }

            var sheetModal = root.Q<VisualElement>("character-sheet-modal");
            if (sheetModal == default)
            {
                Debug.LogError("[MLPGameUI] Character sheet modal not found for closing!");
                return;
            }

            // Hide the character sheet
            sheetModal.style.display = DisplayStyle.None;
            Debug.Log("[MLPGameUI] Character sheet closed successfully");
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
    }
}
