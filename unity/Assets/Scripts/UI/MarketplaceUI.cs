using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MarketplaceUI : MonoBehaviour
{
    [SerializeField] private GameObject marketplacePanel;
    [SerializeField] private Transform listingsContainer;
    [SerializeField] private GameObject listingCardPrefab;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button refreshButton;
    [SerializeField] private Button myListingsButton;
    [SerializeField] private Button allListingsButton;
    [SerializeField] private Button analyticsButton;
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TMP_Dropdown rarityFilterDropdown;
    [SerializeField] private Toggle showAllRaritiesToggle;
    
    private List<GameObject> spawnedListingCards = new List<GameObject>();
    private bool showingMyListings = false;
    
    private void Awake()
    {
        closeButton.onClick.AddListener(CloseMarketplace);
        refreshButton.onClick.AddListener(RefreshListings);
        myListingsButton.onClick.AddListener(ShowMyListings);
        allListingsButton.onClick.AddListener(ShowAllListings);
        analyticsButton.onClick.AddListener(OpenAnalytics);
        rarityFilterDropdown.onValueChanged.AddListener(OnRarityFilterChanged);
        showAllRaritiesToggle.onValueChanged.AddListener(OnShowAllRaritiesChanged);
        
        // Hide marketplace at start
        marketplacePanel.SetActive(false);
        loadingPanel.SetActive(false);
    }
    
    private void Start()
    {
        // Subscribe to events
        GameEvents.OnOpenMarketplace += OpenMarketplace;
        GameEvents.OnCloseMarketplace += CloseMarketplace;
        
        MarketplaceManager.Instance.OnListingsLoaded += HandleListingsLoaded;
        MarketplaceManager.Instance.OnListingCreated += HandleListingCreated;
        MarketplaceManager.Instance.OnListingUpdated += HandleListingUpdated;
        MarketplaceManager.Instance.OnListingSold += HandleListingSold;
        MarketplaceManager.Instance.OnListingCancelled += HandleListingCancelled;
        
        // Initialize rarity filter dropdown
        InitializeRarityFilter();
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        GameEvents.OnOpenMarketplace -= OpenMarketplace;
        GameEvents.OnCloseMarketplace -= CloseMarketplace;
        
        if (MarketplaceManager.Instance != null)
        {
            MarketplaceManager.Instance.OnListingsLoaded -= HandleListingsLoaded;
            MarketplaceManager.Instance.OnListingCreated -= HandleListingCreated;
            MarketplaceManager.Instance.OnListingUpdated -= HandleListingUpdated;
            MarketplaceManager.Instance.OnListingSold -= HandleListingSold;
            MarketplaceManager.Instance.OnListingCancelled -= HandleListingCancelled;
        }
    }
    
    private void OpenMarketplace()
    {
        marketplacePanel.SetActive(true);
        ShowAllListings();
    }
    
    private void CloseMarketplace()
    {
        marketplacePanel.SetActive(false);
    }
    
    private async void ShowAllListings()
    {
        showingMyListings = false;
        titleText.text = "Marketplace - All Listings";
        
        loadingPanel.SetActive(true);
        ClearListingCards();
        
        await MarketplaceManager.Instance.GetActiveListings();
        
        loadingPanel.SetActive(false);
    }
    
    private async void ShowMyListings()
    {
        if (!Web3Manager.Instance.IsWalletConnected())
        {
            Debug.LogError("Wallet not connected");
            return;
        }
        
        showingMyListings = true;
        titleText.text = "Marketplace - My Listings";
        
        loadingPanel.SetActive(true);
        ClearListingCards();
        
        var listings = await MarketplaceManager.Instance.GetMyListings();
        HandleListingsLoaded(listings);
        
        loadingPanel.SetActive(false);
    }
    
    private void RefreshListings()
    {
        if (showingMyListings)
        {
            ShowMyListings();
        }
        else
        {
            ShowAllListings();
        }
    }
    
    private void HandleListingsLoaded(List<MarketplaceListing> listings)
    {
        ClearListingCards();
        
        foreach (var listing in listings)
        {
            // Apply rarity filter if needed
            if (!showAllRaritiesToggle.isOn)
            {
                // Get selected rarity
                RarityTier selectedRarity = (RarityTier)rarityFilterDropdown.value;
                
                // Check if listing has rarity info
                if (listing.characterData.attributes.TryGetValue("rarity", out string rarityStr) && 
                    Enum.TryParse<RarityTier>(rarityStr, out RarityTier listingRarity))
                {
                    // Skip if not matching the filter
                    if (listingRarity != selectedRarity)
                    {
                        continue;
                    }
                }
                else if (selectedRarity != RarityTier.Common)
                {
                    // If no rarity info, assume Common and skip if filter is not Common
                    continue;
                }
            }
        
            CreateListingCard(listing);
        }
    }
    
    private void HandleListingCreated(MarketplaceListing listing)
    {
        RefreshListings();
    }
    
    private void HandleListingUpdated(MarketplaceListing listing)
    {
        RefreshListings();
    }
    
    private void HandleListingSold(MarketplaceListing listing)
    {
        RefreshListings();
    }
    
    private void HandleListingCancelled(MarketplaceListing listing)
    {
        RefreshListings();
    }
    
    private void CreateListingCard(MarketplaceListing listing)
    {
        GameObject cardObj = Instantiate(listingCardPrefab, listingsContainer);
        spawnedListingCards.Add(cardObj);
        
        // Set card data
        ListingCardUI cardUI = cardObj.GetComponent<ListingCardUI>();
        if (cardUI != null)
        {
            cardUI.SetListing(listing);
        }
    }
    
    private void ClearListingCards()
    {
        foreach (var card in spawnedListingCards)
        {
            Destroy(card);
        }
        
        spawnedListingCards.Clear();
    }

    private void OpenAnalytics()
    {
        GameEvents.OnOpenMarketplaceAnalytics?.Invoke();
    }
    
    private void InitializeRarityFilter()
    {
        rarityFilterDropdown.ClearOptions();
        
        List<string> options = new List<string>();
        foreach (RarityTier rarity in Enum.GetValues(typeof(RarityTier)))
        {
            options.Add(RaritySystem.GetRarityName(rarity));
        }
        
        rarityFilterDropdown.AddOptions(options);
        rarityFilterDropdown.value = 0; // Default to Common
    }

    private void OnRarityFilterChanged(int index)
    {
        showAllRaritiesToggle.isOn = false;
        RefreshListings();
    }

    private void OnShowAllRaritiesChanged(bool showAll)
    {
        if (showAll)
        {
            rarityFilterDropdown.interactable = false;
        }
        else
        {
            rarityFilterDropdown.interactable = true;
        }
        
        RefreshListings();
    }
}
