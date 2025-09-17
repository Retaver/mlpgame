using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;

namespace MyGameNamespace.UI
{
    /// <summary>
    /// Enhanced TiTS-style minimap with advanced features
    /// Features: Zoom, fog of war, fast travel, location markers, compass
    /// </summary>
    public class TiTSMinimap : VisualElement
    {
        // Map settings
        private const int MAP_SIZE = 200;
        private const int ICON_SIZE = 8;
        private const float ZOOM_MIN = 0.5f;
        private const float ZOOM_MAX = 2.0f;
        private const float ZOOM_SPEED = 0.1f;

        // Map components
        private VisualElement mapContainer;
        private VisualElement mapViewport;
        private VisualElement playerIcon;
        private VisualElement compass;
        private Button zoomInButton;
        private Button zoomOutButton;
        private Label zoomLabel;

        // Map data
        private readonly Dictionary<string, VisualElement> locationIcons = new();
        private readonly Dictionary<string, LocationData> locationData = new();
        private readonly HashSet<string> discoveredLocations = new();
        private readonly HashSet<string> visitedLocations = new();

        // Map state
        private float currentZoom = 1.0f;
        private Vector2 mapOffset = Vector2.zero;
        private bool isDragging = false;
        private Vector2 lastMousePosition;

        // Player position
        private Vector2 playerPosition = Vector2.zero;
        private float playerRotation = 0f;

        public TiTSMinimap()
        {
            AddToClassList("tits-minimap");
            CreateMinimap();
            InitializeLocations();
        }

        private void CreateMinimap()
        {
            // Main container
            var mainContainer = new VisualElement();
            mainContainer.AddToClassList("tits-minimap-main");
            Add(mainContainer);

            // Header with controls
            var header = new VisualElement();
            header.AddToClassList("tits-minimap-header");
            mainContainer.Add(header);

            // Zoom controls
            var zoomControls = new VisualElement();
            zoomControls.AddToClassList("tits-zoom-controls");
            header.Add(zoomControls);

            zoomOutButton = new Button(() => ZoomOut()) { text = "-" };
            zoomOutButton.AddToClassList("tits-zoom-button");
            zoomControls.Add(zoomOutButton);

            zoomLabel = new Label("100%");
            zoomLabel.AddToClassList("tits-zoom-label");
            zoomControls.Add(zoomLabel);

            zoomInButton = new Button(() => ZoomIn()) { text = "+" };
            zoomInButton.AddToClassList("tits-zoom-button");
            zoomControls.Add(zoomInButton);

            // Compass
            compass = new VisualElement();
            compass.AddToClassList("tits-compass");
            header.Add(compass);

            var compassNeedle = new VisualElement();
            compassNeedle.AddToClassList("tits-compass-needle");
            compass.Add(compassNeedle);

            // Map viewport
            mapViewport = new VisualElement();
            mapViewport.AddToClassList("tits-map-viewport");
            mainContainer.Add(mapViewport);

            // Map container (this is what gets zoomed and panned)
            mapContainer = new VisualElement();
            mapContainer.AddToClassList("tits-map-container");
            mapContainer.style.width = MAP_SIZE;
            mapContainer.style.height = MAP_SIZE;
            mapViewport.Add(mapContainer);

            // Map background
            var mapBackground = new VisualElement();
            mapBackground.AddToClassList("tits-map-background");
            mapContainer.Add(mapBackground);

            // Player icon
            playerIcon = new VisualElement();
            playerIcon.AddToClassList("tits-player-icon");
            playerIcon.style.position = Position.Absolute;
            playerIcon.style.width = ICON_SIZE;
            playerIcon.style.height = ICON_SIZE;
            playerIcon.style.left = (MAP_SIZE - ICON_SIZE) / 2;
            playerIcon.style.top = (MAP_SIZE - ICON_SIZE) / 2;
            mapContainer.Add(playerIcon);

            // Register mouse events for panning
            mapViewport.RegisterCallback<MouseDownEvent>(OnMouseDown);
            mapViewport.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            mapViewport.RegisterCallback<MouseUpEvent>(OnMouseUp);
            mapViewport.RegisterCallback<WheelEvent>(OnWheel);

            UpdateZoom();
        }

