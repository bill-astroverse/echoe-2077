using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ListingCardUI : MonoBehaviour
{
    [SerializeField] private Image characterImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private TextMeshProUGUI rarityText;
    [SerializeField] private Image rarityBorder;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TextMeshProUGUI sellerText;
    [SerializeField] private Button buyButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button updatePriceButton;
    [SerializeField] private GameObject loadingPanel;
    
    private MarketplaceListing listing;
    
    private void Awake()
    {
        buyButton.onClick.AddListener(HandleBuyClicked);
        cancelButton.onClick.AddListener(HandleCancelClicked);
        updatePriceButton.onClick.AddListener(HandleUpdatePriceClicked);
        
        // Hide loading panel
        loadingPanel.SetActive(false);
    }
    
    public void SetListing(MarketplaceListing marketplaceListing)
    {
        listing = marketplaceListing;
        
        // Update UI elements
        nameText.text = listing.characterData.name;
        levelText.text = $"Level {listing.characterData.level}";
        statsText.text = $"STR: {listing.characterData.strength} | AGI: {listing.characterData.agility} | INT: {listing.characterData.intelligence}";
        priceText.text = MarketplaceManager.Instance.FormatPrice(listing.price);
        
        // Set rarity text and color
        if (listing.characterData.attributes.TryGetValue("rarity", out string rarityStr) && 
            Enum.TryParse<RarityTier>(rarityStr, out RarityTier rarity))
        {
            rarityText.text = RaritySystem.GetRarityName(rarity);
            rarityText.color = RaritySystem.GetRarityColor(rarity);
            rarityBorder.color = RaritySystem.GetRarityColor(rarity);
        }
        else
        {
            rarityText.text = "Common";
            rarityText.color = RaritySystem.GetRarityColor(RarityTier.Common);
            rarityBorder.color = RaritySystem.GetRarityColor(RarityTier.Common);
        }
        
        // Format seller address
        string formattedSeller = listing.seller;
        if (formattedSeller.Length > 10)
        {
            formattedSeller = $"{formattedSeller.Substring(0, 6)}...{formattedSeller.Substring(formattedSeller.Length - 4)}";
        }
        sellerText.text = $"Seller: {formattedSeller}";
        
        // Load character image if available
        StartCoroutine(LoadCharacterImage(listing.characterData.imageURI));
        
        // Show/hide buttons based on ownership
        bool isOwner = Web3Manager.Instance.IsWalletConnected() && 
                       Web3Manager.Instance.GetConnectedAccount().ToLower() == listing.seller.ToLower();
        
        buyButton.gameObject.SetActive(!isOwner);
        cancelButton.gameObject.SetActive(isOwner);
        updatePriceButton.gameObject.SetActive(isOwner);
    }
    
    private IEnumerator LoadCharacterImage(string uri)
    {
        if (string.IsNullOrEmpty(uri))
        {
            yield break;
        }
        
        // Handle both IPFS and HTTP URIs
        string fullUri = uri;
        if (uri.StartsWith("ipfs://"))
        {
            fullUri = uri.Replace("ipfs://", "https://ipfs.io/ipfs/");
        }
        
        using (var webRequest = UnityEngine.Networking.UnityWebRequestTexture.GetTexture(fullUri))
        {
            yield return webRequest.SendWebRequest();
            
            if (webRequest.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Texture2D texture = ((UnityEngine.Networking.DownloadHandlerTexture)webRequest.downloadHandler).texture;
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                
                if (characterImage != null)
                {
                    characterImage.sprite = sprite;
                    characterImage.preserveAspect = true;
                }
            }
            else
            {
                Debug.LogError($"Failed to load character image: {webRequest.error}");
            }
        }
    }
    
    private async void HandleBuyClicked()
    {
        if (!Web3Manager.Instance.IsWalletConnected())
        {
            Debug.LogError("Wallet not connected");
            return;
        }
        
        // Show loading panel
        loadingPanel.SetActive(true);
        
        // Buy the listing
        bool success = await MarketplaceManager.Instance.BuyListing(listing.listingId, listing.price);
        
        // Hide loading panel
        loadingPanel.SetActive(false);
        
        if (!success)
        {
            Debug.LogError("Failed to buy listing");
        }
    }
    
    private async void HandleCancelClicked()
    {
        if (!Web3Manager.Instance.IsWalletConnected())
        {
            Debug.LogError("Wallet not connected");
            return;
        }
        
        // Show loading panel
        loadingPanel.SetActive(true);
        
        // Cancel the listing
        bool success = await MarketplaceManager.Instance.CancelListing(listing.listingId);
        
        // Hide loading panel
        loadingPanel.SetActive(false);
        
        if (!success)
        {
            Debug.LogError("Failed to cancel listing");
        }
    }
    
    private void HandleUpdatePriceClicked()
    {
        if (!Web3Manager.Instance.IsWalletConnected())
        {
            Debug.LogError("Wallet not connected");
            return;
        }
        
        // Open update price dialog
        UpdateListingPriceUI updateUI = FindObjectOfType<UpdateListingPriceUI>(true);
        if (updateUI != null)
        {
            updateUI.gameObject.SetActive(true);
            updateUI.SetListing(listing);
        }
    }
}
