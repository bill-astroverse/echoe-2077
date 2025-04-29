using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private NFTCharacterFactory characterFactory;
    [SerializeField] private GameObject inventoryButton;
    
    private NFTCharacter activeCharacter;
    
    private void Start()
    {
        // Subscribe to events
        GameEvents.OnNFTCharacterSelected += HandleCharacterSelected;
        
        // Hide inventory button until wallet is connected
        inventoryButton.SetActive(false);
        
        // Subscribe to wallet events
        Web3Manager.Instance.OnWalletConnected += HandleWalletConnected;
        Web3Manager.Instance.OnWalletDisconnected += HandleWalletDisconnected;
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        GameEvents.OnNFTCharacterSelected -= HandleCharacterSelected;
        
        if (Web3Manager.Instance != null)
        {
            Web3Manager.Instance.OnWalletConnected -= HandleWalletConnected;
            Web3Manager.Instance.OnWalletDisconnected -= HandleWalletDisconnected;
        }
    }
    
    private void HandleCharacterSelected(NFTCharacter character)
    {
        activeCharacter = character;
        
        // Position the character in the game world
        if (activeCharacter != null)
        {
            activeCharacter.transform.position = Vector3.zero;
            
            // Notify game that character is active
            Debug.Log($"Character {character.characterData.name} is now active");
            
            // Start the game
            GameEvents.OnGameStarted?.Invoke();
        }
    }
    
    private void HandleWalletConnected(string address)
    {
        // Show inventory button when wallet is connected
        inventoryButton.SetActive(true);
    }
    
    private void HandleWalletDisconnected(string _)
    {
        // Hide inventory button when wallet is disconnected
        inventoryButton.SetActive(false);
        
        // Deactivate current character if any
        if (activeCharacter != null)
        {
            activeCharacter = null;
        }
    }
    
    public void OpenInventory()
    {
        GameEvents.OnOpenNFTInventory?.Invoke();
    }
    
    public NFTCharacter GetActiveCharacter()
    {
        return activeCharacter;
    }

    public void OpenMarketplace()
    {
        GameEvents.OnOpenMarketplace?.Invoke();
    }
}
