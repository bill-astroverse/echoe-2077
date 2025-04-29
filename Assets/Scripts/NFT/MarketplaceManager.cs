using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using UnityEngine;
using Nethereum.Web3;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Newtonsoft.Json;

[Serializable]
public class MarketplaceListing
{
    public string listingId;
    public string nftContract;
    public string tokenId;
    public string seller;
    public string buyer;
    public string price; // Using string to handle large numbers
    public int status; // 0 = Active, 1 = Sold, 2 = Cancelled
    public long createdAt;
    public long updatedAt;
    public NFTCharacterData characterData; // Associated character data
}

public class MarketplaceManager : MonoBehaviour
{
    public static MarketplaceManager Instance { get; private set; }
    
    [Header("Marketplace Settings")]
    [SerializeField] private string marketplaceContractAddress = "0x0000000000000000000000000000000000000000"; // Replace with your marketplace contract address
    [SerializeField] private string marketplaceContractABI = ""; // Your marketplace contract ABI goes here
    
    private Contract marketplaceContract;
    private bool isInitialized = false;
    
    // Events
    public event Action<List<MarketplaceListing>> OnListingsLoaded;
    public event Action<MarketplaceListing> OnListingCreated;
    public event Action<MarketplaceListing> OnListingUpdated;
    public event Action<MarketplaceListing> OnListingSold;
    public event Action<MarketplaceListing> OnListingCancelled;
    
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
    
    private void Start()
    {
        Initialize();
    }
    
