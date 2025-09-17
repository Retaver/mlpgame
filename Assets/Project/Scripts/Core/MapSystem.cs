using UnityEngine;
using System.Collections.Generic;
using MyGameNamespace;

namespace MyGameNamespace
{
    /// <summary>
    /// Manages the game map system, locations, and navigation
    /// </summary>
    public class MapSystem : MonoBehaviour
    {
        [Header("Map Settings")]
        [SerializeField] private string currentLocation = "Ponyville";
        [SerializeField] private List<string> discoveredLocations = new List<string>();

        // Singleton pattern for easy access
        private static MapSystem instance;
        public static MapSystem Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<MapSystem>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("MapSystem");
                        instance = go.AddComponent<MapSystem>();
                    }
                }
                return instance;
            }
        }

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (instance != this)
            {
                Destroy(gameObject);
                return;
            }

            // Initialize with default locations if empty
            if (discoveredLocations.Count == 0)
            {
                discoveredLocations.Add("Ponyville");
                discoveredLocations.Add("Sweet Apple Acres");
                discoveredLocations.Add("Carousel Boutique");
            }
        }

        /// <summary>
        /// Get the current location name
        /// </summary>
        public string GetCurrentLocation()
        {
            return currentLocation;
        }

        /// <summary>
        /// Set the current location
        /// </summary>
        public void SetCurrentLocation(string location)
        {
            currentLocation = location;
            if (!discoveredLocations.Contains(location))
            {
                discoveredLocations.Add(location);
            }
        }

        /// <summary>
        /// Get all discovered locations
        /// </summary>
        public List<string> GetDiscoveredLocations()
        {
            return new List<string>(discoveredLocations);
        }

        /// <summary>
        /// Check if a location has been discovered
        /// </summary>
        public bool IsLocationDiscovered(string location)
        {
            return discoveredLocations.Contains(location);
        }

        /// <summary>
        /// Discover a new location
        /// </summary>
        public void DiscoverLocation(string location)
        {
            if (!discoveredLocations.Contains(location))
            {
                discoveredLocations.Add(location);
                Debug.Log($"[MapSystem] Discovered new location: {location}");
            }
        }

        /// <summary>
        /// Get location description (placeholder for now)
        /// </summary>
        public string GetLocationDescription(string location)
        {
            // This could be expanded with a location database
            switch (location)
            {
                case "Ponyville":
                    return "A peaceful town where everypony lives in harmony.";
                case "Sweet Apple Acres":
                    return "The Apple family farm, known for its delicious apples.";
                case "Carousel Boutique":
                    return "Rarity's fashion boutique, home of the latest trends.";
                default:
                    return "A mysterious location in Equestria.";
            }
        }
    }
}