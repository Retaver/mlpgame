using UnityEngine;
using UnityEngine.UIElements;
using MyGameNamespace.World;

namespace MyGameNamespace
{
    /// <summary>
    /// Bootstrap component for setting up the Equestria map system
    /// Attach this to a GameObject in your scene to initialize the map
    /// </summary>
    public class EquestriaMapBootstrap : MonoBehaviour
    {
        [Header("Map Configuration")]
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private int mapWidth = 12;
        [SerializeField] private int mapHeight = 10;
        [SerializeField] private Vector2Int startingPosition = new Vector2Int(5, 4);

        [Header("Location Data")]
        [SerializeField] private LocationData ponyvilleData;
        [SerializeField] private LocationData canterlotData;
        [SerializeField] private LocationData everfreeForestData;
        [SerializeField] private LocationData sweetAppleAcresData;
        [SerializeField] private LocationData cloudsdaleData;

        private MapSystem mapSystem;
        private MapService mapService;

        private void Awake()
        {
            InitializeMapSystem();
        }

        private void InitializeMapSystem()
        {
            // Create MapSystem component
            mapSystem = gameObject.AddComponent<MapSystem>();

            // Configure MapSystem
            var mapSystemType = typeof(MapSystem);
            var widthField = mapSystemType.GetField("mapWidth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var heightField = mapSystemType.GetField("mapHeight", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var uiDocumentField = mapSystemType.GetField("uiDocument", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (widthField != null) widthField.SetValue(mapSystem, mapWidth);
            if (heightField != null) heightField.SetValue(mapSystem, mapHeight);
            if (uiDocumentField != null) uiDocumentField.SetValue(mapSystem, uiDocument);

            // Create MapService
            mapService = new MapService(mapWidth, mapHeight, startingPosition, ponyvilleData);

            // Set up location data
            SetupEquestriaLocations();

            Debug.Log("Equestria Map System initialized!");
        }

        private void SetupEquestriaLocations()
        {
            if (mapService == null) return;

            // Ponyville (starting location - already set)
            mapService.SetLocation(new Vector2Int(5, 4), ponyvilleData);

            // Canterlot (north of Ponyville)
            if (canterlotData != null)
                mapService.SetLocation(new Vector2Int(5, 2), canterlotData);

            // Everfree Forest (east of Ponyville)
            if (everfreeForestData != null)
                mapService.SetLocation(new Vector2Int(8, 4), everfreeForestData);

            // Sweet Apple Acres (south of Ponyville)
            if (sweetAppleAcresData != null)
                mapService.SetLocation(new Vector2Int(5, 6), sweetAppleAcresData);

            // Cloudsdale (west of Ponyville)
            if (cloudsdaleData != null)
                mapService.SetLocation(new Vector2Int(2, 4), cloudsdaleData);
        }

        private void OnDestroy()
        {
            // Clean up if needed
        }
    }
}