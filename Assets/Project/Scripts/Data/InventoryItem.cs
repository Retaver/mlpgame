using System;
using UnityEngine;
using MyGameNamespace;

[Serializable]
public class InventoryItem
{
    public string id;
    public string name;
    public string description;
    public int quantity;
    public string iconPath;
    public bool isUsable;
    public string effectId; // Reference key to ItemEffect ScriptableObject in Resources/ItemEffects
    public ItemCategory category = ItemCategory.Miscellaneous;

    public InventoryItem() { }

    public InventoryItem(string id, string name, string description, string iconPath, bool isUsable, string effectId, ItemCategory category)
    {
        this.id = id;
        this.name = name;
        this.description = description;
        this.iconPath = iconPath;
        this.isUsable = isUsable;
        this.effectId = effectId;
        this.category = category;
        this.quantity = 1;
    }

    
    // Back-compat overload (no category provided) defaults to Miscellaneous
    public InventoryItem(string id, string name, string description, string iconPath, bool isUsable, string effectId)
        : this(id, name, description, iconPath, isUsable, effectId, ItemCategory.Miscellaneous) { }

    // Minimal overload (no effectId, no category)
    public InventoryItem(string id, string name, string description, string iconPath, bool isUsable)
        : this(id, name, description, iconPath, isUsable, null, ItemCategory.Miscellaneous) { }
public Sprite GetIcon()
    {
        return !string.IsNullOrWhiteSpace(iconPath) ? Resources.Load<Sprite>(iconPath) : null;
    }

    private ItemEffect GetEffect()
    {
        if (string.IsNullOrWhiteSpace(effectId)) return null;
        // ItemEffects are ScriptableObjects under Resources/ItemEffects/{effectId}
        return Resources.Load<ItemEffect>($"ItemEffects/{effectId}");
    }

    public bool Use(PlayerCharacter player)
    {
        if (!isUsable || player == null) return false;
        var effect = GetEffect();
        if (effect != null)
        {
            if (effect.CanUse(player))
            {
                effect.Apply(player);
                return true;
            }
            return false;
        }
        // No effect asset; consider it consumable with no side-effect (still succeed)
        return true;
    }

    public bool CanUse(PlayerCharacter player)
    {
        var effect = GetEffect();
        return effect == null ? true : effect.CanUse(player);
    }

    public InventoryItem Clone()
    {
        return new InventoryItem(id, name, description, iconPath, isUsable, effectId, category)
        {
            quantity = this.quantity
        };
    }
}
