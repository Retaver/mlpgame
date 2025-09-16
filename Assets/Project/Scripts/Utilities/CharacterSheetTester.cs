using UnityEngine;
using UnityEngine.UIElements;

namespace MyGameNamespace
{
    /// <summary>
    /// Simple test script to verify character sheet functionality
    /// Attach this to any GameObject in your scene to test the character button
    /// </summary>
    public class CharacterSheetTester : MonoBehaviour
    {
        private void Start()
        {
            Debug.Log("=== CHARACTER SHEET TEST STARTED ===");

            // Check if MLPGameUI exists
            if (MLPGameUI.Instance == null)
            {
                Debug.LogError("❌ MLPGameUI.Instance is null - MLPGameUI not properly initialized!");
                return;
            }
            else
            {
                Debug.Log("✅ MLPGameUI.Instance found");
            }

            // Check if MLPGameUI has a UIDocument
            if (MLPGameUI.Instance.GetComponent<UIDocument>() == null)
            {
                Debug.LogError("❌ MLPGameUI has no UIDocument component!");
                return;
            }
            else
            {
                Debug.Log("✅ MLPGameUI has UIDocument");
            }

            // Check if character sheet modal exists
            var uiDoc = MLPGameUI.Instance.GetComponent<UIDocument>();
            var root = uiDoc.rootVisualElement;
            var sheetModal = root.Q<VisualElement>("character-sheet-modal");

            if (sheetModal == null)
            {
                Debug.LogError("❌ Character sheet modal not found in MLPGameUI!");
                return;
            }
            else
            {
                Debug.Log("✅ Character sheet modal found");
            }

            // Check if character button exists
            var charBtn = root.Q<Button>("CharacterButton");
            if (charBtn == null)
            {
                Debug.LogError("❌ CharacterButton not found in MLPGameUI!");
                return;
            }
            else
            {
                Debug.Log($"✅ CharacterButton found: {charBtn.name} (text: {charBtn.text})");
            }

            // Check if close button exists
            var closeBtn = root.Q<Button>("close-button");
            if (closeBtn == null)
            {
                Debug.LogError("❌ Close button not found in character sheet!");
                return;
            }
            else
            {
                Debug.Log("✅ Close button found in character sheet");
            }

            // Check if tabs exist
            var statsTab = root.Q<Button>("stats-tab");
            var skillsTab = root.Q<Button>("skills-tab");
            var perksTab = root.Q<Button>("perks-tab");
            var effectsTab = root.Q<Button>("effects-tab");

            if (statsTab == null || skillsTab == null || perksTab == null || effectsTab == null)
            {
                Debug.LogError("❌ Some character sheet tabs not found!");
                return;
            }
            else
            {
                Debug.Log("✅ All character sheet tabs found");
            }

            // Check if panels exist
            var statsPanel = root.Q<VisualElement>("stats-panel");
            var skillsPanel = root.Q<VisualElement>("skills-panel");
            var perksPanel = root.Q<VisualElement>("perks-panel");
            var effectsPanel = root.Q<VisualElement>("effects-panel");

            if (statsPanel == null || skillsPanel == null || perksPanel == null || effectsPanel == null)
            {
                Debug.LogError("❌ Some character sheet panels not found!");
                return;
            }
            else
            {
                Debug.Log("✅ All character sheet panels found");
            }

            Debug.Log("=== CHARACTER SHEET TEST COMPLETED SUCCESSFULLY ===");
            Debug.Log("Click the Character button in your game to test the character sheet!");
            Debug.Log("Use number keys 1-4 to test tabs: 1=Stats, 2=Skills, 3=Perks, 4=Effects");
        }

        private void Update()
        {
            // Press C key to test character sheet programmatically
            if (Input.GetKeyDown(KeyCode.C))
            {
                Debug.Log("Testing character sheet via C key...");
                if (MLPGameUI.Instance != null)
                {
                    // This will call the OnCharacter method
                    var uiDoc = MLPGameUI.Instance.GetComponent<UIDocument>();
                    var root = uiDoc.rootVisualElement;
                    var charBtn = root.Q<Button>("CharacterButton");

                    if (charBtn != null)
                    {
                        charBtn.SendEvent(new ClickEvent());
                        Debug.Log("Character button clicked programmatically");
                    }
                    else
                    {
                        Debug.LogError("Could not find CharacterButton to test");
                    }
                }
            }

            // Test tabs with number keys (only when character sheet is visible)
            if (MLPGameUI.Instance != null)
            {
                var uiDoc = MLPGameUI.Instance.GetComponent<UIDocument>();
                var root = uiDoc.rootVisualElement;
                var sheetModal = root.Q<VisualElement>("character-sheet-modal");

                if (sheetModal != null && sheetModal.style.display == DisplayStyle.Flex)
                {
                    if (Input.GetKeyDown(KeyCode.Alpha1))
                    {
                        var statsTab = root.Q<Button>("stats-tab");
                        if (statsTab != null) statsTab.SendEvent(new ClickEvent());
                        Debug.Log("Testing Stats tab (1 key)");
                    }
                    else if (Input.GetKeyDown(KeyCode.Alpha2))
                    {
                        var skillsTab = root.Q<Button>("skills-tab");
                        if (skillsTab != null) skillsTab.SendEvent(new ClickEvent());
                        Debug.Log("Testing Skills tab (2 key)");
                    }
                    else if (Input.GetKeyDown(KeyCode.Alpha3))
                    {
                        var perksTab = root.Q<Button>("perks-tab");
                        if (perksTab != null) perksTab.SendEvent(new ClickEvent());
                        Debug.Log("Testing Perks tab (3 key)");
                    }
                    else if (Input.GetKeyDown(KeyCode.Alpha4))
                    {
                        var effectsTab = root.Q<Button>("effects-tab");
                        if (effectsTab != null) effectsTab.SendEvent(new ClickEvent());
                        Debug.Log("Testing Effects tab (4 key)");
                    }
                }
            }
        }
    }
}