        private void InitializeLocations()
        {
            // Initialize Ponyville locations
            AddLocation("Ponyville Town Square", 100, 100, LocationType.Town, "The heart of Ponyville");
            AddLocation("Sweet Apple Acres", 50, 60, LocationType.Farm, "Apple farming community");
            AddLocation("Carousel Boutique", 140, 80, LocationType.Shop, "Fashion and clothing");
            AddLocation("Golden Oak Library", 120, 120, LocationType.Library, "Books and knowledge");
            AddLocation("Sugarcube Corner", 90, 140, LocationType.Shop, "Sweets and treats");
            AddLocation("Everfree Forest", 30, 150, LocationType.Danger, "Mysterious and dangerous woods");
            AddLocation("Cloudsdale", 160, 40, LocationType.Town, "City in the clouds");
            AddLocation("Canterlot", 200, 20, LocationType.Capital, "Royal capital city");

            // Mark some locations as discovered
            DiscoverLocation("Ponyville Town Square");
            DiscoverLocation("Sweet Apple Acres");
            DiscoverLocation("Carousel Boutique");
        }

        private void AddLocation(string name, float x, float y, LocationType type, string description)
        {
            var location = new LocationData
            {
                Name = name,
                Position = new Vector2(x, y),
                Type = type,
                Description = description,
                IsDiscovered = false,
                IsVisited = false
            };

            locationData[name] = location;
        }

        public void DiscoverLocation(string locationName)
        {
            if (locationData.ContainsKey(locationName))
            {
                locationData[locationName].IsDiscovered = true;
                discoveredLocations.Add(locationName);
                CreateLocationIcon(locationName);
            }
        }

        public void VisitLocation(string locationName)
        {
            if (locationData.ContainsKey(locationName))
            {
                locationData[locationName].IsVisited = true;
                visitedLocations.Add(locationName);
                UpdateLocationIcon(locationName);
            }
        }

        private void CreateLocationIcon(string locationName)
        {
            if (!locationData.ContainsKey(locationName)) return;

            var location = locationData[locationName];
            var icon = new VisualElement();
            icon.AddToClassList("tits-location-icon");
            icon.AddToClassList($"tits-location-{location.Type.ToString().ToLower()}");
            icon.style.position = Position.Absolute;
            icon.style.width = ICON_SIZE;
            icon.style.height = ICON_SIZE;
            icon.style.left = location.Position.x;
            icon.style.top = location.Position.y;
            icon.tooltip = $"{location.Name}\n{location.Description}";

            // Add click handler for fast travel
            icon.RegisterCallback<ClickEvent>(evt => OnLocationClick(locationName));

            mapContainer.Add(icon);
            locationIcons[locationName] = icon;
        }

        private void UpdateLocationIcon(string locationName)
        {
            if (locationIcons.ContainsKey(locationName))
            {
                var icon = locationIcons[locationName];
                icon.AddToClassList("tits-location-visited");
            }
        }

        private void OnLocationClick(string locationName)
        {
            if (locationData.ContainsKey(locationName) && locationData[locationName].IsDiscovered)
            {
                // Trigger fast travel event
                Debug.Log($"Fast traveling to: {locationName}");
                // You would integrate this with your game's travel system
            }
        }

        public void UpdatePlayerPosition(Vector2 position, float rotation = 0f)
        {
            playerPosition = position;
            playerRotation = rotation;

            // Convert world position to map coordinates
            float mapX = Mathf.Clamp(position.x / 10f * MAP_SIZE + MAP_SIZE / 2, 0, MAP_SIZE - ICON_SIZE);
            float mapY = Mathf.Clamp(position.y / 10f * MAP_SIZE + MAP_SIZE / 2, 0, MAP_SIZE - ICON_SIZE);

            playerIcon.style.left = mapX;
            playerIcon.style.top = mapY;
            playerIcon.style.rotate = new Rotate(new Angle(rotation, AngleUnit.Degree));

            // Update compass
            UpdateCompass(rotation);
        }

