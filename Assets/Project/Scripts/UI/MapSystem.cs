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
        [SerializeField] private float tileSize = 40f;

        [Header("UI References")]
        [SerializeField] private UIDocument uiDocument;
        private VisualElement mapContainer;
        private Label locationNameLabel;
        private Label locationDescriptionLabel;
        private VisualElement movementButtons;

        // Map data
        private MapLocation[,] mapGrid;
        private Vector2Int playerPosition;
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
            mapGrid = new MapLocation[mapWidth, mapHeight];
            tileElements = new VisualElement[mapWidth, mapHeight];

            // Initialize with default empty locations
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

            // Set up MLP-themed locations
            SetupMLPMapLocations();

            // Set starting position (Ponyville)
            playerPosition = new Vector2Int(5, 4);
            discoveredLocations.Add(playerPosition);
            mapGrid[playerPosition.x, playerPosition.y].IsAccessible = true;
        }

        /// <summary>
        /// Set up the MLP-themed map with various pony locations
        /// </summary>
        private void SetupMLPMapLocations()
        {
            // Ponyville (starting area)
            SetLocation(5, 4, "Ponyville", "The heart of Equestria! A bustling town filled with friendly ponies.", LocationType.Town);

            // Canterlot
            SetLocation(7, 6, "Canterlot", "The majestic capital city of Equestria, home to Princess Celestia.", LocationType.Capital);

            // Sweet Apple Acres
            SetLocation(3, 3, "Sweet Apple Acres", "Applejack's family farm. The best apples in all of Equestria!", LocationType.Farm);

            // Everfree Forest
            SetLocation(2, 5, "Everfree Forest", "A mysterious and dangerous forest. Zecora's hut is nearby.", LocationType.Forest);

            // Cloudsdale
            SetLocation(6, 7, "Cloudsdale", "The floating city of pegasi. Rainbow Dash's hometown!", LocationType.City);

            // Crystal Empire
            SetLocation(8, 2, "Crystal Empire", "A magical empire ruled by Princess Cadance and Shining Armor.", LocationType.Capital);

            // Fluttershy's Cottage
            SetLocation(4, 5, "Fluttershy's Cottage", "A cozy cottage surrounded by animal friends.", LocationType.Home);

            // Rarity's Boutique
            SetLocation(5, 5, "Carousel Boutique", "Rarity's fashion boutique in Ponyville.", LocationType.Shop);

            // Sugarcube Corner
            SetLocation(6, 4, "Sugarcube Corner", "Pinkie Pie's bakery. Cupcakes and parties!", LocationType.Shop);

            // Golden Oak Library
            SetLocation(4, 4, "Golden Oak Library", "Twilight Sparkle's library. Knowledge awaits!", LocationType.Library);

            // Whitetail Woods
            SetLocation(1, 4, "Whitetail Woods", "A peaceful forest area perfect for picnics.", LocationType.Forest);

            // Connect locations with paths
            CreateMapConnections();
        }

        /// <summary>
        /// Helper method to set up a location
        /// </summary>
        private void SetLocation(int x, int y, string name, string description, LocationType type)
        {
            if (x >= 0 && x < mapWidth && y >= 0 && y < mapHeight)
            {
                mapGrid[x, y] = new MapLocation
                {
                    Position = new Vector2Int(x, y),
                    Name = name,
                    Description = description,
                    LocationType = type,
                    IsAccessible = true
                };
            }
        }

        /// <summary>
        /// Create connections between accessible locations
        /// </summary>
        private void CreateMapConnections()
        {
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

        /// <summary>
        /// Initialize the UI elements
        /// </summary>
        private void InitializeUI(VisualElement root)
        {
            mapContainer = root.Q<VisualElement>("map-grid");
            locationNameLabel = root.Q<Label>("location-name");
            locationDescriptionLabel = root.Q<Label>("location-description");
            movementButtons = root.Q<VisualElement>("movement-buttons");

            if (mapContainer == null)
            {
                Debug.LogError("[MapSystem] Map container not found in UXML!");
                return;
            }

            // Create movement buttons if they don't exist
            CreateMovementButtons();

            // Create the map grid
            CreateMapGrid();
        }

        /// <summary>
        /// Create the movement control buttons
        /// </summary>
        private void CreateMovementButtons()
        {
            if (movementButtons == null) return;

            // Clear existing buttons
            movementButtons.Clear();

            // Create directional buttons
            CreateDirectionButton("North", "↑", () => MovePlayer(0, 1));
            CreateDirectionButton("South", "↓", () => MovePlayer(0, -1));
            CreateDirectionButton("East", "→", () => MovePlayer(1, 0));
            CreateDirectionButton("West", "←", () => MovePlayer(-1, 0));
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

            // Mark player position
            if (x == playerPosition.x && y == playerPosition.y)
            {
                tile.AddToClassList("player-location");
                var playerIcon = new VisualElement();
                playerIcon.AddToClassList("player-icon");
                tile.Add(playerIcon);
            }

            // Add location name for discovered locations
            if (discoveredLocations.Contains(new Vector2Int(x, y)) && location.LocationType != LocationType.Empty && location.LocationType != LocationType.Path)
            {
                var nameLabel = new Label(location.Name);
                nameLabel.AddToClassList("tile-name");
                tile.Add(nameLabel);
            }

            // Make tile clickable
            tile.RegisterCallback<ClickEvent>(evt => OnTileClicked(x, y));

            return tile;
        }

        /// <summary>
        /// Handle tile click events
        /// </summary>
        private void OnTileClicked(int x, int y)
        {
            var location = mapGrid[x, y];

            // Update location info display
            if (locationNameLabel != null)
                locationNameLabel.text = location.Name;

            if (locationDescriptionLabel != null)
                locationDescriptionLabel.text = location.Description;

            // If it's an adjacent location, allow movement
            if (IsAdjacentToPlayer(x, y) && location.IsAccessible)
            {
                MovePlayer(x - playerPosition.x, y - playerPosition.y);
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
        /// Update the location information display
        /// </summary>
        private void UpdateLocationInfo()
        {
            var currentLocation = mapGrid[playerPosition.x, playerPosition.y];

            if (locationNameLabel != null)
                locationNameLabel.text = currentLocation.Name;

            if (locationDescriptionLabel != null)
                locationDescriptionLabel.text = currentLocation.Description;
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
        River
    }
}