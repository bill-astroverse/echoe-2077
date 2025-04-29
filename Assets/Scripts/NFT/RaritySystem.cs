using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum RarityTier
{
    Common = 0,
    Uncommon = 1,
    Rare = 2,
    Epic = 3,
    Legendary = 4,
    Mythic = 5
}

public static class RaritySystem
{
    // Rarity colors
    public static readonly Color CommonColor = new Color(0.7f, 0.7f, 0.7f);
    public static readonly Color UncommonColor = new Color(0.3f, 0.8f, 0.3f);
    public static readonly Color RareColor = new Color(0.3f, 0.5f, 0.9f);
    public static readonly Color EpicColor = new Color(0.7f, 0.3f, 0.9f);
    public static readonly Color LegendaryColor = new Color(1.0f, 0.7f, 0.0f);
    public static readonly Color MythicColor = new Color(1.0f, 0.3f, 0.3f);
    
    // Rarity value multipliers (affects base price)
    public static readonly float[] ValueMultipliers = {
        1.0f,    // Common
        1.5f,    // Uncommon
        2.5f,    // Rare
        4.0f,    // Epic
        7.0f,    // Legendary
        12.0f    // Mythic
    };
    
    // Rarity drop rates (percentages)
    public static readonly float[] DropRates = {
        60.0f,   // Common (60%)
        25.0f,   // Uncommon (25%)
        10.0f,   // Rare (10%)
        3.0f,    // Epic (3%)
        1.5f,    // Legendary (1.5%)
        0.5f     // Mythic (0.5%)
    };
    
    // Get color for a rarity tier
    public static Color GetRarityColor(RarityTier tier)
    {
        switch (tier)
        {
            case RarityTier.Common:
                return CommonColor;
            case RarityTier.Uncommon:
                return UncommonColor;
            case RarityTier.Rare:
                return RareColor;
            case RarityTier.Epic:
                return EpicColor;
            case RarityTier.Legendary:
                return LegendaryColor;
            case RarityTier.Mythic:
                return MythicColor;
            default:
                return Color.white;
        }
    }
    
    // Get display name for a rarity tier
    public static string GetRarityName(RarityTier tier)
    {
        return tier.ToString();
    }
    
    // Get value multiplier for a rarity tier
    public static float GetValueMultiplier(RarityTier tier)
    {
        return ValueMultipliers[(int)tier];
    }
    
    // Calculate a suggested base price in wei based on character stats and rarity
    public static string CalculateBasePrice(NFTCharacterData characterData)
    {
        // Base price calculation (in ETH)
        float basePrice = 0.01f; // Starting at 0.01 ETH
        
        // Add value based on level
        basePrice += characterData.level * 0.005f;
        
        // Add value based on stats
        int totalStats = characterData.strength + characterData.agility + characterData.intelligence;
        basePrice += totalStats * 0.001f;
        
        // Apply rarity multiplier if available
        if (characterData.attributes.TryGetValue("rarity", out string rarityStr) && 
            Enum.TryParse<RarityTier>(rarityStr, out RarityTier rarity))
        {
            basePrice *= GetValueMultiplier(rarity);
        }
        
        // Convert to wei (1 ETH = 10^18 wei)
        decimal weiPrice = (decimal)basePrice * 1000000000000000000m;
        
        // Return as string
        return weiPrice.ToString("0");
    }
    
    // Determine rarity based on character stats
    public static RarityTier DetermineRarity(int strength, int agility, int intelligence)
    {
        // Calculate total stats
        int totalStats = strength + agility + intelligence;
        
        // Calculate stat distribution (how specialized the character is)
        float maxStat = Mathf.Max(strength, Mathf.Max(agility, intelligence));
        float statVariance = maxStat / (float)((totalStats / 3) + 0.1f); // Avoid division by zero
        
        // Calculate rarity score (0-100)
        float rarityScore = (totalStats * 2) + (statVariance * 10);
        
        // Determine rarity tier based on score
        if (rarityScore >= 95) return RarityTier.Mythic;
        if (rarityScore >= 85) return RarityTier.Legendary;
        if (rarityScore >= 70) return RarityTier.Epic;
        if (rarityScore >= 55) return RarityTier.Rare;
        if (rarityScore >= 40) return RarityTier.Uncommon;
        return RarityTier.Common;
    }
    
    // Roll for a random rarity tier based on drop rates
    public static RarityTier RollRandomRarity()
    {
        float roll = UnityEngine.Random.Range(0f, 100f);
        float cumulativeChance = 0f;
        
        for (int i = 0; i < DropRates.Length; i++)
        {
            cumulativeChance += DropRates[i];
            if (roll <= cumulativeChance)
            {
                return (RarityTier)i;
            }
        }
        
        return RarityTier.Common; // Fallback
    }
    
    // Generate special traits based on rarity
    public static Dictionary<string, string> GenerateSpecialTraits(RarityTier rarity)
    {
        Dictionary<string, string> traits = new Dictionary<string, string>();
        
        // Number of special traits based on rarity
        int traitCount = (int)rarity;
        
        // List of possible traits
        string[] traitTypes = {
            "element", "weapon", "armor", "special_ability", 
            "background", "origin", "faction", "companion"
        };
        
        // List of possible values for each trait type
        Dictionary<string, string[]> traitValues = new Dictionary<string, string[]>
        {
            { "element", new[] { "fire", "water", "earth", "air", "light", "shadow", "void", "cosmic" } },
            { "weapon", new[] { "sword", "axe", "bow", "staff", "dagger", "hammer", "spear", "wand" } },
            { "armor", new[] { "cloth", "leather", "chainmail", "plate", "crystal", "dragon_scale", "void_forged", "ancient" } },
            { "special_ability", new[] { "healing", "teleport", "invisibility", "flight", "time_control", "mind_control", "elemental_mastery", "resurrection" } },
            { "background", new[] { "noble", "peasant", "mercenary", "scholar", "outlaw", "royalty", "divine", "otherworldly" } },
            { "origin", new[] { "forest", "mountain", "desert", "ocean", "city", "underworld", "celestial", "void" } },
            { "faction", new[] { "kingdom", "empire", "guild", "cult", "tribe", "order", "rebellion", "pantheon" } },
            { "companion", new[] { "wolf", "hawk", "dragon", "spirit", "golem", "fairy", "demon", "angel" } }
        };
        
        // Select random traits based on rarity
        List<string> selectedTraitTypes = new List<string>(traitTypes);
        for (int i = 0; i < traitCount && selectedTraitTypes.Count > 0; i++)
        {
            int index = UnityEngine.Random.Range(0, selectedTraitTypes.Count);
            string traitType = selectedTraitTypes[index];
            selectedTraitTypes.RemoveAt(index);
            
            if (traitValues.TryGetValue(traitType, out string[] values))
            {
                // Higher rarity has a better chance of getting rarer trait values
                int valueIndex = Mathf.Min(
                    UnityEngine.Random.Range(0, values.Length),
                    UnityEngine.Random.Range(0, values.Length - (int)rarity)
                );
                valueIndex = Mathf.Clamp(valueIndex, 0, values.Length - 1);
                
                traits[traitType] = values[valueIndex];
            }
        }
        
        return traits;
    }
}
