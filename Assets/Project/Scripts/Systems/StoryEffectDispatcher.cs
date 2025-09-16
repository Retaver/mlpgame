using UnityEngine;

// Call this from your story runner when applying effects from JSON choices.
public static class StoryEffectDispatcher
{
    // Minimal signature matching your JSON fields
    public static void Dispatch(string type, string target, int value = 0, string stringValue = null)
    {
        if (string.IsNullOrEmpty(type)) return;

        switch (type.ToLowerInvariant())
        {
            // This is the important mapping for your main_story.json
            case "combat":
                if (GameEventSystem.Instance == default)
                {
                    Debug.LogWarning("[StoryEffectDispatcher] GameEventSystem.Instance is null; cannot raise combat.");
                    return;
                }
                if (string.IsNullOrWhiteSpace(target))
                {
                    Debug.LogWarning("[StoryEffectDispatcher] Combat effect missing target id; defaulting to 'Hostile'.");
                    target = "Hostile";
                }
                GameEventSystem.Instance.RaiseCombatRequested(target);
                break;

            case "personality":
                // Apply personality trait changes
                if (!string.IsNullOrEmpty(target))
                {
                    var trait = PersonalityDatabase.GetTrait(target);
                    if (trait != null)
                    {
                        // Apply trait modifiers to player character
                        var player = GameManager.Instance?.GetPlayer();
                        if (player != null)
                        {
                            player.AddPersonalityTrait(trait);
                            Debug.Log($"[StoryEffectDispatcher] Applied personality trait: {trait.name}");
                        }
                        else
                        {
                            Debug.LogWarning("[StoryEffectDispatcher] No player character found to apply personality trait");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[StoryEffectDispatcher] Personality trait not found: {target}");
                    }
                }
                break;

            case "flag":
                // Set story flag using StoryManager
                if (!string.IsNullOrEmpty(target))
                {
                    StoryManager.Instance?.SetFlag(target, stringValue ?? value.ToString() ?? "true");
                    Debug.Log($"[StoryEffectDispatcher] Set story flag: {target} = {stringValue ?? value.ToString() ?? "true"}");
                }
                break;

            case "item":
                // Add item to inventory
                if (!string.IsNullOrEmpty(target))
                {
                    var item = ItemDatabase.Get(target);
                    if (item != null)
                    {
                        var inventorySystem = UnityEngine.Object.FindFirstObjectByType<InventorySystem>();
                        if (inventorySystem != null)
                        {
                            int quantity = value > 0 ? (int)value : 1;
                            inventorySystem.AddItem(item, quantity);
                            Debug.Log($"[StoryEffectDispatcher] Added item to inventory: {item.name} x{quantity}");
                        }
                        else
                        {
                            Debug.LogWarning("[StoryEffectDispatcher] InventorySystem not found to add item");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[StoryEffectDispatcher] Item not found: {target}");
                    }
                }
                break;

            default:
                Debug.Log($"[StoryEffectDispatcher] Unhandled effect type: {type} ({target}, {value}, {stringValue})");
                break;
        }
    }
}