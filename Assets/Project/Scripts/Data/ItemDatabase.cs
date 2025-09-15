using System.Collections.Generic;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance { get; private set; }

    [Header("Item Database")]
    [SerializeField] private List<InventoryItem> defaultItems = new();

    private readonly Dictionary<string, InventoryItem> itemsById = new();
    private bool isInitialized = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        if (isInitialized) return;
        itemsById.Clear();

        if (defaultItems != null)
        {
            foreach (var it in defaultItems)
            {
                if (it == null || string.IsNullOrWhiteSpace(it.id)) continue;
                RegisterItem(it);
            }
        }

        // Ensure some sensible defaults exist (only if not provided)
        if (!itemsById.ContainsKey("health_potion"))
        {
            RegisterItem(new InventoryItem(
                id: "health_potion",
                name: "Health Potion",
                description: "Restores a moderate amount of health.",
                iconPath: "Icons/health_potion",
                isUsable: true,
                effectId: "HealthEffect",
                category: ItemCategory.Consumable
            ));
        }
        if (!itemsById.ContainsKey("energy_potion"))
        {
            RegisterItem(new InventoryItem(
                id: "energy_potion",
                name: "Energy Potion",
                description: "Restores a moderate amount of energy.",
                iconPath: "Icons/energy_potion",
                isUsable: true,
                effectId: "EnergyEffect",
                category: ItemCategory.Consumable
            ));
        }

        isInitialized = true;
    }

    public void RegisterItem(InventoryItem item)
    {
        if (item == null || string.IsNullOrWhiteSpace(item.id)) return;
        itemsById[item.id] = item;
    }

    public bool HasItem(string itemId)
    {
        return !string.IsNullOrWhiteSpace(itemId) && itemsById.ContainsKey(itemId);
    }

    public InventoryItem GetItemById(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId)) return null;
        if (itemsById.TryGetValue(itemId, out var it))
        {
            // Return a clone so inventory stacks aren't shared by reference
            return it.Clone();
        }
        return null;
    }

    // --- Static helpers ---
    public static InventoryItem Get(string itemId) => Instance != null ? Instance.GetItemById(itemId) : null;

    public static bool HasItemStatic(string itemId)
    {
        return Instance?.HasItem(itemId) ?? false;
    }

    public static ItemCategory GetItemCategory(string itemId)
    {
        if (Instance == null) return ItemCategory.Miscellaneous;
        var item = Instance.GetItemById(itemId);
        return item != null ? item.category : ItemCategory.Miscellaneous;
    }
}
