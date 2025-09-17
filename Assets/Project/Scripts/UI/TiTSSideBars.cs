using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace MyGameNamespace.UI
{
    /// <summary>
    /// TiTS-inspired side bar system with animated stat bars
    /// Features: Health, Energy, Magic, Friendship, Discord bars with smooth animations
    /// </summary>
    public class TiTSSideBars : VisualElement
    {
        // Bar data structure
        public class StatBarData
        {
            public string Name { get; set; }
            public float CurrentValue { get; set; }
            public float MaxValue { get; set; }
            public Color BarColor { get; set; }
            public Color BackgroundColor { get; set; }
            public string IconName { get; set; }
        }

        private readonly Dictionary<string, ProgressBar> statBars = new();
        private readonly Dictionary<string, Label> statLabels = new();
        private readonly Dictionary<string, VisualElement> statIcons = new();

        // Animation settings
        private const float LOW_THRESHOLD = 0.25f;
        private const float CRITICAL_THRESHOLD = 0.1f;

        public TiTSSideBars()
        {
            AddToClassList("tits-side-bars");
            CreateSideBars();
        }

        private void CreateSideBars()
        {
            // Create left side bars container
            var leftBars = new VisualElement();
            leftBars.AddToClassList("tits-left-bars");
            Add(leftBars);

            // Create right side bars container
            var rightBars = new VisualElement();
            rightBars.AddToClassList("tits-right-bars");
            Add(rightBars);

            // Initialize stat bars
            CreateStatBar(leftBars, "health", "❤️", new Color(0.9f, 0.2f, 0.2f));
            CreateStatBar(leftBars, "energy", "⚡", new Color(0.2f, 0.7f, 0.9f));
            CreateStatBar(rightBars, "magic", "🔮", new Color(0.7f, 0.2f, 0.9f));
            CreateStatBar(rightBars, "friendship", "💝", new Color(0.9f, 0.6f, 0.2f));
            CreateStatBar(rightBars, "discord", "👹", new Color(0.5f, 0.2f, 0.7f));
        }

        private void CreateStatBar(VisualElement parent, string statName, string iconText, Color barColor)
        {
            var barContainer = new VisualElement();
            barContainer.AddToClassList("tits-stat-bar-container");
            barContainer.AddToClassList($"tits-{statName}-bar");
            parent.Add(barContainer);

            // Icon
            var icon = new Label(iconText);
            icon.AddToClassList("tits-stat-icon");
            barContainer.Add(icon);
            statIcons[statName] = icon;

            // Bar container
            var barWrapper = new VisualElement();
            barWrapper.AddToClassList("tits-bar-wrapper");
            barContainer.Add(barWrapper);

            // Progress bar
            var progressBar = new ProgressBar();
            progressBar.AddToClassList("tits-stat-bar");
            progressBar.lowValue = 0;
            progressBar.highValue = 100;
            progressBar.value = 100;
            barWrapper.Add(progressBar);
            statBars[statName] = progressBar;

            // Value label
            var label = new Label("100/100");
            label.AddToClassList("tits-stat-label");
            barWrapper.Add(label);
            statLabels[statName] = label;

            // Set bar color
            progressBar.style.backgroundColor = new Color(barColor.r, barColor.g, barColor.b, 0.3f);
            var fill = progressBar.Q<VisualElement>("unity-progress-bar__progress");
            if (fill != null)
            {
                fill.style.backgroundColor = barColor;
            }
        }

        /// <summary>
        /// Update a stat bar value with smooth animation
        /// </summary>
        public void UpdateStat(string statName, float currentValue, float maxValue)
        {
            if (!statBars.ContainsKey(statName) || !statLabels.ContainsKey(statName))
                return;

            var bar = statBars[statName];
            var label = statLabels[statName];

            // Update label
            label.text = $"{Mathf.RoundToInt(currentValue)}/{Mathf.RoundToInt(maxValue)}";

            // Calculate percentage
            float percentage = maxValue > 0 ? (currentValue / maxValue) : 0f;
            float targetValue = percentage * 100f;

            // Animate bar directly (no coroutine needed for UI Toolkit)
            AnimateBar(bar, targetValue, statName);
        }

        private void AnimateBar(ProgressBar bar, float targetValue, string statName)
        {
            // Set the value directly for UI Toolkit
            bar.value = targetValue;

            // Determine status class
            string statusClass = GetStatusClass(targetValue / 100f);

            // Update status classes
            UpdateBarStatus(bar, statusClass);
        }

        private string GetStatusClass(float percentage)
        {
            if (percentage <= CRITICAL_THRESHOLD) return "critical";
            if (percentage <= LOW_THRESHOLD) return "low";
            return "normal";
        }

        private void UpdateBarStatus(ProgressBar bar, string statusClass)
        {
            bar.RemoveFromClassList("normal");
            bar.RemoveFromClassList("low");
            bar.RemoveFromClassList("critical");
            bar.AddToClassList(statusClass);
        }

        /// <summary>
        /// Set all stats at once (useful for initialization)
        /// </summary>
        public void SetAllStats(float health, float maxHealth, float energy, float maxEnergy,
                               float magic, float maxMagic, float friendship, float maxFriendship,
                               float discord, float maxDiscord)
        {
            UpdateStat("health", health, maxHealth);
            UpdateStat("energy", energy, maxEnergy);
            UpdateStat("magic", magic, maxMagic);
            UpdateStat("friendship", friendship, maxFriendship);
            UpdateStat("discord", discord, maxDiscord);
        }

        /// <summary>
        /// Get current stat value
        /// </summary>
        public float GetStatValue(string statName)
        {
            if (statBars.ContainsKey(statName))
            {
                return statBars[statName].value;
            }
            return 0f;
        }
    }

    /// <summary>
    /// Extension methods for easy integration
    /// </summary>
    public static class TiTSSideBarsExtensions
    {
        public static TiTSSideBars AddTiTSSideBars(this VisualElement parent)
        {
            var sideBars = new TiTSSideBars();
            parent.Add(sideBars);
            return sideBars;
        }
    }
}