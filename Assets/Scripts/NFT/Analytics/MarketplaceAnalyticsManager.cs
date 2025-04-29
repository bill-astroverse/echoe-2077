using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;

public class MarketplaceAnalyticsManager : MonoBehaviour
{
    public static MarketplaceAnalyticsManager Instance { get; private set; }
    
    // Data storage
    private List<MarketplaceTransaction> allTransactions = new List<MarketplaceTransaction>();
    private Dictionary<string, CharacterPriceHistory> priceHistoryByToken = new Dictionary<string, CharacterPriceHistory>();
    private GlobalMarketStats globalStats = new GlobalMarketStats();
    
    // Cache settings
    [SerializeField] private bool usePlayerPrefsStorage = true;
    [SerializeField] private int maxTransactionsToStore = 1000;
    
    // Events
    public event Action<List<MarketplaceTransaction>> OnTransactionsUpdated;
    public event Action<GlobalMarketStats> OnGlobalStatsUpdated;
    public event Action<string, CharacterPriceHistory> OnCharacterPriceHistoryUpdated;
    
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
        // Load data from storage
        LoadData();
        
        // Subscribe to marketplace events
        if (MarketplaceManager.Instance != null)
        {
            MarketplaceManager.Instance.OnListingCreated += HandleListingCreated;
            MarketplaceManager.Instance.OnListingUpdated += HandleListingUpdated;
            MarketplaceManager.Instance.OnListingSold += HandleListingSold;
            MarketplaceManager.Instance.OnListingCancelled += HandleListingCancelled;
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from marketplace events
        if (MarketplaceManager.Instance != null)
        {
            MarketplaceManager.Instance.OnListingCreated -= HandleListingCreated;
            MarketplaceManager.Instance.OnListingUpdated -= HandleListingUpdated;
            MarketplaceManager.Instance.OnListingSold -= HandleListingSold;
            MarketplaceManager.Instance.OnListingCancelled -= HandleListingCancelled;
        }
        
        // Save data before destroying
        SaveData();
    }
    
    #region Event Handlers
    
    private void HandleListingCreated(MarketplaceListing listing)
    {
        // Create transaction record
        var transaction = new MarketplaceTransaction
        {
            transactionId = Guid.NewGuid().ToString(),
            tokenId = listing.tokenId,
            characterName = listing.characterData.name,
            seller = listing.seller,
            buyer = "",
            price = listing.price,
            type = MarketplaceTransaction.TransactionType.Listing,
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            characterLevel = listing.characterData.level,
            characterStats = new Dictionary<string, int>
            {
                { "strength", listing.characterData.strength },
                { "agility", listing.characterData.agility },
                { "intelligence", listing.characterData.intelligence }
            }
        };
        
        // Add to transactions
        AddTransaction(transaction);
        
        // Add to price history
        AddPricePoint(listing.tokenId, listing.characterData.name, listing.price);
    }
    
    private void HandleListingUpdated(MarketplaceListing listing)
    {
        // Create transaction record
        var transaction = new MarketplaceTransaction
        {
            transactionId = Guid.NewGuid().ToString(),
            tokenId = listing.tokenId,
            characterName = listing.characterData.name,
            seller = listing.seller,
            buyer = "",
            price = listing.price,
            type = MarketplaceTransaction.TransactionType.PriceUpdate,
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            characterLevel = listing.characterData.level,
            characterStats = new Dictionary<string, int>
            {
                { "strength", listing.characterData.strength },
                { "agility", listing.characterData.agility },
                { "intelligence", listing.characterData.intelligence }
            }
        };
        
        // Add to transactions
        AddTransaction(transaction);
        
        // Add to price history
        AddPricePoint(listing.tokenId, listing.characterData.name, listing.price);
    }
    
    private void HandleListingSold(MarketplaceListing listing)
    {
        // Create transaction record
        var transaction = new MarketplaceTransaction
        {
            transactionId = Guid.NewGuid().ToString(),
            tokenId = listing.tokenId,
            characterName = listing.characterData.name,
            seller = listing.seller,
            buyer = listing.buyer,
            price = listing.price,
            type = MarketplaceTransaction.TransactionType.Sale,
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            characterLevel = listing.characterData.level,
            characterStats = new Dictionary<string, int>
            {
                { "strength", listing.characterData.strength },
                { "agility", listing.characterData.agility },
                { "intelligence", listing.characterData.intelligence }
            }
        };
        
        // Add to transactions
        AddTransaction(transaction);
        
        // Add to sales history
        AddSaleRecord(listing.tokenId, listing.characterData.name, transaction);
    }
    
