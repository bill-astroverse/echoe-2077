using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NFTInventoryUI : MonoBehaviour
{
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Transform characterCardsContainer;
    [SerializeField] private GameObject characterCardPrefab;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button mintButton;
    
    private List<GameObject> spawnedCards = new List<GameObject>();
    
    private void Awake()
    {
        closeButton.onClick.AddListener(CloseInventory);
        mintButton.onClick.AddListener(OpenMintDialog);
        
        // Hide inventory at start
        inventoryPanel.SetActive(false);
    }
    
    private void Start()
    {
        // Subscribe to events
        GameEvents.OnOpenNFTInventory += OpenInventory;
        GameEvents.OnCloseNFTInventory += CloseInventory;
        
        Web3Manager.Instance.OnCharactersLoaded += RefreshInventory;
        Web3Manager.Instance.OnCharacterUpdated += HandleCharacterUpdated;
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        GameEvents.OnOpenNFTInventory -= OpenInventory;
        GameEvents.OnCloseNFTInventory -= CloseInventory;
        
        if (Web3Manager.Instance != null)
        {
            Web3Manager.Instance.OnCharactersLoaded -= RefreshInventory;
            Web3Manager.Instance.OnCharacterUpdated -= HandleCharacterUpdated;
        }
    }
    
    private void OpenInventory()
    {
        inventoryPanel.SetActive(true);
        RefreshInventory();
    }
    
    private void CloseInventory()
    {
        inventoryPanel.SetActive(false);
    }
    
    private void RefreshInventory()
    {
        // Clear existing cards
        ClearCards();
        
        // Get characters from factory
        NFTCharacterFactory factory = FindObjectOfType<NFTCharacterFactory>();
        if (factory != null)
        {
            List<NFTCharacter> characters = factory.GetAllCharacters();
            foreach (var character in characters)
            {
                CreateCharacterCard(character);
            }
        }
    }
    
    private void RefreshInventory(List<NFTCharacterData> characters)
    {
        // This overload is called when characters are loaded from Web3Manager
        // We don't need to do anything here as the factory will handle spawning the characters
        // and we'll get notified via the NFTCharacterInitialized event
    }
    
    private void HandleCharacterUpdated(NFTCharacterData characterData)
    {
        // Refresh the inventory when a character is updated
        RefreshInventory();
    }
    
    private void CreateCharacterCard(NFTCharacter character)
    {
        GameObject cardObj = Instantiate(characterCardPrefab, characterCardsContainer);
        spawnedCards.Add(cardObj);
        
        // Set card data
        CharacterCardUI cardUI = cardObj.GetComponent<CharacterCardUI>();
        if (cardUI != null)
        {
            cardUI.SetCharacter(character);
            cardUI.OnCardClicked += HandleCardClicked;
        }
    }
    
    private void ClearCards()
    {
        foreach (var card in spawnedCards)
        {
            CharacterCardUI cardUI = card.GetComponent<CharacterCardUI>();
            if (cardUI != null)
            {
                cardUI.OnCardClicked -= HandleCardClicked;
            }
            
            Destroy(card);
        }
        
        spawnedCards.Clear();
    }
    
    private void HandleCardClicked(NFTCharacter character)
    {
        // Notify that a character was selected
        GameEvents.OnNFTCharacterSelected?.Invoke(character);
        
        // Close the inventory
        CloseInventory();
    }
    
    private void OpenMintDialog()
    {
        // Open the mint dialog
        MintCharacterUI mintUI = FindObjectOfType<MintCharacterUI>(true);
        if (mintUI != null)
        {
            mintUI.gameObject.SetActive(true);
        }
    }
}
