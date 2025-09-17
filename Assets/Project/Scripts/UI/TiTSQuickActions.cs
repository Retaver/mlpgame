using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace MyGameNamespace.UI
{
    /// <summary>
    /// TiTS-inspired quick actions panel with hotkeys and radial menu
    /// Features: Quick access to common actions, hotkey system, radial layout
    /// </summary>
    public class TiTSQuickActions : VisualElement
    {
        public enum QuickActionType
        {
            Inventory,
            Character,
            Map,
            Quests,
            Spells,
            Items,
            Social,
            Settings
        }

        private class QuickActionData
        {
            public QuickActionType Type { get; set; }
            public string Label { get; set; }
            public string Icon { get; set; }
            public KeyCode Hotkey { get; set; }
            public System.Action OnClick { get; set; }
        }

        // Quick action buttons
        private readonly Dictionary<QuickActionType, Button> actionButtons = new();
        private readonly List<QuickActionData> actions = new();

        // Settings
        private const float BUTTON_SIZE = 50f;
        private const float SPACING = 8f;

        public TiTSQuickActions()
        {
            AddToClassList("tits-quick-actions");
            InitializeActions();
            CreateQuickActionsPanel();
        }

        private void InitializeActions()
        {
            actions.Add(new QuickActionData
            {
                Type = QuickActionType.Inventory,
                Label = "Inventory",
                Icon = "🎒",
                Hotkey = KeyCode.I,
                OnClick = () => Debug.Log("Opening Inventory...")
            });

            actions.Add(new QuickActionData
            {
                Type = QuickActionType.Character,
                Label = "Character",
                Icon = "👤",
                Hotkey = KeyCode.C,
                OnClick = () => Debug.Log("Opening Character Sheet...")
            });

            actions.Add(new QuickActionData
            {
                Type = QuickActionType.Map,
                Label = "Map",
                Icon = "🗺️",
                Hotkey = KeyCode.M,
                OnClick = () => Debug.Log("Opening World Map...")
            });

            actions.Add(new QuickActionData
            {
                Type = QuickActionType.Quests,
                Label = "Quests",
                Icon = "📜",
                Hotkey = KeyCode.Q,
                OnClick = () => Debug.Log("Opening Quest Log...")
            });

            actions.Add(new QuickActionData
            {
                Type = QuickActionType.Spells,
                Label = "Spells",
                Icon = "🔮",
                Hotkey = KeyCode.S,
                OnClick = () => Debug.Log("Opening Spell Book...")
            });

            actions.Add(new QuickActionData
            {
                Type = QuickActionType.Items,
                Label = "Items",
                Icon = "⚔️",
                Hotkey = KeyCode.Tab,
                OnClick = () => Debug.Log("Opening Item Quick Select...")
            });

            actions.Add(new QuickActionData
            {
                Type = QuickActionType.Social,
                Label = "Social",
                Icon = "💬",
                Hotkey = KeyCode.T,
                OnClick = () => Debug.Log("Opening Social Panel...")
            });

            actions.Add(new QuickActionData
            {
                Type = QuickActionType.Settings,
                Label = "Settings",
                Icon = "⚙️",
                Hotkey = KeyCode.Escape,
                OnClick = () => Debug.Log("Opening Settings...")
            });
        }

        private void CreateQuickActionsPanel()
        {
            // Main container
            var container = new VisualElement();
            container.AddToClassList("tits-quick-actions-container");
            Add(container);

            // Title
            var title = new Label("Quick Actions");
            title.AddToClassList("tits-quick-actions-title");
            container.Add(title);

            // Actions grid
            var actionsGrid = new VisualElement();
            actionsGrid.AddToClassList("tits-actions-grid");
            container.Add(actionsGrid);

            // Create action buttons
            foreach (var action in actions)
            {
                CreateActionButton(actionsGrid, action);
            }

            // Hotkey hints
            var hotkeyHints = new VisualElement();
            hotkeyHints.AddToClassList("tits-hotkey-hints");
            container.Add(hotkeyHints);

            var hintsLabel = new Label("Press hotkeys for quick access");
            hintsLabel.AddToClassList("tits-hints-label");
            hotkeyHints.Add(hintsLabel);
        }

        private void CreateActionButton(VisualElement parent, QuickActionData action)
        {
            var button = new Button(() => action.OnClick?.Invoke());
            button.AddToClassList("tits-action-button");
            button.AddToClassList($"tits-action-{action.Type.ToString().ToLower()}");
            button.tooltip = $"{action.Label} ({action.Hotkey})";

            // Icon
            var icon = new Label(action.Icon);
            icon.AddToClassList("tits-action-icon");
            button.Add(icon);

            // Label
            var label = new Label(action.Label);
            label.AddToClassList("tits-action-label");
            button.Add(label);

            // Hotkey indicator
            var hotkey = new Label(action.Hotkey.ToString());
            hotkey.AddToClassList("tits-action-hotkey");
            button.Add(hotkey);

            parent.Add(button);
            actionButtons[action.Type] = button;
        }

        /// <summary>
        /// Handle keyboard input for hotkeys
        /// </summary>
        public void HandleInput()
        {
            foreach (var action in actions)
            {
                if (Input.GetKeyDown(action.Hotkey))
                {
                    action.OnClick?.Invoke();
                    HighlightButton(action.Type);
                    break;
                }
            }
        }

        /// <summary>
        /// Highlight a button temporarily
        /// </summary>
        private void HighlightButton(QuickActionType type)
        {
            if (actionButtons.ContainsKey(type))
            {
                var button = actionButtons[type];
                button.AddToClassList("tits-action-highlighted");

                // Remove highlight after a short time
                RemoveHighlight(button);
            }
        }

        private void RemoveHighlight(VisualElement button)
        {
            button.schedule.Execute(() =>
            {
                button.RemoveFromClassList("tits-action-highlighted");
            }).ExecuteLater(200); // 200ms delay
        }

        /// <summary>
        /// Enable/disable a quick action
        /// </summary>
        public void SetActionEnabled(QuickActionType type, bool enabled)
        {
            if (actionButtons.ContainsKey(type))
            {
                actionButtons[type].SetEnabled(enabled);
            }
        }

        /// <summary>
        /// Update action callback
        /// </summary>
        public void SetActionCallback(QuickActionType type, System.Action callback)
        {
            var action = actions.Find(a => a.Type == type);
            if (action != null)
            {
                action.OnClick = callback;
            }
        }

        /// <summary>
        /// Add custom action
        /// </summary>
        public void AddCustomAction(QuickActionType type, string label, string icon,
                                   KeyCode hotkey, System.Action onClick)
        {
            var action = actions.Find(a => a.Type == type);
            if (action != null)
            {
                action.Label = label;
                action.Icon = icon;
                action.Hotkey = hotkey;
                action.OnClick = onClick;

                // Update existing button if it exists
                if (actionButtons.ContainsKey(type))
                {
                    UpdateButtonContent(actionButtons[type], action);
                }
            }
        }

        private void UpdateButtonContent(Button button, QuickActionData action)
        {
            var icon = button.Q<Label>("tits-action-icon");
            var label = button.Q<Label>("tits-action-label");
            var hotkey = button.Q<Label>("tits-action-hotkey");

            if (icon != null) icon.text = action.Icon;
            if (label != null) label.text = action.Label;
            if (hotkey != null) hotkey.text = action.Hotkey.ToString();

            button.tooltip = $"{action.Label} ({action.Hotkey})";
        }
    }

    public static class TiTSQuickActionsExtensions
    {
        public static TiTSQuickActions AddTiTSQuickActions(this VisualElement parent)
        {
            var quickActions = new TiTSQuickActions();
            parent.Add(quickActions);
            return quickActions;
        }
    }
}