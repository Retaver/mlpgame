using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using MyGameNamespace;

namespace MyGameNamespace
{
    /// <summary>
    /// Save/Load system for MLP game - inspired by TiTS SharedData approach
    /// Handles game state persistence with JSON serialization
    /// </summary>
    public static class SaveLoadSystem
    {
        private const string SAVE_FOLDER = "Saves";
        private const string SAVE_EXTENSION = ".mlp";
        private const string QUICK_SAVE_NAME = "QuickSave";
        private const string SETTINGS_FILE = "GameSettings";

        /// <summary>
        /// Save game data to a file
        /// </summary>
        public static bool SaveGame(string saveName = null)
        {
            if (string.IsNullOrEmpty(saveName))
                saveName = QUICK_SAVE_NAME;

            try
            {
                // Create save directory if it doesn't exist
                string savePath = GetSavePath(saveName);
                string directory = Path.GetDirectoryName(savePath);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                // Collect game data
                GameSaveData saveData = CollectGameData();

                // Serialize to JSON
                string jsonData = JsonUtility.ToJson(saveData, true);

                // Write to file
                File.WriteAllText(savePath, jsonData);

                Debug.Log($"[SaveLoadSystem] Game saved successfully: {savePath}");
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SaveLoadSystem] Failed to save game: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Load game data from a file
        /// </summary>
        public static bool LoadGame(string saveName = null)
        {
            if (string.IsNullOrEmpty(saveName))
                saveName = QUICK_SAVE_NAME;

            try
            {
                string savePath = GetSavePath(saveName);

                if (!File.Exists(savePath))
                {
                    Debug.LogWarning($"[SaveLoadSystem] Save file not found: {savePath}");
                    return false;
                }

                // Read and deserialize
                string jsonData = File.ReadAllText(savePath);
                GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(jsonData);

                // Apply loaded data
                ApplyGameData(saveData);

                Debug.Log($"[SaveLoadSystem] Game loaded successfully: {savePath}");
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SaveLoadSystem] Failed to load game: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Check if a save file exists
        /// </summary>
        public static bool SaveExists(string saveName = null)
        {
            if (string.IsNullOrEmpty(saveName))
                saveName = QUICK_SAVE_NAME;

            string savePath = GetSavePath(saveName);
            return File.Exists(savePath);
        }

        /// <summary>
        /// Get list of all save files
        /// </summary>
        public static string[] GetSaveFiles()
        {
            string saveDirectory = GetSaveDirectory();
            if (!Directory.Exists(saveDirectory))
                return new string[0];

            string[] files = Directory.GetFiles(saveDirectory, $"*{SAVE_EXTENSION}");
            for (int i = 0; i < files.Length; i++)
            {
                files[i] = Path.GetFileNameWithoutExtension(files[i]);
            }
            return files;
        }

        /// <summary>
        /// Delete a save file
        /// </summary>
        public static bool DeleteSave(string saveName)
        {
            try
            {
                string savePath = GetSavePath(saveName);
                if (File.Exists(savePath))
                {
                    File.Delete(savePath);
                    Debug.Log($"[SaveLoadSystem] Save file deleted: {saveName}");
                    return true;
                }
                return false;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SaveLoadSystem] Failed to delete save: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Save game settings
        /// </summary>
        public static bool SaveSettings(GameSettings settings)
        {
            try
            {
                string settingsPath = GetSettingsPath();
                string directory = Path.GetDirectoryName(settingsPath);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                string jsonData = JsonUtility.ToJson(settings, true);
                File.WriteAllText(settingsPath, jsonData);

                Debug.Log("[SaveLoadSystem] Settings saved successfully");
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SaveLoadSystem] Failed to save settings: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Load game settings
        /// </summary>
        public static GameSettings LoadSettings()
        {
            try
            {
                string settingsPath = GetSettingsPath();
                if (!File.Exists(settingsPath))
                {
                    Debug.Log("[SaveLoadSystem] Settings file not found, using defaults");
                    return new GameSettings();
                }

                string jsonData = File.ReadAllText(settingsPath);
                GameSettings settings = JsonUtility.FromJson<GameSettings>(jsonData);

                Debug.Log("[SaveLoadSystem] Settings loaded successfully");
                return settings;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SaveLoadSystem] Failed to load settings: {ex.Message}");
                return new GameSettings();
            }
        }

        private static string GetSaveDirectory()
        {
            return Path.Combine(Application.persistentDataPath, SAVE_FOLDER);
        }

        private static string GetSavePath(string saveName)
        {
            return Path.Combine(GetSaveDirectory(), saveName + SAVE_EXTENSION);
        }

        private static string GetSettingsPath()
        {
            return Path.Combine(Application.persistentDataPath, SETTINGS_FILE + SAVE_EXTENSION);
        }

        private static GameSaveData CollectGameData()
        {
            var saveData = new GameSaveData
            {
                saveTime = System.DateTime.Now.ToString(),
                gameVersion = Application.version
            };

            // Collect character data
            var characterSystem = Object.FindFirstObjectByType<CharacterSystem>();
            if (characterSystem != null)
            {
                var player = characterSystem.GetPlayerCharacter();
                if (player != null)
                {
                    saveData.playerData = new PlayerSaveData
                    {
                        name = player.name,
                        race = player.race,
                        gender = player.gender,
                        level = player.level,
                        experience = player.experience,
                        health = player.health,
                        maxHealth = player.maxHealth,
                        energy = player.energy,
                        maxEnergy = player.maxEnergy,
                        bits = player.bits,
                        skillPoints = player.skillPoints
                    };
                }
            }

            // Collect inventory data
            var inventorySystem = Object.FindFirstObjectByType<InventorySystem>();
            if (inventorySystem != null)
            {
                saveData.inventoryData = new InventorySaveData();
                var items = inventorySystem.GetAllItems();
                if (items != null)
                {
                    saveData.inventoryData.items = new List<InventoryItemSaveData>();
                    foreach (var item in items)
                    {
                        if (item != null)
                        {
                            saveData.inventoryData.items.Add(new InventoryItemSaveData
                            {
                                id = item.id,
                                quantity = item.quantity
                            });
                        }
                    }
                }
            }

            // Collect map/location data
            var mapSystem = Object.FindFirstObjectByType<MapSystem>();
            if (mapSystem != null)
            {
                saveData.mapData = new MapSaveData
                {
                    currentLocation = "Ponyville", // Placeholder
                    discoveredLocations = new List<string> { "Ponyville" }
                };
            }

            return saveData;
        }

        private static void ApplyGameData(GameSaveData saveData)
        {
            // Apply character data
            var characterSystem = Object.FindFirstObjectByType<CharacterSystem>();
            if (characterSystem != null && saveData.playerData != null)
            {
                var player = characterSystem.GetPlayerCharacter();
                if (player != null)
                {
                    player.name = saveData.playerData.name;
                    player.race = saveData.playerData.race;
                    player.gender = saveData.playerData.gender;
                    player.level = saveData.playerData.level;
                    player.experience = saveData.playerData.experience;
                    player.health = saveData.playerData.health;
                    player.maxHealth = saveData.playerData.maxHealth;
                    player.energy = saveData.playerData.energy;
                    player.maxEnergy = saveData.playerData.maxEnergy;
                    player.bits = saveData.playerData.bits;
                    player.skillPoints = saveData.playerData.skillPoints;
                }
            }

            // Apply inventory data
            var inventorySystem = Object.FindFirstObjectByType<InventorySystem>();
            if (inventorySystem != null && saveData.inventoryData != null && saveData.inventoryData.items != null)
            {
                foreach (var itemData in saveData.inventoryData.items)
                {
                    var item = ItemDatabase.GetItemById(itemData.id);
                    if (item != null)
                    {
                        inventorySystem.AddItem(item, itemData.quantity);
                    }
                }
            }

            // Notify systems of data changes
            GameEventSystem.Instance?.RaisePlayerStatsChanged();
            GameEventSystem.Instance?.RaiseInventoryChanged();
        }
    }

    // Save data structures
    [System.Serializable]
    public class GameSaveData
    {
        public string saveTime;
        public string gameVersion;
        public PlayerSaveData playerData;
        public InventorySaveData inventoryData;
        public MapSaveData mapData;
    }

    [System.Serializable]
    public class PlayerSaveData
    {
        public string name;
        public string race;
        public string gender;
        public int level;
        public int experience;
        public float health;
        public float maxHealth;
        public float energy;
        public float maxEnergy;
        public int bits;
        public int skillPoints;
    }

    [System.Serializable]
    public class InventorySaveData
    {
        public List<InventoryItemSaveData> items;
    }

    [System.Serializable]
    public class InventoryItemSaveData
    {
        public string id;
        public int quantity;
    }

    [System.Serializable]
    public class MapSaveData
    {
        public string currentLocation;
        public List<string> discoveredLocations;
    }

    [System.Serializable]
    public class GameSettings
    {
        public float masterVolume = 1.0f;
        public float musicVolume = 0.8f;
        public float sfxVolume = 0.9f;
        public bool showMinimap = true;
        public bool showLocationInfo = true;
        public bool autoSave = true;
        public int autoSaveInterval = 300; // seconds
    }
}