        private void UpdateCompass(float rotation)
        {
            var needle = compass.Q<VisualElement>("tits-compass-needle");
            if (needle != null)
            {
                needle.style.rotate = new Rotate(new Angle(rotation, AngleUnit.Degree));
            }
        }

        private void ZoomIn()
        {
            currentZoom = Mathf.Min(currentZoom + ZOOM_SPEED, ZOOM_MAX);
            UpdateZoom();
        }

        private void ZoomOut()
        {
            currentZoom = Mathf.Max(currentZoom - ZOOM_SPEED, ZOOM_MIN);
            UpdateZoom();
        }

        private void UpdateZoom()
        {
            mapContainer.style.scale = new Scale(new Vector3(currentZoom, currentZoom, 1f));
            zoomLabel.text = $"{Mathf.RoundToInt(currentZoom * 100)}%";

            // Update zoom button states
            zoomInButton.SetEnabled(currentZoom < ZOOM_MAX);
            zoomOutButton.SetEnabled(currentZoom > ZOOM_MIN);
        }

        private void OnMouseDown(MouseDownEvent evt)
        {
            if (evt.button == 0) // Left mouse button
            {
                isDragging = true;
                lastMousePosition = evt.mousePosition;
                mapViewport.CaptureMouse();
                evt.StopPropagation();
            }
        }

        private void OnMouseMove(MouseMoveEvent evt)
        {
            if (isDragging)
            {
                Vector2 delta = evt.mousePosition - lastMousePosition;
                mapOffset += delta / currentZoom;
                lastMousePosition = evt.mousePosition;

                // Apply pan limits
                mapOffset.x = Mathf.Clamp(mapOffset.x, -50, 50);
                mapOffset.y = Mathf.Clamp(mapOffset.y, -50, 50);

                UpdatePan();
                evt.StopPropagation();
            }
        }

        private void OnMouseUp(MouseUpEvent evt)
        {
            if (isDragging && evt.button == 0)
            {
                isDragging = false;
                mapViewport.ReleaseMouse();
                evt.StopPropagation();
            }
        }

        private void OnWheel(WheelEvent evt)
        {
            if (evt.delta.y > 0)
                ZoomOut();
            else
                ZoomIn();

            evt.StopPropagation();
        }

        private void UpdatePan()
        {
            mapContainer.style.translate = new Translate(
                new Length(mapOffset.x, LengthUnit.Pixel),
                new Length(mapOffset.y, LengthUnit.Pixel)
            );
        }

        public void CenterOnPlayer()
        {
            mapOffset = Vector2.zero;
            UpdatePan();
        }

        public void ShowLocation(string locationName, bool show = true)
        {
            if (locationIcons.ContainsKey(locationName))
            {
                locationIcons[locationName].style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        public void HighlightLocation(string locationName, bool highlight = true)
        {
            if (locationIcons.ContainsKey(locationName))
            {
                var icon = locationIcons[locationName];
                if (highlight)
                {
                    icon.AddToClassList("tits-location-highlighted");
                }
                else
                {
                    icon.RemoveFromClassList("tits-location-highlighted");
                }
            }
        }
    }

    // Location type enumeration for map markers
    public enum LocationType
    {
        Town,
        Shop,
        Farm,
        Library,
        Danger,
        Capital,
        Quest,
        Friend
    }

    // Data structure for location information
    public class LocationData
    {
        public string Name { get; set; }
        public Vector2 Position { get; set; }
        public LocationType Type { get; set; }
        public string Description { get; set; }
        public bool IsDiscovered { get; set; }
        public bool IsVisited { get; set; }
    }

    public static class TiTSMinimapExtensions
    {
        public static TiTSMinimap AddTiTSMinimap(this VisualElement parent)
        {
            var minimap = new TiTSMinimap();
            parent.Add(minimap);
            return minimap;
        }
    }
}