    private void HandleListingCancelled(MarketplaceListing listing)
    {
        // Create transaction record
        var transaction = new MarketplaceTransaction
        {
            transactionId = Guid.NewGuid().ToString(),
            tokenId = listing.tokenId,
            characterName = listing.characterData.name,
            seller = listing.seller,
            buyer = "",
            price = listing.price,
            type = MarketplaceTransaction.TransactionType.Cancellation,
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            characterLevel = listing.characterData.level,
            characterStats = new Dictionary<string, int>
            {
                { "strength", listing.characterData.strength },
                { "agility", listing.characterData.agility },
                { "intelligence", listing.characterData.intelligence }
            }
        };
        
        // Add to transactions
        AddTransaction(transaction);
    }
    
    #endregion
    
    #region Data Management
    
    private void AddTransaction(MarketplaceTransaction transaction)
    {
        // Add to list
        allTransactions.Add(transaction);
        
        // Trim list if needed
        if (allTransactions.Count > maxTransactionsToStore)
        {
            allTransactions = allTransactions
                .OrderByDescending(t => t.timestamp)
                .Take(maxTransactionsToStore)
                .ToList();
        }
        
        // Update global stats
        globalStats.UpdateStats(allTransactions);
        
        // Save data
        SaveData();
        
        // Notify listeners
        OnTransactionsUpdated?.Invoke(allTransactions);
        OnGlobalStatsUpdated?.Invoke(globalStats);
    }
    
    private void AddPricePoint(string tokenId, string characterName, string price)
    {
        // Create price point
        var pricePoint = new PricePoint
        {
            tokenId = tokenId,
            price = price,
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };
        
        // Add to price history
        if (!priceHistoryByToken.ContainsKey(tokenId))
        {
            priceHistoryByToken[tokenId] = new CharacterPriceHistory
            {
                tokenId = tokenId,
                characterName = characterName
            };
        }
        
        priceHistoryByToken[tokenId].pricePoints.Add(pricePoint);
        
        // Notify listeners
        OnCharacterPriceHistoryUpdated?.Invoke(tokenId, priceHistoryByToken[tokenId]);
    }
    
    private void AddSaleRecord(string tokenId, string characterName, MarketplaceTransaction transaction)
    {
        // Add to sales history
        if (!priceHistoryByToken.ContainsKey(tokenId))
        {
            priceHistoryByToken[tokenId] = new CharacterPriceHistory
            {
                tokenId = tokenId,
                characterName = characterName
            };
        }
        
        priceHistoryByToken[tokenId].salesHistory.Add(transaction);
        priceHistoryByToken[tokenId].UpdateAnalytics();
        
        // Notify listeners
        OnCharacterPriceHistoryUpdated?.Invoke(tokenId, priceHistoryByToken[tokenId]);
    }
    
    private void SaveData()
    {
        if (usePlayerPrefsStorage)
        {
            // Save transactions
            string transactionsJson = JsonConvert.SerializeObject(allTransactions);
            PlayerPrefs.SetString("MarketplaceTransactions", transactionsJson);
            
            // Save price history
            string priceHistoryJson = JsonConvert.SerializeObject(priceHistoryByToken);
            PlayerPrefs.SetString("MarketplacePriceHistory", priceHistoryJson);
            
            // Save global stats
            string globalStatsJson = JsonConvert.SerializeObject(globalStats);
            PlayerPrefs.SetString("MarketplaceGlobalStats", globalStatsJson);
            
            PlayerPrefs.Save();
        }
        else
        {
            // In a real implementation, you might save to a file or a database
            Debug.Log("Data saving is disabled. Would save to file or database in a real implementation.");
        }
    }
    
