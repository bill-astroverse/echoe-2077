using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CreateListingUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private TextMeshProUGUI rarityText;
    [SerializeField] private TMP_InputField priceInput;
    [SerializeField] private TextMeshProUGUI suggestedPriceText;
    [SerializeField] private Button createButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private GameObject loadingPanel;
    
    private NFTCharacter character;
    
    private void Awake()
    {
        createButton.onClick.AddListener(OnCreateClicked);
        cancelButton.onClick.AddListener(OnCancelClicked);
        
        // Hide loading panel
        loadingPanel.SetActive(false);
    }
    
    public void SetCharacter(NFTCharacter nftCharacter)
    {
        character = nftCharacter;
        characterNameText.text = $"List {character.characterData.name} for Sale";
        
        // Set rarity text and color
        if (character.characterData.attributes.TryGetValue("rarity", out string rarityStr) && 
            Enum.TryParse<RarityTier>(rarityStr, out RarityTier rarity))
        {
            rarityText.text = $"Rarity: {RaritySystem.GetRarityName(rarity)}";
            rarityText.color = RaritySystem.GetRarityColor(rarity);
        }
        else
        {
            rarityText.text = "Rarity: Common";
            rarityText.color = RaritySystem.GetRarityColor(RarityTier.Common);
        }
        
        // Calculate suggested price based on character stats and rarity
        string suggestedPriceWei = RaritySystem.CalculateBasePrice(character.characterData);
        string suggestedPriceEth = MarketplaceManager.Instance.FormatPrice(suggestedPriceWei);
        suggestedPriceText.text = $"Suggested Price: {suggestedPriceEth}";
        
        // Set default price to suggested price
        decimal ethPrice = decimal.Parse(suggestedPriceWei) / 1000000000000000000m;
        priceInput.text = ethPrice.ToString("0.###");
    }
    
    private async void OnCreateClicked()
    {
        if (string.IsNullOrEmpty(priceInput.text))
        {
            Debug.LogError("Price cannot be empty");
            return;
        }
        
        // Convert ETH to wei
        string weiPrice = MarketplaceManager.Instance.ConvertEthToWei(priceInput.text);
        
        // Show loading panel
        loadingPanel.SetActive(true);
        
        // Create the listing
        bool success = await MarketplaceManager.Instance.CreateListing(character.characterData.tokenId, weiPrice);
        
        // Hide loading panel
        loadingPanel.SetActive(false);
        
        if (success)
        {
            // Close the dialog
            gameObject.SetActive(false);
            
            // Open the marketplace
            GameEvents.OnOpenMarketplace?.Invoke();
        }
        else
        {
            Debug.LogError("Failed to create listing");
        }
    }
    
    private void OnCancelClicked()
    {
        gameObject.SetActive(false);
    }
}
