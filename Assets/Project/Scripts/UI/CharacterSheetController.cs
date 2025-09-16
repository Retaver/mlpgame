
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace MyGameNamespace
{
    /// <summary>
    /// Lightweight, non-fullscreen Character Sheet controller for UI Toolkit (Unity 6.2).
    /// - No absolute/fullscreen sizing; respects your UXML/USS layout.
    /// - Clean show/hide via display style.
    /// - ESC and Close button support.
    /// - Static helpers + legacy global adapter compatibility.
    /// </summary>
    [DisallowMultipleComponent]
    public class CharacterSheetController : MonoBehaviour
    {
        [Header("Optional: assign if the UIDocument is not on this GameObject")]
        [SerializeField] private UIDocument sheetDocument;

        [Header("Optional element lookups (leave blank to auto-detect)")]
    // Accept more variants commonly used in UXML files (lowercase, hyphenated, modal/container names)
    [SerializeField] private string[] rootIds = new[] { "CharacterSheetRoot", "character-sheet-root", "CharacterSheet", "character-sheet", "character-sheet-container", "character-sheet-modal" };
    [SerializeField] private string[] rootClasses = new[] { "character-sheet", "panel--character", "sheet--character", "sheet-container", "sheet-modal-overlay" };
    // Include common close button names used in UXML (close-button, CloseButton)
    [SerializeField] private string[] closeIds = new[] { "CloseCharacterSheet", "close-character-sheet", "CloseButton", "close", "close-button" };
    [SerializeField] private string[] closeClasses = new[] { "close", "btn-close", "sheet__close", "sheet-button", "close-btn" };

        [Header("Behavior")]
        [Tooltip("Hide the sheet on Awake so it doesn't pop in during scene load.")]
        [SerializeField] private bool hideOnAwake = true;

        private UIDocument _doc;
        private VisualElement _root;   // the UIDocument.rootVisualElement
        private VisualElement _panel;  // the sheet panel container
        private Button _closeBtn;

        public bool IsOpen => _panel != null && _panel.resolvedStyle.display != DisplayStyle.None;

        #region Unity

        private void Awake()
        {
            CacheDocument();
            if (hideOnAwake) ForceHidden();
        }

        private void OnEnable()
        {
            TryWire();
        }

        private void OnDisable()
        {
            Unwire();
        }

        #endregion

        private void CacheDocument()
        {
            _doc = sheetDocument;
            if (_doc == null) TryGetComponent(out _doc);
            if (_doc == null) _doc = GetComponentInChildren<UIDocument>(true);
            // If still null, search the scene for any UIDocument that contains character sheet elements
            if (_doc == null)
            {
                // Note: Unity's FindObjectsOfType overload here expects a bool (includeInactive)
                var docs = UnityEngine.Object.FindObjectsByType<UIDocument>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                foreach (var d in docs)
                {
                    if (d == default || d.rootVisualElement == default) continue;
                    var found = d.rootVisualElement.Q<VisualElement>(className: "character-sheet")
                                ?? d.rootVisualElement.Q<VisualElement>("character-sheet-container")
                                ?? d.rootVisualElement.Q<VisualElement>("CharacterSheet")
                                ?? d.rootVisualElement.Q<VisualElement>("CharacterSheetRoot");
                    if (found != default)
                    {
                        _doc = d;
                        break;
                    }
                }
            }
            _root = _doc != null ? _doc.rootVisualElement : null;
        }

        private void TryWire()
        {
            CacheDocument();
            if (_root == null)
            {
                Debug.LogWarning("[CharacterSheetController] No UIDocument/rootVisualElement found.");
                return;
            }

            // Find panel
            _panel = FindByIdOrClass(_root, rootIds, rootClasses);
            if (_panel == null)
            {
                // Fallback: a VisualElement with name "CharacterSheet"
                _panel = _root.Q<VisualElement>("CharacterSheet");
            }

            if (_panel == null)
            {
                Debug.LogWarning("[CharacterSheetController] Could not locate character sheet panel in the UIDocument.");
                return;
            }

            // Find close button
            _closeBtn = FindButtonByIdOrClassOrText(_panel, closeIds, closeClasses, new[] { "Close", "×", "X" });

            if (_closeBtn != null)
            {
                _closeBtn.clicked -= OnCloseClicked;
                _closeBtn.clicked += OnCloseClicked;
            }

            // ESC to close, panel-scoped
            _panel.UnregisterCallback<KeyDownEvent>(OnKeyDown);
            _panel.RegisterCallback<KeyDownEvent>(OnKeyDown);
        }

        private void Unwire()
        {
            if (_closeBtn != null)
            {
                _closeBtn.clicked -= OnCloseClicked;
            }
            if (_panel != null)
            {
                _panel.UnregisterCallback<KeyDownEvent>(OnKeyDown);
            }
        }

        #region Public API

        public void Show()
        {
            TryWire();
            if (_panel == null) return;
            SetDisplay(_panel, DisplayStyle.Flex);
            _panel.Focus();
            Debug.Log("[CharacterSheet] Show");
        }

        public void Hide()
        {
            if (_panel == null) { TryWire(); if (_panel == null) return; }
            SetDisplay(_panel, DisplayStyle.None);
            Debug.Log("[CharacterSheet] Hide");
        }

        public void ToggleCharacterSheet()
        {
            TryWire();
            if (_panel == null) return;
            bool hidden = _panel.resolvedStyle.display == DisplayStyle.None;
            if (hidden) Show(); else Hide();
        }

        // Legacy method names used in some call sites
        public void ShowCharacterSheet() => Show();
        public void HideCharacterSheet() => Hide();

        #endregion

        #region Static helpers

        private static CharacterSheetController FindFirst()
        {
            // Prefer First, then Any (Unity 6.2 safe)
            var inst = UnityEngine.Object.FindFirstObjectByType<CharacterSheetController>();
            if (inst == null) inst = UnityEngine.Object.FindAnyObjectByType<CharacterSheetController>();
            return inst;
        }

        public static void Open() => FindFirst()?.Show();
        public static void Close() => FindFirst()?.Hide();
        public static void Toggle() => FindFirst()?.ToggleCharacterSheet();

        #endregion

        #region Private helpers & events

        private void OnCloseClicked() => Hide();

        private void OnKeyDown(KeyDownEvent evt)
        {
            if (evt == null) return;
            if (evt.keyCode == KeyCode.Escape)
            {
                Hide();
                evt.StopImmediatePropagation();
            }
        }

        private static void SetDisplay(VisualElement ve, DisplayStyle style)
        {
            if (ve != null) ve.style.display = style;
        }

        private static Button FindButtonByIdOrClassOrText(VisualElement root, string[] ids, string[] classes, string[] texts)
        {
            if (root == null) return null;

            if (ids != null)
            {
                foreach (var id in ids)
                {
                    var b = root.Q<Button>(id);
                    if (b != null) return b;
                }
            }

            if (classes != null)
            {
                foreach (var cls in classes)
                {
                    var b = root.Q<Button>(className: cls);
                    if (b != null) return b;
                }
            }

            if (texts != null)
            {
                foreach (var t in texts)
                {
                    var b = FindButtonByText(root, t);
                    if (b != null) return b;
                }
            }

            return null;
        }

        private static Button FindButtonByText(VisualElement root, string text)
        {
            if (root == null) return null;
            Button found = null;
            root.Query<Button>().ForEach(b =>
            {
                if (found != null || b == null) return;
                // Compare without case; allow surrounding whitespace
                if (string.Equals(b.text?.Trim(), text, StringComparison.OrdinalIgnoreCase))
                    found = b;
            });
            return found;
        }

        private static VisualElement FindByIdOrClass(VisualElement root, string[] ids, string[] classes)
        {
            if (root == null) return null;

            if (ids != null)
            {
                foreach (var id in ids)
                {
                    var ve = root.Q<VisualElement>(id);
                    if (ve != null) return ve;
                }
            }

            if (classes != null)
            {
                foreach (var cls in classes)
                {
                    var ve = root.Q<VisualElement>(className: cls);
                    if (ve != null) return ve;
                }
            }

            return null;
        }

        private void ForceHidden()
        {
            TryWire();
            if (_panel != null)
                SetDisplay(_panel, DisplayStyle.None);
        }

        #endregion
    }
}
