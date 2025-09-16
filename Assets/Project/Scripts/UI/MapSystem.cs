using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace MyGameNamespace
{
    /// <summary>
    /// Interactive map system for Unity 6.2 using UI Toolkit
    /// Tile-based grid system with MLP-themed locations
    /// </summary>
    public class MapSystem : MonoBehaviour
    {
        [Header("Map Configuration")]
        [SerializeField] private int mapWidth = 10;
        [SerializeField] private int mapHeight = 8;

        [Header("UI References")]
        [SerializeField] private UIDocument uiDocument;
        private VisualElement mapModal;
        private VisualElement mapModalContainer;
        private VisualElement mapContainer;
        private Label locationNameLabel;
        private Label locationDescriptionLabel;
        private VisualElement movementButtons;
        private Button closeMapButton;

        // Map data
        private MapLocation[,] mapGrid;
        private Vector2Int playerPosition = new Vector2Int(5, 4); // Default to Ponyville
        private HashSet<Vector2Int> discoveredLocations = new HashSet<Vector2Int>();

        // UI Elements
        private VisualElement[,] tileElements;

        private void Awake()
        {
            InitializeMapData();
        }

        private void OnEnable()
        {
            if (uiDocument == null)
            {
                uiDocument = GetComponent<UIDocument>();
            }

            if (uiDocument != null)
            {
                var root = uiDocument.rootVisualElement;
                InitializeUI(root);
                LoadMapState();
                UpdateMapDisplay();
            }
        }

        private void OnDisable()
        {
            SaveMapState();
        }

        /// <summary>
        /// Initialize the MLP-themed map data
        /// </summary>
        private void InitializeMapData()
        {
            // Ensure map dimensions are valid
            if (mapWidth <= 0) mapWidth = 10;
            if (mapHeight <= 0) mapHeight = 8;

            Debug.Log($"[MapSystem] Initializing map with dimensions: {mapWidth}x{mapHeight}");

            // Initialize arrays
            mapGrid = new MapLocation[mapWidth, mapHeight];
            tileElements = new VisualElement[mapWidth, mapHeight];

            // Initialize with default empty locations
            // Use try/catch around the allocation loop so we can log problematic indices if an IndexOutOfRangeException happens
            int initX = -1, initY = -1;
            try
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    for (int y = 0; y < mapHeight; y++)
                    {
                        initX = x; initY = y;
                        mapGrid[x, y] = new MapLocation
                        {
                            Position = new Vector2Int(x, y),
                            Name = "Unknown Area",
                            Description = "An unexplored region of Equestria.",
                            LocationType = LocationType.Empty,
                            IsAccessible = false
                        };
                    }
                }
            }
            catch (IndexOutOfRangeException ex)
            {
                int g0 = mapGrid == null ? -1 : mapGrid.GetLength(0);
                int g1 = mapGrid == null ? -1 : mapGrid.GetLength(1);
                Debug.LogError($"[MapSystem] IndexOutOfRange in InitializeMapData at x={initX}, y={initY} -- mapWidth={mapWidth}, mapHeight={mapHeight}, mapGridSize=({g0},{g1})\nException: {ex}");

                // Attempt a safe recovery: re-create arrays with sensible defaults and continue
                int safeW = Math.Max(1, Math.Min(mapWidth, 10));
                int safeH = Math.Max(1, Math.Min(mapHeight, 8));
                Debug.LogWarning($"[MapSystem] Reinitializing mapGrid and tileElements to safe size {safeW}x{safeH}");
                mapWidth = safeW;
                mapHeight = safeH;
                mapGrid = new MapLocation[mapWidth, mapHeight];
                tileElements = new VisualElement[mapWidth, mapHeight];

                for (int x = 0; x < mapWidth; x++)
                {
                    for (int y = 0; y < mapHeight; y++)
                    {
                        mapGrid[x, y] = new MapLocation
                        {
                            Position = new Vector2Int(x, y),
                            Name = "Unknown Area",
                            Description = "An unexplored region of Equestria.",
                            LocationType = LocationType.Empty,
                            IsAccessible = false
                        };
                    }
                }
            }

            Debug.Log($"[MapSystem] Map grid initialized with {mapWidth * mapHeight} locations");

            // Set up MLP-themed locations
            SetupMLPMapLocations();

            // Ensure player position is within bounds
            if (playerPosition.x < 0 || playerPosition.x >= mapWidth ||
                playerPosition.y < 0 || playerPosition.y >= mapHeight)
            {
                playerPosition = new Vector2Int(5, 4); // Default to Ponyville
                Debug.Log($"[MapSystem] Player position reset to {playerPosition}");
            }

            // Ensure discoveredLocations is initialized
            if (discoveredLocations == null)
            {
                discoveredLocations = new HashSet<Vector2Int>();
                Debug.Log("[MapSystem] discoveredLocations was null, reinitialized");
            }

            discoveredLocations.Add(playerPosition);
            if (playerPosition.x >= 0 && playerPosition.x < mapWidth &&
                playerPosition.y >= 0 && playerPosition.y < mapHeight)
            {
                mapGrid[playerPosition.x, playerPosition.y].IsAccessible = true;
                Debug.Log($"[MapSystem] Player position {playerPosition} marked as accessible");
            }

            Debug.Log($"[MapSystem] Map initialization complete");
        }

        /// <summary>
        /// Set up the MLP-themed map with Everfree Isekai story locations
        /// </summary>
        private void SetupMLPMapLocations()
        {
            // Everfree Forest (starting area - player starts here)
            SetLocation(5, 4, "Everfree Forest", "The mysterious forest where your adventure begins. Home to timberwolves and ancient magic.", LocationType.Forest);

            // Moon-Mirror Lake (transformation scene)
            SetLocation(4, 5, "Moon-Mirror Lake", "A serene lake that reflects the night sky. This is where you first transformed.", LocationType.Lake);

            // Ancient Castle Ruins (magical circle)
            SetLocation(3, 3, "Ancient Castle Ruins", "Ancient ruins with a magical circle. The moon's blessing awaits here.", LocationType.Ruins);

            // Kirin Village (Emberglow's home)
            SetLocation(2, 6, "Kirin Village", "Hidden village of the Kirin people. Emberglow's peaceful home.", LocationType.Kirin);

            // Path to Canterlot (exit from Everfree)
            SetLocation(6, 2, "Path to Canterlot", "The mountain path leading toward the majestic city of Canterlot.", LocationType.Path);

            // Timberwolf Den (dangerous area)
            SetLocation(7, 5, "Timberwolf Den", "A dangerous area where timberwolves gather. Best avoided at night.", LocationType.Forest);

            // Zap Apple Grove (mystical location)
            SetLocation(1, 4, "Zap Apple Grove", "A grove where zap apples grow during the harvest moon.", LocationType.Farm);

            // Connect locations with paths
            CreateMapConnections();
        }

        /// <summary>
        /// Helper method to set up a location
        /// </summary>
        private void SetLocation(int x, int y, string name, string description, LocationType type)
        {
            if (mapGrid == null)
            {
                Debug.LogError("[MapSystem] mapGrid is null in SetLocation!");
                return;
            }

            if (x >= 0 && x < mapWidth && y >= 0 && y < mapHeight &&
                x < mapGrid.GetLength(0) && y < mapGrid.GetLength(1))
            {
                mapGrid[x, y] = new MapLocation
                {
                    Position = new Vector2Int(x, y),
                    Name = name,
                    Description = description,
                    LocationType = type,
                    IsAccessible = true
                };
                Debug.Log($"[MapSystem] Set location at ({x},{y}): {name}");
            }
            else
            {
                Debug.LogWarning($"[MapSystem] Invalid location coordinates: ({x},{y}) for map size {mapWidth}x{mapHeight}");
            }
        }

        /// <summary>
        /// Create connections between accessible locations
        /// </summary>
        private void CreateMapConnections()
        {
            if (mapGrid == null)
            {
                Debug.LogError("[MapSystem] mapGrid is null in CreateMapConnections!");
                return;
            }

            // Mark areas around towns as accessible paths
            int[] townX = { 5, 7, 3, 6, 8, 4, 5, 6, 4 };
            int[] townY = { 4, 6, 3, 7, 2, 5, 5, 4, 4 };

            for (int i = 0; i < townX.Length; i++)
            {
                int x = townX[i];
                int y = townY[i];

                // Mark adjacent areas as paths
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        int nx = x + dx;
                        int ny = y + dy;

                        if (nx >= 0 && nx < mapWidth && ny >= 0 && ny < mapHeight)
                        {
                            // Double-check bounds and array validity
                            if (mapGrid != null && nx < mapGrid.GetLength(0) && ny < mapGrid.GetLength(1))
                            {
                                if (mapGrid[nx, ny].LocationType == LocationType.Empty)
                                {
                                    mapGrid[nx, ny].LocationType = LocationType.Path;
                                    mapGrid[nx, ny].IsAccessible = true;
                                }
                            }
                        }
                    }
                }
            }

            Debug.Log($"[MapSystem] Created connections for {townX.Length} town locations");
        }

        /// <summary>
        /// Initialize the UI elements
        /// </summary>
        private void InitializeUI(VisualElement root)
        {
            // Look for embedded map grid in sidebar instead of modal
            mapContainer = root.Q<VisualElement>("map-grid");

            if (mapContainer == null)
            {
                Debug.LogError("[MapSystem] Map container not found in UXML!");
                return;
            }

            // Create the map grid
            CreateMapGrid();

            // Set up map navigation buttons in sidebar
            SetupSidebarNavigation(root);
        }

        /// <summary>
        /// Set up sidebar navigation buttons
        /// </summary>
        private void SetupSidebarNavigation(VisualElement root)
        {
            // Set up directional buttons in sidebar
            var northBtn = root.Q<Button>("map-north");
            var southBtn = root.Q<Button>("map-south");
            var eastBtn = root.Q<Button>("map-east");
            var westBtn = root.Q<Button>("map-west");

            if (northBtn != null) northBtn.clicked += () => MovePlayer(0, 1);
            if (southBtn != null) southBtn.clicked += () => MovePlayer(0, -1);
            if (eastBtn != null) eastBtn.clicked += () => MovePlayer(1, 0);
            if (westBtn != null) westBtn.clicked += () => MovePlayer(-1, 0);
        }

        /// <summary>
        /// Create a directional movement button
        /// </summary>
        private void CreateDirectionButton(string direction, string symbol, Action onClick)
        {
            var button = new Button();
            button.text = $"{direction}\n{symbol}";
            button.AddToClassList("direction-button");
            button.AddToClassList($"direction-{direction.ToLower()}");
            button.clicked += onClick;
            movementButtons.Add(button);
        }

        /// <summary>
        /// Create the visual map grid
        /// </summary>
        private void CreateMapGrid()
        {
            if (mapContainer == null) return;

            mapContainer.Clear();

            for (int y = mapHeight - 1; y >= 0; y--) // Start from top
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    var tile = CreateMapTile(x, y);
                    mapContainer.Add(tile);
                    tileElements[x, y] = tile;
                }
            }
        }

        /// <summary>
        /// Create a single map tile
        /// </summary>
        private VisualElement CreateMapTile(int x, int y)
        {
            var tile = new VisualElement();
            tile.AddToClassList("map-tile");

            var location = mapGrid[x, y];

            // Set tile type class
            tile.AddToClassList($"tile-{location.LocationType.ToString().ToLower()}");

            // Mark if discovered
            if (discoveredLocations.Contains(new Vector2Int(x, y)))
            {
                tile.AddToClassList("discovered");
            }
            else
            {
                tile.AddToClassList("invalid");
            }

            // Mark player position
            if (x == playerPosition.x && y == playerPosition.y)
            {
                tile.AddToClassList("active");
                var playerIcon = new Label("●");
                playerIcon.AddToClassList("player-icon");
                tile.Add(playerIcon);
            }

            // Add simple location indicator for discovered locations
            if (discoveredLocations.Contains(new Vector2Int(x, y)) && location.LocationType != LocationType.Empty && location.LocationType != LocationType.Path)
            {
                var locationIcon = new Label(GetLocationIcon(location.LocationType));
                locationIcon.AddToClassList("location-icon");
                tile.Add(locationIcon);
            }

            // Make tile clickable for movement
            tile.RegisterCallback<ClickEvent>(evt => OnTileClicked(x, y));

            return tile;
        }

        /// <summary>
        /// Get icon for location type
        /// </summary>
        private string GetLocationIcon(LocationType type)
        {
            switch (type)
            {
                case LocationType.Town:
                case LocationType.City:
                case LocationType.Capital:
                    return "🏘️";
                case LocationType.Farm:
                    return "🌾";
                case LocationType.Forest:
                    return "🌲";
                case LocationType.Home:
                    return "🏠";
                case LocationType.Shop:
                    return "🏪";
                case LocationType.Library:
                    return "📚";
                case LocationType.Castle:
                    return "🏰";
                case LocationType.Mountain:
                    return "⛰️";
                case LocationType.River:
                    return "🌊";
                case LocationType.Lake:
                    return "🏞️";
                case LocationType.Ruins:
                    return "🏛️";
                case LocationType.Kirin:
                    return "🐉";
                default:
                    return "📍";
            }
        }

        /// <summary>
        /// Handle tile click events
        /// </summary>
        private void OnTileClicked(int x, int y)
        {
            var location = mapGrid[x, y];

            // If it's an adjacent location, allow movement
            if (IsAdjacentToPlayer(x, y) && location.IsAccessible)
            {
                MovePlayer(x - playerPosition.x, y - playerPosition.y);
            }
            // If it's the current location, maybe show info (could be expanded later)
            else if (x == playerPosition.x && y == playerPosition.y)
            {
                Debug.Log($"[MapSystem] Current location: {location.Name}");
            }
        }

        /// <summary>
        /// Check if a location is adjacent to the player
        /// </summary>
        private bool IsAdjacentToPlayer(int x, int y)
        {
            int dx = Mathf.Abs(x - playerPosition.x);
            int dy = Mathf.Abs(y - playerPosition.y);
            return (dx == 1 && dy == 0) || (dx == 0 && dy == 1);
        }

        /// <summary>
        /// Move the player to a new location
        /// </summary>
        private void MovePlayer(int dx, int dy)
        {
            Vector2Int newPos = new Vector2Int(playerPosition.x + dx, playerPosition.y + dy);

            // Check bounds
            if (newPos.x < 0 || newPos.x >= mapWidth || newPos.y < 0 || newPos.y >= mapHeight)
                return;

            // Check if location is accessible
            if (!mapGrid[newPos.x, newPos.y].IsAccessible)
                return;

            // Move player
            playerPosition = newPos;
            discoveredLocations.Add(playerPosition);

            // Update display
            UpdateMapDisplay();
            UpdateLocationInfo();

            Debug.Log($"[MapSystem] Player moved to {mapGrid[playerPosition.x, playerPosition.y].Name}");
        }

        /// <summary>
        /// Update the entire map display
        /// </summary>
        private void UpdateMapDisplay()
        {
            if (mapContainer == null) return;

            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    if (tileElements[x, y] != null)
                    {
                        // Remove old tile and create new one
                        int index = mapContainer.IndexOf(tileElements[x, y]);
                        if (index >= 0)
                        {
                            mapContainer.RemoveAt(index);
                            var newTile = CreateMapTile(x, y);
                            mapContainer.Insert(index, newTile);
                            tileElements[x, y] = newTile;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Update the location information display (removed for embedded layout)
        /// </summary>
        private void UpdateLocationInfo()
        {
            // Location info is now handled by the main game UI
            // This method is kept for compatibility but doesn't update UI elements
        }

        /// <summary>
        /// Get the current player location
        /// </summary>
        public MapLocation GetCurrentLocation()
        {
            return mapGrid[playerPosition.x, playerPosition.y];
        }

        /// <summary>
        /// Get player position
        /// </summary>
        public Vector2Int GetPlayerPosition()
        {
            return playerPosition;
        }

        /// <summary>
        /// Set player position (for loading saved games)
        /// </summary>
        public void SetPlayerPosition(Vector2Int position)
        {
            if (position.x >= 0 && position.x < mapWidth && position.y >= 0 && position.y < mapHeight)
            {
                playerPosition = position;
                discoveredLocations.Add(playerPosition);
                UpdateMapDisplay();
                UpdateLocationInfo();
            }
        }

        /// <summary>
        /// Save map state to PlayerPrefs
        /// </summary>
        private void SaveMapState()
        {
            PlayerPrefs.SetInt("MapSystem_PlayerX", playerPosition.x);
            PlayerPrefs.SetInt("MapSystem_PlayerY", playerPosition.y);

            // Save discovered locations as a comma-separated string
            string discoveredString = "";
            foreach (var pos in discoveredLocations)
            {
                if (discoveredString != "") discoveredString += ",";
                discoveredString += $"{pos.x},{pos.y}";
            }
            PlayerPrefs.SetString("MapSystem_DiscoveredLocations", discoveredString);
            PlayerPrefs.Save();

            Debug.Log($"[MapSystem] Saved map state - Player at {playerPosition}, {discoveredLocations.Count} locations discovered");
        }

        /// <summary>
        /// Load map state from PlayerPrefs
        /// </summary>
        private void LoadMapState()
        {
            // Load player position
            int playerX = PlayerPrefs.GetInt("MapSystem_PlayerX", 5); // Default to Ponyville
            int playerY = PlayerPrefs.GetInt("MapSystem_PlayerY", 4);
            playerPosition = new Vector2Int(playerX, playerY);

            // Load discovered locations
            string discoveredString = PlayerPrefs.GetString("MapSystem_DiscoveredLocations", "");
            if (!string.IsNullOrEmpty(discoveredString))
            {
                discoveredLocations.Clear();
                string[] locationStrings = discoveredString.Split(',');
                for (int i = 0; i < locationStrings.Length; i += 2)
                {
                    if (i + 1 < locationStrings.Length)
                    {
                        if (int.TryParse(locationStrings[i], out int x) && int.TryParse(locationStrings[i + 1], out int y))
                        {
                            discoveredLocations.Add(new Vector2Int(x, y));
                        }
                    }
                }
            }

            // Ensure current player position is discovered
            discoveredLocations.Add(playerPosition);

            Debug.Log($"[MapSystem] Loaded map state - Player at {playerPosition}, {discoveredLocations.Count} locations discovered");
        }

        /// <summary>
        /// Represents a location on the map
        /// </summary>
        [Serializable]
        public class MapLocation
        {
            public Vector2Int Position;
            public string Name;
            public string Description;
            public LocationType LocationType;
            public bool IsAccessible;
            public bool HasSpecialEvent;
        }

        /// <summary>
        /// Types of locations on the map
        /// </summary>
        public enum LocationType
        {
            Empty,
            Path,
            Town,
            City,
            Capital,
            Farm,
            Forest,
            Home,
            Shop,
            Library,
            Castle,
            Mountain,
            River,
            Lake,
            Ruins,
            Kirin
        }
    }
}