    private void LoadData()
    {
        if (usePlayerPrefsStorage)
        {
            // Load transactions
            if (PlayerPrefs.HasKey("MarketplaceTransactions"))
            {
                string transactionsJson = PlayerPrefs.GetString("MarketplaceTransactions");
                allTransactions = JsonConvert.DeserializeObject<List<MarketplaceTransaction>>(transactionsJson);
            }
            
            // Load price history
            if (PlayerPrefs.HasKey("MarketplacePriceHistory"))
            {
                string priceHistoryJson = PlayerPrefs.GetString("MarketplacePriceHistory");
                priceHistoryByToken = JsonConvert.DeserializeObject<Dictionary<string, CharacterPriceHistory>>(priceHistoryJson);
            }
            
            // Load global stats
            if (PlayerPrefs.HasKey("MarketplaceGlobalStats"))
            {
                string globalStatsJson = PlayerPrefs.GetString("MarketplaceGlobalStats");
                globalStats = JsonConvert.DeserializeObject<GlobalMarketStats>(globalStatsJson);
            }
        }
        else
        {
            // In a real implementation, you might load from a file or a database
            Debug.Log("Data loading is disabled. Would load from file or database in a real implementation.");
        }
        
        // Notify listeners
        OnTransactionsUpdated?.Invoke(allTransactions);
        OnGlobalStatsUpdated?.Invoke(globalStats);
    }
    
    #endregion
    
    #region Public API
    
    public List<MarketplaceTransaction> GetAllTransactions()
    {
        return new List<MarketplaceTransaction>(allTransactions);
    }
    
    public List<MarketplaceTransaction> GetTransactionsByType(MarketplaceTransaction.TransactionType type)
    {
        return allTransactions.Where(t => t.type == type).ToList();
    }
    
    public List<MarketplaceTransaction> GetTransactionsByToken(string tokenId)
    {
        return allTransactions.Where(t => t.tokenId == tokenId).ToList();
    }
    
    public List<MarketplaceTransaction> GetTransactionsByUser(string address, bool asSeller = true, bool asBuyer = true)
    {
        return allTransactions.Where(t => 
            (asSeller && t.seller.ToLower() == address.ToLower()) || 
            (asBuyer && t.buyer.ToLower() == address.ToLower())
        ).ToList();
    }
    
    public CharacterPriceHistory GetPriceHistory(string tokenId)
    {
        if (priceHistoryByToken.ContainsKey(tokenId))
        {
            return priceHistoryByToken[tokenId];
        }
        
        return null;
    }
    
    public GlobalMarketStats GetGlobalStats()
    {
        return globalStats;
    }
    
    public List<KeyValuePair<string, CharacterPriceHistory>> GetTopSellingCharacters(int count = 5)
    {
        return priceHistoryByToken
            .OrderByDescending(kv => kv.Value.totalSales)
            .Take(count)
            .ToList();
    }
    
    public List<KeyValuePair<string, CharacterPriceHistory>> GetMostValuableCharacters(int count = 5)
    {
        return priceHistoryByToken
            .OrderByDescending(kv => BigInteger.Parse(kv.Value.highestPrice))
            .Take(count)
            .ToList();
    }
    
    public Dictionary<RarityTier, int> GetSalesByRarity()
    {
        Dictionary<RarityTier, int> salesByRarity = new Dictionary<RarityTier, int>();
        
        foreach (var tx in allTransactions)
        {
            if (tx.type == MarketplaceTransaction.TransactionType.Sale && 
                tx.characterStats.ContainsKey("rarity") && 
                Enum.TryParse<RarityTier>(tx.characterStats["rarity"], out RarityTier rarity))
            {
                if (salesByRarity.ContainsKey(rarity))
                {
                    salesByRarity[rarity]++;
                }
                else
                {
                    salesByRarity[rarity] = 1;
                }
            }
        }
        
        return salesByRarity;
    }

    public Dictionary<RarityTier, string> GetAveragePriceByRarity()
    {
        Dictionary<RarityTier, List<string>> pricesByRarity = new Dictionary<RarityTier, List<string>>();
        Dictionary<RarityTier, string> avgPriceByRarity = new Dictionary<RarityTier, string>();
        
        foreach (var tx in allTransactions)
        {
            if (tx.type == MarketplaceTransaction.TransactionType.Sale && 
                tx.characterStats.ContainsKey("rarity") && 
                Enum.TryParse<RarityTier>(tx.characterStats["rarity"], out RarityTier rarity))
            {
                if (!pricesByRarity.ContainsKey(rarity))
                {
                    pricesByRarity[rarity] = new List<string>();
                }
                
                pricesByRarity[rarity].Add(tx.price);
            }
        }
        
        foreach (var entry in pricesByRarity)
        {
            if (entry.Value.Count > 0)
            {
                BigInteger total = BigInteger.Parse("0");
                foreach (var price in entry.Value)
                {
                    total += BigInteger.Parse(price);
                }
                
                string avgPrice = (total / entry.Value.Count).ToString();
                avgPriceByRarity[entry.Key] = avgPrice;
            }
        }
        
        return avgPriceByRarity;
    }
    
    #endregion
}
