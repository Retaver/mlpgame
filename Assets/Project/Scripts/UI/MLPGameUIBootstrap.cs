using UnityEngine;
using UnityEngine.UIElements;

namespace MyGameNamespace
{
    /// <summary>
    /// Boots the HUD and spawns the Character Sheet on its own panel with higher sorting order.
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public sealed class MLPGameUIBootstrap : MonoBehaviour
    {
        [Header("HUD")]
        [SerializeField] private VisualTreeAsset gameHudUxml;
        [SerializeField] private StyleSheet gameHudStyles;

        [Header("Character Sheet")]
        [SerializeField] private VisualTreeAsset characterSheetUxml;
        [SerializeField] private StyleSheet characterSheetStyles;

        [Header("Overlay order (int)")]
        [SerializeField] private int overlaySortingOrder = 500;

        private UIDocument doc;

        private void Awake()
        {
            doc = GetComponent<UIDocument>();
            if (!doc)
            {
                Debug.LogError("[MLPGameUIBootstrap] No UIDocument on this GameObject.");
                enabled = false;
                return;
            }

            // Bind HUD assets
            if (gameHudUxml != default && doc.visualTreeAsset != gameHudUxml)
            {
                doc.visualTreeAsset = gameHudUxml;
                Debug.Log("[MLPGameUIBootstrap] Loaded gameHudUxml");
            }

            if (gameHudStyles != default && !doc.rootVisualElement.styleSheets.Contains(gameHudStyles))
            {
                doc.rootVisualElement.styleSheets.Add(gameHudStyles);
                Debug.Log("[MLPGameUIBootstrap] Added gameHudStyles");
            }

            // Load sheet assets from Resources if not assigned
            if (characterSheetUxml == default)
                characterSheetUxml = Resources.Load<VisualTreeAsset>("UI/CharacterSheet");
            if (characterSheetStyles == default)
                characterSheetStyles = Resources.Load<StyleSheet>("UI/CharacterSheet");

            // Create a Character Sheet document on a cloned PanelSettings with higher order
            try
            {
                var existing = FindFirstObjectByType<CharacterSheetController>();
                if (existing == default && characterSheetUxml != default)
                {
                    // Compute max existing order
                    int maxOrder = 0;
                    foreach (var d in FindObjectsByType<UIDocument>(FindObjectsSortMode.None))
                        if (d && d.panelSettings)
                            maxOrder = Mathf.Max(maxOrder, (int)d.panelSettings.sortingOrder); // int overload

                    var sheetGO = new GameObject("CharacterSheetUI");
                    var sheetDoc = sheetGO.AddComponent<UIDocument>();
                    sheetDoc.visualTreeAsset = characterSheetUxml;

                    // Clone or create PanelSettings so we can set a higher int sortingOrder
                    if (doc.panelSettings != default)
                    {
                        var clone = ScriptableObject.Instantiate(doc.panelSettings);
                        clone.sortingOrder = Mathf.Max(overlaySortingOrder, maxOrder + 10); // all int
                        sheetDoc.panelSettings = clone;
                        Debug.Log($"[MLPGameUIBootstrap] Set sheet sorting order to {clone.sortingOrder}");
                    }
                    else
                    {
                        var newPs = ScriptableObject.CreateInstance<PanelSettings>();
                        newPs.sortingOrder = Mathf.Max(overlaySortingOrder, maxOrder + 10);  // all int
                        sheetDoc.panelSettings = newPs;
                        Debug.Log($"[MLPGameUIBootstrap] Created new PanelSettings with sorting order {newPs.sortingOrder}");
                    }

                    if (characterSheetStyles != default)
                        sheetDoc.rootVisualElement.styleSheets.Add(characterSheetStyles);

                    sheetGO.AddComponent<CharacterSheetController>(); // controller manages visibility
                    sheetDoc.rootVisualElement.style.display = DisplayStyle.None; // start hidden
                }
            }
            catch
            {
                Debug.LogWarning("[MLPGameUIBootstrap] Failed to bootstrap CharacterSheetController.");
            }
        }
    }
}
