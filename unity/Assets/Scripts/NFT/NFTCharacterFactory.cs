using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NFTCharacterFactory : MonoBehaviour
{
    [SerializeField] private GameObject characterPrefab;
    [SerializeField] private Transform charactersParent;
    
    private Dictionary<string, NFTCharacter> spawnedCharacters = new Dictionary<string, NFTCharacter>();
    
    private void Start()
    {
        // Subscribe to character loaded event
        Web3Manager.Instance.OnCharactersLoaded += HandleCharactersLoaded;
        Web3Manager.Instance.OnCharacterUpdated += HandleCharacterUpdated;
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        if (Web3Manager.Instance != null)
        {
            Web3Manager.Instance.OnCharactersLoaded -= HandleCharactersLoaded;
            Web3Manager.Instance.OnCharacterUpdated -= HandleCharacterUpdated;
        }
    }
    
    private void HandleCharactersLoaded(List<NFTCharacterData> characters)
    {
        foreach (var characterData in characters)
        {
            SpawnCharacter(characterData);
        }
    }
    
    private void HandleCharacterUpdated(NFTCharacterData characterData)
    {
        if (spawnedCharacters.TryGetValue(characterData.tokenId, out NFTCharacter character))
        {
            // Update existing character
            character.Initialize(characterData);
        }
        else
        {
            // Spawn new character
            SpawnCharacter(characterData);
        }
    }
    
    public NFTCharacter SpawnCharacter(NFTCharacterData characterData)
    {
        if (spawnedCharacters.TryGetValue(characterData.tokenId, out NFTCharacter existingCharacter))
        {
            // Character already exists, update it
            existingCharacter.Initialize(characterData);
            return existingCharacter;
        }
        
        // Create new character
        GameObject characterObj = Instantiate(characterPrefab, charactersParent);
        NFTCharacter nftCharacter = characterObj.GetComponent<NFTCharacter>();
        
        if (nftCharacter != null)
        {
            nftCharacter.Initialize(characterData);
            spawnedCharacters.Add(characterData.tokenId, nftCharacter);
            return nftCharacter;
        }
        
        Debug.LogError("Failed to spawn character: NFTCharacter component not found on prefab");
        return null;
    }
    
    public void DespawnCharacter(string tokenId)
    {
        if (spawnedCharacters.TryGetValue(tokenId, out NFTCharacter character))
        {
            Destroy(character.gameObject);
            spawnedCharacters.Remove(tokenId);
        }
    }
    
    public NFTCharacter GetCharacter(string tokenId)
    {
        if (spawnedCharacters.TryGetValue(tokenId, out NFTCharacter character))
        {
            return character;
        }
        
        return null;
    }
    
    public List<NFTCharacter> GetAllCharacters()
    {
        return new List<NFTCharacter>(spawnedCharacters.Values);
    }
}
