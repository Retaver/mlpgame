using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Globalization;

namespace MyGameNamespace
{
    /// <summary>
    /// Binds selected Race + Gender to a portrait image.
    /// Place images under: Assets/Resources/Portraits/{Race}/{Gender}/portrait.png
    /// Example: Assets/Resources/Portraits/EarthPony/Female/portrait.png
    /// The control can be an Image with name "PortraitImage" or a VisualElement "Portrait".
    /// </summary>
    public class CharacterCreationPortraitBinder : MonoBehaviour
    {
        [SerializeField] private UIDocument document;
        [SerializeField] private string portraitImageName = "PortraitImage";
        [SerializeField] private string portraitElementName = "Portrait";

        private VisualElement root;
        private Image portraitImage;
        private VisualElement portraitElement;

        private string currentRace = "EarthPony";
        private string currentGender = "Female";

        private void Awake()
        {
            if (document == default) document = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            root = document != default ? document.rootVisualElement : null;
            if (root == default) { Debug.LogWarning("[PortraitBinder] Missing UIDocument/root."); return; }

            portraitImage = root.Q<Image>(portraitImageName);
            portraitElement = root.Q<VisualElement>(portraitElementName);

            ApplyPortrait();
        }

        /// <summary>Call this when the user changes race (e.g., from a dropdown/pills).</summary>
        public void SetRace(string raceLabel)
        {
            currentRace = NormalizeRace(raceLabel);
            ApplyPortrait();
        }

        /// <summary>Call this when the user toggles Male/Female.</summary>
        public void SetGender(string genderLabel)
        {
            currentGender = NormalizeGender(genderLabel);
            ApplyPortrait();
        }

        private void ApplyPortrait()
        {
            var path = $"Portraits/{currentRace}/{currentGender}/portrait";
            var sprite = Resources.Load<Sprite>(path);
            if (sprite == default)
            {
                Debug.LogWarning($"[PortraitBinder] Missing portrait at Resources/{path}.png");
                ClearPortrait();
                return;
            }

            if (portraitImage != default)
            {
                portraitImage.sprite = sprite;
            }
            if (portraitElement != default)
            {
                // Use background image without deprecated scale mode API.
                portraitElement.style.backgroundImage = new StyleBackground(sprite);
                // Optional (Unity 6.2+): center/contain if available via USS instead.
                // Set in USS: background-position: center; background-repeat: no-repeat; background-size: contain;
            }
        }

        private void ClearPortrait()
        {
            if (portraitImage != default) portraitImage.sprite = null;
            if (portraitElement != default) portraitElement.style.backgroundImage = null;
        }

        private static string NormalizeRace(string label)
        {
            if (string.IsNullOrEmpty(label)) return "EarthPony";
            label = label.Trim().ToLowerInvariant();
            if (label.Contains("earth")) return "EarthPony";
            if (label.Contains("unicorn")) return "Unicorn";
            if (label.Contains("pegas")) return "Pegasus";
            if (label.Contains("griff")) return "Griffon";
            if (label.Contains("dragon")) return "Dragon";
            if (label.Contains("human")) return "Human";
            return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(label).Replace(" ", "");
        }

        private static string NormalizeGender(string label)
        {
            if (string.IsNullOrEmpty(label)) return "Female";
            label = label.Trim();
            if (label.StartsWith("f")) return "Female";
            if (label.StartsWith("m")) return "Male";
            return char.ToUpperInvariant(label[0]) + label.Substring(1);
        }
    }
}