    private void Initialize()
    {
        try
        {
            if (Web3Manager.Instance != null && !string.IsNullOrEmpty(marketplaceContractAddress) && !string.IsNullOrEmpty(marketplaceContractABI))
            {
                marketplaceContract = Web3Manager.Instance.GetWeb3().Eth.GetContract(marketplaceContractABI, marketplaceContractAddress);
                isInitialized = true;
                Debug.Log("MarketplaceManager initialized successfully");
            }
            else
            {
                Debug.LogError("Failed to initialize MarketplaceManager: Web3Manager not found or contract details missing");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to initialize MarketplaceManager: {e.Message}");
        }
    }
    
    #region Marketplace Operations
    
    public async Task<List<MarketplaceListing>> GetActiveListings(int offset = 0, int limit = 20)
    {
        if (!isInitialized)
        {
            Debug.LogError("MarketplaceManager not initialized");
            return new List<MarketplaceListing>();
        }
        
        try
        {
            // In a real implementation, this would call the contract's getActiveListings function
            // For this example, we'll create some mock data
            
            await Task.Delay(1000); // Simulate blockchain delay
            
            var listings = new List<MarketplaceListing>();
            
            // Mock listing 1
            var listing1 = new MarketplaceListing
            {
                listingId = "1",
                nftContract = Web3Manager.Instance.GetNFTContractAddress(),
                tokenId = "101",
                seller = "0x123456789abcdef123456789abcdef123456789a",
                buyer = "",
                price = "50000000000000000", // 0.05 ETH
                status = 0, // Active
                createdAt = DateTimeOffset.UtcNow.AddDays(-2).ToUnixTimeSeconds(),
                updatedAt = DateTimeOffset.UtcNow.AddDays(-2).ToUnixTimeSeconds(),
                characterData = new NFTCharacterData
                {
                    tokenId = "101",
                    owner = "0x123456789abcdef123456789abcdef123456789a",
                    name = "Elite Warrior",
                    level = 8,
                    strength = 15,
                    agility = 10,
                    intelligence = 7,
                    imageURI = "https://example.com/elite-warrior.png"
                }
            };
            listings.Add(listing1);
            
            // Mock listing 2
            var listing2 = new MarketplaceListing
            {
                listingId = "2",
                nftContract = Web3Manager.Instance.GetNFTContractAddress(),
                tokenId = "102",
                seller = "0x987654321fedcba987654321fedcba987654321f",
                buyer = "",
                price = "100000000000000000", // 0.1 ETH
                status = 0, // Active
                createdAt = DateTimeOffset.UtcNow.AddDays(-1).ToUnixTimeSeconds(),
                updatedAt = DateTimeOffset.UtcNow.AddDays(-1).ToUnixTimeSeconds(),
                characterData = new NFTCharacterData
                {
                    tokenId = "102",
                    owner = "0x987654321fedcba987654321fedcba987654321f",
                    name = "Master Mage",
                    level = 10,
                    strength = 5,
                    agility = 8,
                    intelligence = 18,
                    imageURI = "https://example.com/master-mage.png"
                }
            };
            listings.Add(listing2);
            
            // Mock listing 3
            var listing3 = new MarketplaceListing
            {
                listingId = "3",
                nftContract = Web3Manager.Instance.GetNFTContractAddress(),
                tokenId = "103",
                seller = "0xabcdef123456789abcdef123456789abcdef1234",
                buyer = "",
                price = "75000000000000000", // 0.075 ETH
                status = 0, // Active
                createdAt = DateTimeOffset.UtcNow.AddHours(-12).ToUnixTimeSeconds(),
                updatedAt = DateTimeOffset.UtcNow.AddHours(-12).ToUnixTimeSeconds(),
                characterData = new NFTCharacterData
                {
                    tokenId = "103",
                    owner = "0xabcdef123456789abcdef123456789abcdef1234",
                    name = "Swift Rogue",
                    level = 7,
                    strength = 9,
                    agility = 16,
                    intelligence = 10,
                    imageURI = "https://example.com/swift-rogue.png"
                }
            };
            listings.Add(listing3);
            
            OnListingsLoaded?.Invoke(listings);
            return listings;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to get active listings: {e.Message}");
            return new List<MarketplaceListing>();
        }
    }
    
    public async Task<List<MarketplaceListing>> GetMyListings()
    {
        if (!isInitialized || !Web3Manager.Instance.IsWalletConnected())
        {
            Debug.LogError("MarketplaceManager not initialized or wallet not connected");
            return new List<MarketplaceListing>();
        }
        
        try
        {
            string connectedAccount = Web3Manager.Instance.GetConnectedAccount();
            
            // In a real implementation, this would call the contract's getListingsBySeller function
            // For this example, we'll create some mock data
            
            await Task.Delay(1000); // Simulate blockchain delay
            
            var listings = new List<MarketplaceListing>();
            
            // Mock listing
            var listing = new MarketplaceListing
            {
                listingId = "4",
                nftContract = Web3Manager.Instance.GetNFTContractAddress(),
                tokenId = "104",
                seller = connectedAccount,
                buyer = "",
                price = "60000000000000000", // 0.06 ETH
                status = 0, // Active
                createdAt = DateTimeOffset.UtcNow.AddHours(-6).ToUnixTimeSeconds(),
                updatedAt = DateTimeOffset.UtcNow.AddHours(-6).ToUnixTimeSeconds(),
                characterData = new NFTCharacterData
                {
                    tokenId = "104",
                    owner = connectedAccount,
                    name = "Battle Cleric",
                    level = 6,
                    strength = 8,
                    agility = 7,
                    intelligence = 12,
                    imageURI = "https://example.com/battle-cleric.png"
                }
            };
            listings.Add(listing);
            
            return listings;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to get my listings: {e.Message}");
            return new List<MarketplaceListing>();
        }
    }
    
    public async Task<bool> CreateListing(string tokenId, string price)
    {
        if (!isInitialized || !Web3Manager.Instance.IsWalletConnected())
        {
            Debug.LogError("MarketplaceManager not initialized or wallet not connected");
            return false;
        }
        
        try
        {
            Debug.Log($"Creating listing for token {tokenId} with price {price}");
            
            // In a real implementation, this would call the contract's createListing function
            // For this example, we'll simulate a successful listing creation
            
            await Task.Delay(2000); // Simulate blockchain delay
            
            // Create a mock listing
            string connectedAccount = Web3Manager.Instance.GetConnectedAccount();
            var listing = new MarketplaceListing
            {
                listingId = UnityEngine.Random.Range(100, 999).ToString(),
                nftContract = Web3Manager.Instance.GetNFTContractAddress(),
                tokenId = tokenId,
                seller = connectedAccount,
                buyer = "",
                price = price,
                status = 0, // Active
                createdAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                updatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
            
            // Get character data
            NFTCharacterFactory factory = FindObjectOfType<NFTCharacterFactory>();
            if (factory != null)
            {
                NFTCharacter character = factory.GetCharacter(tokenId);
                if (character != null)
                {
                    listing.characterData = character.characterData;
                }
            }
            
            OnListingCreated?.Invoke(listing);
            
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to create listing: {e.Message}");
            return false;
        }
    }
    
    public async Task<bool> UpdateListing(string listingId, string newPrice)
    {
        if (!isInitialized || !Web3Manager.Instance.IsWalletConnected())
        {
            Debug.LogError("MarketplaceManager not initialized or wallet not connected");
            return false;
        }
        
        try
        {
            Debug.Log($"Updating listing {listingId} with new price {newPrice}");
            
            // In a real implementation, this would call the contract's updateListing function
            // For this example, we'll simulate a successful update
            
            await Task.Delay(2000); // Simulate blockchain delay
            
            // Create a mock updated listing
            string connectedAccount = Web3Manager.Instance.GetConnectedAccount();
            var listing = new MarketplaceListing
            {
                listingId = listingId,
                nftContract = Web3Manager.Instance.GetNFTContractAddress(),
                tokenId = "104", // Example token ID
                seller = connectedAccount,
                buyer = "",
                price = newPrice,
                status = 0, // Active
                createdAt = DateTimeOffset.UtcNow.AddHours(-6).ToUnixTimeSeconds(),
                updatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                characterData = new NFTCharacterData
                {
                    tokenId = "104",
                    owner = connectedAccount,
                    name = "Battle Cleric",
                    level = 6,
                    strength = 8,
                    agility = 7,
                    intelligence = 12,
                    imageURI = "https://example.com/battle-cleric.png"
                }
            };
            
            OnListingUpdated?.Invoke(listing);
            
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to update listing: {e.Message}");
            return false;
        }
    }
    
    public async Task<bool> CancelListing(string listingId)
    {
        if (!isInitialized || !Web3Manager.Instance.IsWalletConnected())
        {
            Debug.LogError("MarketplaceManager not initialized or wallet not connected");
            return false;
        }
        
        try
        {
            Debug.Log($"Cancelling listing {listingId}");
            
            // In a real implementation, this would call the contract's cancelListing function
            // For this example, we'll simulate a successful cancellation
            
            await Task.Delay(2000); // Simulate blockchain delay
            
            // Create a mock cancelled listing
            string connectedAccount = Web3Manager.Instance.GetConnectedAccount();
            var listing = new MarketplaceListing
            {
                listingId = listingId,
                nftContract = Web3Manager.Instance.GetNFTContractAddress(),
                tokenId = "104", // Example token ID
                seller = connectedAccount,
                buyer = "",
                price = "60000000000000000", // 0.06 ETH
                status = 2, // Cancelled
                createdAt = DateTimeOffset.UtcNow.AddHours(-6).ToUnixTimeSeconds(),
                updatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                characterData = new NFTCharacterData
                {
                    tokenId = "104",
                    owner = connectedAccount,
                    name = "Battle Cleric",
                    level = 6,
                    strength = 8,
                    agility = 7,
                    intelligence = 12,
                    imageURI = "https://example.com/battle-cleric.png"
                }
            };
            
            OnListingCancelled?.Invoke(listing);
            
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to cancel listing: {e.Message}");
            return false;
        }
    }
    
    public async Task<bool> BuyListing(string listingId, string price)
    {
        if (!isInitialized || !Web3Manager.Instance.IsWalletConnected())
        {
            Debug.LogError("MarketplaceManager not initialized or wallet not connected");
            return false;
        }
        
        try
        {
            Debug.Log($"Buying listing {listingId} for {price}");
            
            // In a real implementation, this would call the contract's buyListing function
            // For this example, we'll simulate a successful purchase
            
            await Task.Delay(2000); // Simulate blockchain delay
            
            // Create a mock purchased listing
            string connectedAccount = Web3Manager.Instance.GetConnectedAccount();
            var listing = new MarketplaceListing
            {
                listingId = listingId,
                nftContract = Web3Manager.Instance.GetNFTContractAddress(),
                tokenId = "102", // Example token ID
                seller = "0x987654321fedcba987654321fedcba987654321f",
                buyer = connectedAccount,
                price = price,
                status = 1, // Sold
                createdAt = DateTimeOffset.UtcNow.AddDays(-1).ToUnixTimeSeconds(),
                updatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                characterData = new NFTCharacterData
                {
                    tokenId = "102",
                    owner = connectedAccount, // Now owned by the buyer
                    name = "Master Mage",
                    level = 10,
                    strength = 5,
                    agility = 8,
                    intelligence = 18,
                    imageURI = "https://example.com/master-mage.png"
                }
            };
            
            OnListingSold?.Invoke(listing);
            
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to buy listing: {e.Message}");
            return false;
        }
    }
    
    #endregion
    
    #region Helper Methods
    
    public string FormatPrice(string weiPrice)
    {
        try
        {
            // Convert wei to ether
            decimal ethPrice = decimal.Parse(weiPrice) / 1000000000000000000m;
            return $"{ethPrice:0.###} ETH";
        }
        catch
        {
            return "? ETH";
        }
    }
    
    public string ConvertEthToWei(string ethPrice)
    {
        try
        {
            // Convert ether to wei
            decimal eth = decimal.Parse(ethPrice);
            decimal wei = eth * 1000000000000000000m;
            return wei.ToString("0");
        }
        catch
        {
            return "0";
        }
    }
    
    #endregion
}
