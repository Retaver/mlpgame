using UnityEngine;
using MyGameNamespace;

public class CharacterSystem : MonoBehaviour
{
    private static CharacterSystem instance;
    public static CharacterSystem Instance
    {
        get
        {
            if (instance == null)
            {
                // Try to find existing instance
                instance = FindFirstObjectByType<CharacterSystem>();
                
                // If not found, create a new one
                if (instance == null)
                {
                    GameObject go = new GameObject("CharacterSystem");
                    instance = go.AddComponent<CharacterSystem>();
                    DontDestroyOnLoad(go);
                    Debug.Log("[CharacterSystem] Created new singleton instance");
                }
            }
            return instance;
        }
    }

    private PlayerCharacter playerCharacter;
    private Inventory inventory;

    // Ensure initialization as soon as the GameObject is created
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[CharacterSystem] Singleton instance set");
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Initialize();
    }

    public void Initialize()
    {
        if (playerCharacter == default)
        {
            Debug.Log("[CharacterSystem] Creating new PlayerCharacter");
            playerCharacter = new PlayerCharacter();
        }

        if (inventory == default)
        {
            inventory = new Inventory();
        }
    }

    public PlayerCharacter GetPlayerCharacter()
    {
        // Return existing playerCharacter (Awake should have initialized it)
        if (playerCharacter == default)
        {
            Debug.Log("[CharacterSystem] PlayerCharacter was null when requested  initializing now");
            Initialize();
        }
        return playerCharacter;
    }

    public void SetPlayerCharacter(PlayerCharacter character)
    {
        playerCharacter = character;
        Debug.Log($"[CharacterSystem] Player character set: {character?.name ?? "null"}");

        // Notify UI systems of the change
        if (character != default)
        {
            GameEventSystem.Instance?.RaisePlayerCharacterChanged(character);
            GameEventSystem.Instance?.RaisePlayerStatsChanged(character);
        }
    }

    // Clear character data (useful for starting fresh)
    public void ClearCharacterData()
    {
        playerCharacter = null;
        inventory = null;
        Debug.Log("[CharacterSystem] Character data cleared");
    }

    public void ApplyRacialBonuses(Race race)
    {
        if (race == default || playerCharacter == default) return;

        playerCharacter.ApplyRacialBonuses(race);
        Debug.Log($"[CharacterSystem] Applied racial bonuses for {race.name}");
    }

    public void AddStartingItems()
    {
        if (inventory == default) inventory = new Inventory();

        var itemDb = ItemDatabase.Instance;
        if (itemDb != default)
        {
            var healthPotion = itemDb.GetItemById("health_potion");
            var energyPotion = itemDb.GetItemById("energy_potion");
            var bread = itemDb.GetItemById("bread");

            if (healthPotion != default) inventory.AddItem(healthPotion, 2);
            if (energyPotion != default) inventory.AddItem(energyPotion, 1);
            if (bread != default) inventory.AddItem(bread, 3);

            Debug.Log("[CharacterSystem] Added starting items to inventory");
        }
        else
        {
            Debug.LogError("[CharacterSystem] ItemDatabase instance not found");
        }
    }

    public Inventory GetInventory()
    {
        if (inventory == default) inventory = new Inventory();
        return inventory;
    }

    public string GetPlayerStatsDisplay()
    {
        if (playerCharacter == default || playerCharacter.stats == default)
            return "No character stats available";

        return playerCharacter.stats.GetStatsDisplay();
    }

    public void AddExperience(int amount)
    {
        if (playerCharacter != default)
        {
            playerCharacter.AddExperience(amount);
            Debug.Log($"[CharacterSystem] Added {amount} XP to player. Current level: {playerCharacter.level}");
        }
        else
        {
            Debug.LogError("[CharacterSystem] Cannot add experience - PlayerCharacter is null");
        }
    }

    public bool AddPerk(PerkType perkType)
    {
        if (playerCharacter != default)
        {
            return playerCharacter.AddPerk(perkType);
        }
        Debug.LogError("[CharacterSystem] Cannot add perk - PlayerCharacter is null");
        return false;
    }

    public bool AllocateStatPoint(StatType stat, int points = 1)
    {
        if (playerCharacter != default)
        {
            return playerCharacter.AllocateStatPoint(stat, points);
        }
        Debug.LogError("[CharacterSystem] Cannot allocate stat point - PlayerCharacter is null");
        return false;
    }
}