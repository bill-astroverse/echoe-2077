using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;

public class NFTMetadataGenerator : MonoBehaviour
{
    public static NFTMetadataGenerator Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Generates and uploads metadata for an NFT character
    /// </summary>
    /// <param name="characterData">The character data</param>
    /// <returns>The URL of the uploaded metadata</returns>
    public async Task<string> GenerateAndUploadMetadataAsync(NFTCharacterData characterData)
    {
        // Create the metadata object
        var metadata = new NFTMetadata
        {
            name = characterData.name,
            description = $"A level {characterData.level} character with {characterData.strength} strength, {characterData.agility} agility, and {characterData.intelligence} intelligence.",
            image = characterData.imageURI,
            external_url = $"https://your-game-website.com/character/{characterData.tokenId}",
            attributes = new List<NFTAttribute>()
        };
        
        // Add basic attributes
        metadata.attributes.Add(new NFTAttribute { trait_type = "Level", value = characterData.level });
        metadata.attributes.Add(new NFTAttribute { trait_type = "Strength", value = characterData.strength });
        metadata.attributes.Add(new NFTAttribute { trait_type = "Agility", value = characterData.agility });
        metadata.attributes.Add(new NFTAttribute { trait_type = "Intelligence", value = characterData.intelligence });
        
        // Add rarity
        metadata.attributes.Add(new NFTAttribute { trait_type = "Rarity", value = characterData.RarityName });
        
        // Add special traits
        foreach (var trait in characterData.attributes)
        {
            // Skip rarity as we already added it
            if (trait.Key == "rarity")
                continue;
                
            metadata.attributes.Add(new NFTAttribute { trait_type = FormatTraitName(trait.Key), value = trait.Value });
        }
        
        // Upload the metadata to Vercel Blob
        string fileName = $"metadata_{characterData.tokenId}.json";
        string metadataUrl = await BlobStorageManager.Instance.UploadJsonAsync(metadata, fileName);
        
        return metadataUrl;
    }
    
    private string FormatTraitName(string traitKey)
    {
        // Convert snake_case to Title Case
        string[] words = traitKey.Split('_');
        for (int i = 0; i < words.Length; i++)
        {
            if (words[i].Length > 0)
            {
                words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1);
            }
        }
        
        return string.Join(" ", words);
    }
    
    [Serializable]
    private class NFTMetadata
    {
        public string name;
        public string description;
        public string image;
        public string external_url;
        public List<NFTAttribute> attributes;
    }
    
    [Serializable]
    private class NFTAttribute
    {
        public string trait_type;
        public object value;
    }
}
