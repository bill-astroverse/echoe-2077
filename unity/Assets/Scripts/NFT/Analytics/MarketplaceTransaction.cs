using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MarketplaceTransaction
{
    public string transactionId;
    public string tokenId;
    public string characterName;
    public string seller;
    public string buyer;
    public string price; // In wei
    public TransactionType type;
    public long timestamp;
    
    // Additional metadata
    public int characterLevel;
    public Dictionary<string, int> characterStats;
    
    public enum TransactionType
    {
        Listing,
        Sale,
        PriceUpdate,
        Cancellation
    }
}

[Serializable]
public class PricePoint
{
    public string tokenId;
    public string price; // In wei
    public long timestamp;
}

[Serializable]
public class CharacterPriceHistory
{
    public string tokenId;
    public string characterName;
    public List<PricePoint> pricePoints = new List<PricePoint>();
    public List<MarketplaceTransaction> salesHistory = new List<MarketplaceTransaction>();
    
    // Analytics data
    public string highestPrice = "0";
    public string lowestPrice = "0";
    public string averagePrice = "0";
    public int totalSales = 0;
    
    public void UpdateAnalytics()
    {
        if (salesHistory.Count == 0)
        {
            return;
        }
        
        // Calculate analytics
        BigInteger highest = BigInteger.Parse("0");
        BigInteger lowest = BigInteger.Parse(salesHistory[0].price);
        BigInteger total = BigInteger.Parse("0");
        
        foreach (var sale in salesHistory)
        {
            BigInteger salePrice = BigInteger.Parse(sale.price);
            
            if (salePrice > highest)
            {
                highest = salePrice;
            }
            
            if (salePrice < lowest)
            {
                lowest = salePrice;
            }
            
            total += salePrice;
        }
        
        highestPrice = highest.ToString();
        lowestPrice = lowest.ToString();
        averagePrice = (total / salesHistory.Count).ToString();
        totalSales = salesHistory.Count;
    }
}

[Serializable]
public class GlobalMarketStats
{
    public int totalTransactions = 0;
    public int totalSales = 0;
    public string totalVolume = "0"; // In wei
    public string averagePrice = "0"; // In wei
    public Dictionary<string, int> salesByDay = new Dictionary<string, int>();
    public Dictionary<int, int> salesByLevel = new Dictionary<int, int>();
    
    public void UpdateStats(List<MarketplaceTransaction> transactions)
    {
        if (transactions.Count == 0)
        {
            return;
        }
        
        totalTransactions = transactions.Count;
        
        // Reset counters
        totalSales = 0;
        BigInteger volume = BigInteger.Parse("0");
        salesByDay.Clear();
        salesByLevel.Clear();
        
        foreach (var tx in transactions)
        {
            if (tx.type == MarketplaceTransaction.TransactionType.Sale)
            {
                totalSales++;
                
                // Add to volume
                BigInteger txPrice = BigInteger.Parse(tx.price);
                volume += txPrice;
                
                // Add to sales by day
                string day = DateTimeOffset.FromUnixTimeSeconds(tx.timestamp).ToString("yyyy-MM-dd");
                if (salesByDay.ContainsKey(day))
                {
                    salesByDay[day]++;
                }
                else
                {
                    salesByDay[day] = 1;
                }
                
                // Add to sales by level
                if (salesByLevel.ContainsKey(tx.characterLevel))
                {
                    salesByLevel[tx.characterLevel]++;
                }
                else
                {
                    salesByLevel[tx.characterLevel] = 1;
                }
            }
        }
        
        totalVolume = volume.ToString();
        
        if (totalSales > 0)
        {
            averagePrice = (volume / totalSales).ToString();
        }
    }
}
