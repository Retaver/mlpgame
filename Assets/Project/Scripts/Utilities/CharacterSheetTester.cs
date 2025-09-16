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

            Debug.Log("=== CHARACTER SHEET TEST COMPLETED SUCCESSFULLY ===");
            Debug.Log("Click the Character button in your game to test the character sheet!");
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
        }
    }
}