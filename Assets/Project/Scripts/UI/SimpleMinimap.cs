using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using MyGameNamespace;

namespace MyGameNamespace.UI
{
    /// <summary>
    /// Simple minimap component for MLP game HUD
    /// Shows current location and nearby points of interest
    /// </summary>
    public class SimpleMinimap : VisualElement
    {
        private const float MAP_SIZE = 150f;
        private const float ICON_SIZE = 12f;

        private VisualElement mapContainer;
        private VisualElement playerIcon;
        private Dictionary<string, VisualElement> locationIcons = new();

        public SimpleMinimap()
        {
            CreateMinimap();
            RefreshMinimap();
        }

        private void CreateMinimap()
        {
            // Main map container
            mapContainer = new VisualElement();
            mapContainer.name = "minimap-background";
            mapContainer.AddToClassList("minimap-background");
            mapContainer.style.width = MAP_SIZE;
            mapContainer.style.height = MAP_SIZE;
            mapContainer.style.backgroundColor = new Color(22/255f, 18/255f, 24/255f); /* Twilight menu card background */
            mapContainer.style.borderTopLeftRadius = 8;
            mapContainer.style.borderTopRightRadius = 8;
            mapContainer.style.borderBottomLeftRadius = 8;
            mapContainer.style.borderBottomRightRadius = 8;
            mapContainer.style.borderTopWidth = 2;
            mapContainer.style.borderBottomWidth = 2;
            mapContainer.style.borderLeftWidth = 2;
            mapContainer.style.borderRightWidth = 2;
            mapContainer.style.borderTopColor = new Color(108/255f, 92/255f, 110/255f); /* Twilight button border */
            mapContainer.style.borderBottomColor = new Color(108/255f, 92/255f, 110/255f);
            mapContainer.style.borderLeftColor = new Color(108/255f, 92/255f, 110/255f);
            mapContainer.style.borderRightColor = new Color(108/255f, 92/255f, 110/255f);
            Add(mapContainer);

            // Player icon (center of map)
            playerIcon = new VisualElement();
            playerIcon.name = "player-icon";
            playerIcon.AddToClassList("player-icon");
            playerIcon.style.position = Position.Absolute;
            playerIcon.style.width = ICON_SIZE;
            playerIcon.style.height = ICON_SIZE;
            playerIcon.style.left = (MAP_SIZE - ICON_SIZE) / 2;
            playerIcon.style.top = (MAP_SIZE - ICON_SIZE) / 2;
            playerIcon.style.backgroundColor = new Color(244/255f, 162/255f, 232/255f); /* Twilight title color for player */
            playerIcon.style.borderTopLeftRadius = ICON_SIZE / 2;
            playerIcon.style.borderTopRightRadius = ICON_SIZE / 2;
            playerIcon.style.borderBottomLeftRadius = ICON_SIZE / 2;
            playerIcon.style.borderBottomRightRadius = ICON_SIZE / 2;
            mapContainer.Add(playerIcon);

            // Add some sample location icons
            AddLocationIcon("Sweet Apple Acres", 30, 40, LocationType.Farm);
            AddLocationIcon("Carousel Boutique", 100, 60, LocationType.Shop);
            AddLocationIcon("Golden Oak Library", 80, 100, LocationType.Library);
            AddLocationIcon("Sugarcube Corner", 50, 80, LocationType.Shop);
        }

        private void AddLocationIcon(string name, float x, float y, LocationType type)
        {
            var icon = new VisualElement();
            icon.name = $"{name.ToLower().Replace(" ", "-")}-icon";
            icon.AddToClassList("location-icon");
            icon.AddToClassList($"location-{type.ToString().ToLower()}");
            icon.style.position = Position.Absolute;
            icon.style.width = ICON_SIZE;
            icon.style.height = ICON_SIZE;
            icon.style.left = x;
            icon.style.top = y;

            // Set color based on location type
            Color iconColor = GetLocationColor(type);
            icon.style.backgroundColor = iconColor;
            icon.style.borderTopLeftRadius = ICON_SIZE / 2;
            icon.style.borderTopRightRadius = ICON_SIZE / 2;
            icon.style.borderBottomLeftRadius = ICON_SIZE / 2;
            icon.style.borderBottomRightRadius = ICON_SIZE / 2;

            // Add tooltip
            icon.tooltip = name;

            mapContainer.Add(icon);
            locationIcons[name] = icon;
        }

        private Color GetLocationColor(LocationType type)
        {
            switch (type)
            {
                case LocationType.Farm: return new Color(108/255f, 92/255f, 110/255f); /* Twilight button border color for farms */
                case LocationType.Shop: return new Color(244/255f, 162/255f, 232/255f); /* Twilight title color for shops */
                case LocationType.Library: return new Color(180/255f, 120/255f, 200/255f); /* Twilight accent color for libraries */
                case LocationType.Home: return new Color(150/255f, 100/255f, 180/255f); /* Twilight secondary color for homes */
                case LocationType.Danger: return new Color(220/255f, 100/255f, 150/255f); /* Twilight warning color for danger */
                default: return new Color(80/255f, 70/255f, 85/255f); /* Twilight muted color for default */
            }
        }

        public void RefreshMinimap()
        {
            // Update player position (center for now)
            // In a real implementation, this would get position from the map system

            // Update location visibility based on discovery
            // For now, all locations are visible
        }

        public void UpdatePlayerPosition(Vector2 position)
        {
            // Convert world position to minimap coordinates
            float mapX = Mathf.Clamp((position.x / 100f) * MAP_SIZE + MAP_SIZE / 2, 0, MAP_SIZE - ICON_SIZE);
            float mapY = Mathf.Clamp((position.y / 100f) * MAP_SIZE + MAP_SIZE / 2, 0, MAP_SIZE - ICON_SIZE);

            playerIcon.style.left = mapX;
            playerIcon.style.top = mapY;
        }

        public void ShowLocation(string locationName, bool discovered = true)
        {
            if (locationIcons.TryGetValue(locationName, out var icon))
            {
                icon.style.display = discovered ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        public void HighlightLocation(string locationName, bool highlight = true)
        {
            if (locationIcons.TryGetValue(locationName, out var icon))
            {
                if (highlight)
                {
                    icon.style.borderTopWidth = 2;
                    icon.style.borderBottomWidth = 2;
                    icon.style.borderLeftWidth = 2;
                    icon.style.borderRightWidth = 2;
                    icon.style.borderTopColor = new Color(244/255f, 162/255f, 232/255f); /* Twilight title color for highlight */
                    icon.style.borderBottomColor = new Color(244/255f, 162/255f, 232/255f);
                    icon.style.borderLeftColor = new Color(244/255f, 162/255f, 232/255f);
                    icon.style.borderRightColor = new Color(244/255f, 162/255f, 232/255f);
                }
                else
                {
                    icon.style.borderTopWidth = 0;
                    icon.style.borderBottomWidth = 0;
                    icon.style.borderLeftWidth = 0;
                    icon.style.borderRightWidth = 0;
                }
            }
        }
    }

    public enum LocationType
    {
        Home,
        Shop,
        Farm,
        Library,
        Danger,
        Quest,
        Friend
    }

    public static class MinimapExtensions
    {
        public static SimpleMinimap AddMinimap(this VisualElement parent)
        {
            var minimap = new SimpleMinimap();
            parent.Add(minimap);
            return minimap;
        }
    }
}