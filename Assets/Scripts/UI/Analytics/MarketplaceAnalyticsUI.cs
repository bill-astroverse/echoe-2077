using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MarketplaceAnalyticsUI : MonoBehaviour
{
    [SerializeField] private GameObject analyticsPanel;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button refreshButton;
    [SerializeField] private TabGroup tabGroup;
    
    [Header("Global Stats")]
    [SerializeField] private TextMeshProUGUI totalTransactionsText;
    [SerializeField] private TextMeshProUGUI totalSalesText;
    [SerializeField] private TextMeshProUGUI totalVolumeText;
    [SerializeField] private TextMeshProUGUI averagePriceText;
    [SerializeField] private Transform salesChartContainer;
    [SerializeField] private GameObject salesChartBarPrefab;
    
    [Header("Character Stats")]
    [SerializeField] private TMP_Dropdown characterDropdown;
    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private TextMeshProUGUI highestPriceText;
    [SerializeField] private TextMeshProUGUI lowestPriceText;
    [SerializeField] private TextMeshProUGUI averageCharacterPriceText;
    [SerializeField] private TextMeshProUGUI totalCharacterSalesText;
    [SerializeField] private Transform priceHistoryChartContainer;
    [SerializeField] private GameObject priceChartPointPrefab;
    
    [Header("Top Characters")]
    [SerializeField] private Transform topSellingContainer;
    [SerializeField] private Transform mostValuableContainer;
    [SerializeField] private GameObject topCharacterCardPrefab;
    
    [Header("My Activity")]
    [SerializeField] private Transform myListingsContainer;
    [SerializeField] private Transform myPurchasesContainer;
    [SerializeField] private GameObject activityCardPrefab;
    
    [Header("Rarity Stats")]
    [SerializeField] private Transform rarityStatsContainer;
    [SerializeField] private GameObject rarityStatPrefab;
    
    private List<GameObject> spawnedChartBars = new List<GameObject>();
    private List<GameObject> spawnedPricePoints = new List<GameObject>();
    private List<GameObject> spawnedTopCards = new List<GameObject>();
    private List<GameObject> spawnedActivityCards = new List<GameObject>();
    
    private void Awake()
    {
        closeButton.onClick.AddListener(CloseAnalytics);
        refreshButton.onClick.AddListener(RefreshData);
        characterDropdown.onValueChanged.AddListener(OnCharacterSelected);
        
        // Hide analytics panel at start
        analyticsPanel.SetActive(false);
    }
    
    private void Start()
    {
        // Subscribe to events
        GameEvents.OnOpenMarketplaceAnalytics += OpenAnalytics;
        GameEvents.OnCloseMarketplaceAnalytics += CloseAnalytics;
        
        if (MarketplaceAnalyticsManager.Instance != null)
        {
            MarketplaceAnalyticsManager.Instance.OnGlobalStatsUpdated += UpdateGlobalStats;
            MarketplaceAnalyticsManager.Instance.OnCharacterPriceHistoryUpdated += UpdateCharacterPriceHistory;
            MarketplaceAnalyticsManager.Instance.OnTransactionsUpdated += UpdateMyActivity;
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        GameEvents.OnOpenMarketplaceAnalytics -= OpenAnalytics;
        GameEvents.OnCloseMarketplaceAnalytics -= CloseAnalytics;
        
        if (MarketplaceAnalyticsManager.Instance != null)
        {
            MarketplaceAnalyticsManager.Instance.OnGlobalStatsUpdated -= UpdateGlobalStats;
            MarketplaceAnalyticsManager.Instance.OnCharacterPriceHistoryUpdated -= UpdateCharacterPriceHistory;
            MarketplaceAnalyticsManager.Instance.OnTransactionsUpdated -= UpdateMyActivity;
        }
    }
    
    private void OpenAnalytics()
    {
        analyticsPanel.SetActive(true);
        RefreshData();
    }
    
    private void CloseAnalytics()
    {
        analyticsPanel.SetActive(false);
    }
    
    private void RefreshData()
    {
        if (MarketplaceAnalyticsManager.Instance == null)
        {
            Debug.LogError("MarketplaceAnalyticsManager not found");
            return;
        }
        
        // Update global stats
        UpdateGlobalStats(MarketplaceAnalyticsManager.Instance.GetGlobalStats());
        
        // Update character dropdown
        PopulateCharacterDropdown();
        
        // Update top characters
        UpdateTopCharacters();
        
        // Update my activity
        UpdateMyActivity(MarketplaceAnalyticsManager.Instance.GetAllTransactions());

        UpdateRarityStats();
    }
    
    private void UpdateGlobalStats(GlobalMarketStats stats)
    {
        totalTransactionsText.text = stats.totalTransactions.ToString();
        totalSalesText.text = stats.totalSales.ToString();
        totalVolumeText.text = MarketplaceManager.Instance.FormatPrice(stats.totalVolume);
        averagePriceText.text = MarketplaceManager.Instance.FormatPrice(stats.averagePrice);
        
        // Update sales chart
        UpdateSalesChart(stats.salesByDay);
    }
    
    private void UpdateSalesChart(Dictionary<string, int> salesByDay)
    {
        // Clear existing bars
        foreach (var bar in spawnedChartBars)
        {
            Destroy(bar);
        }
        spawnedChartBars.Clear();
        
        // Get last 7 days
        var last7Days = new List<string>();
        for (int i = 6; i >= 0; i--)
        {
            string day = DateTime.Now.AddDays(-i).ToString("yyyy-MM-dd");
            last7Days.Add(day);
        }
        
        // Create bars for each day
        float maxSales = 1; // Minimum to avoid division by zero
        foreach (var day in last7Days)
        {
            if (salesByDay.ContainsKey(day) && salesByDay[day] > maxSales)
            {
                maxSales = salesByDay[day];
            }
        }
        
        foreach (var day in last7Days)
        {
            int sales = salesByDay.ContainsKey(day) ? salesByDay[day] : 0;
            float height = (sales / maxSales) * 100f; // Scale to 100 max height
            
            GameObject barObj = Instantiate(salesChartBarPrefab, salesChartContainer);
            spawnedChartBars.Add(barObj);
            
            // Set bar height
            RectTransform barRect = barObj.GetComponent<RectTransform>();
            barRect.sizeDelta = new Vector2(barRect.sizeDelta.x, height);
            
            // Set bar label
            TextMeshProUGUI dayLabel = barObj.GetComponentInChildren<TextMeshProUGUI>();
            if (dayLabel != null)
            {
                dayLabel.text = DateTime.Parse(day).ToString("MM/dd");
            }
            
            // Set tooltip
            TooltipTrigger tooltip = barObj.GetComponent<TooltipTrigger>();
            if (tooltip != null)
            {
                tooltip.tooltipText = $"{DateTime.Parse(day).ToString("MMM dd, yyyy")}: {sales} sales";
            }
        }
    }
    
    private void PopulateCharacterDropdown()
    {
        if (MarketplaceAnalyticsManager.Instance == null)
        {
            return;
        }
        
        // Get all characters with price history
        var characters = MarketplaceAnalyticsManager.Instance.GetAllTransactions()
            .Where(t => t.type == MarketplaceTransaction.TransactionType.Sale || t.type == MarketplaceTransaction.TransactionType.Listing)
            .GroupBy(t => t.tokenId)
            .Select(g => new { TokenId = g.Key, Name = g.First().characterName })
            .ToList();
        
        // Clear dropdown
        characterDropdown.ClearOptions();
        
        // Add options
        var options = new List<TMP_Dropdown.OptionData>();
        foreach (var character in characters)
        {
            options.Add(new TMP_Dropdown.OptionData($"{character.Name} (#{character.TokenId})"));
        }
        
        characterDropdown.AddOptions(options);
        
        // Select first character if available
        if (options.Count > 0)
        {
            characterDropdown.value = 0;
            OnCharacterSelected(0);
        }
    }
    
    private void OnCharacterSelected(int index)
    {
        if (MarketplaceAnalyticsManager.Instance == null || characterDropdown.options.Count == 0)
        {
            return;
        }
        
        // Parse token ID from option text
        string optionText = characterDropdown.options[index].text;
        int startIndex = optionText.LastIndexOf("(#") + 2;
        int endIndex = optionText.LastIndexOf(")");
        
        if (startIndex >= 2 && endIndex > startIndex)
        {
            string tokenId = optionText.Substring(startIndex, endIndex - startIndex);
            
            // Get price history
            CharacterPriceHistory history = MarketplaceAnalyticsManager.Instance.GetPriceHistory(tokenId);
            if (history != null)
            {
                UpdateCharacterPriceHistory(tokenId, history);
            }
        }
    }
    
    private void UpdateCharacterPriceHistory(string tokenId, CharacterPriceHistory history)
    {
        characterNameText.text = history.characterName;
        highestPriceText.text = MarketplaceManager.Instance.FormatPrice(history.highestPrice);
        lowestPriceText.text = MarketplaceManager.Instance.FormatPrice(history.lowestPrice);
        averageCharacterPriceText.text = MarketplaceManager.Instance.FormatPrice(history.averagePrice);
        totalCharacterSalesText.text = history.totalSales.ToString();
        
        // Update price history chart
        UpdatePriceHistoryChart(history.pricePoints);
    }
    
    private void UpdatePriceHistoryChart(List<PricePoint> pricePoints)
    {
        // Clear existing points
        foreach (var point in spawnedPricePoints)
        {
            Destroy(point);
        }
        spawnedPricePoints.Clear();
        
        if (pricePoints.Count == 0)
        {
            return;
        }
        
        // Sort by timestamp
        pricePoints = pricePoints.OrderBy(p => p.timestamp).ToList();
        
        // Find min and max price
        BigInteger minPrice = BigInteger.Parse(pricePoints[0].price);
        BigInteger maxPrice = minPrice;
        
        foreach (var point in pricePoints)
        {
            BigInteger price = BigInteger.Parse(point.price);
            if (price < minPrice) minPrice = price;
            if (price > maxPrice) maxPrice = price;
        }
        
        // Ensure min and max are different to avoid division by zero
        if (maxPrice == minPrice)
        {
            maxPrice += 1;
        }
        
        // Calculate chart dimensions
        float chartWidth = priceHistoryChartContainer.GetComponent<RectTransform>().rect.width;
        float chartHeight = priceHistoryChartContainer.GetComponent<RectTransform>().rect.height;
        
        // Calculate time range
        long startTime = pricePoints.First().timestamp;
        long endTime = pricePoints.Last().timestamp;
        long timeRange = endTime - startTime;
        
        // Ensure time range is not zero
        if (timeRange == 0)
        {
            timeRange = 1;
        }
        
        // Create points
        for (int i = 0; i < pricePoints.Count; i++)
        {
            var point = pricePoints[i];
            
            // Calculate position
            float xPos = ((point.timestamp - startTime) / (float)timeRange) * chartWidth;
            
            BigInteger price = BigInteger.Parse(point.price);
            float yPos = ((price - minPrice) / (float)(maxPrice - minPrice)) * chartHeight;
            
            // Create point object
            GameObject pointObj = Instantiate(priceChartPointPrefab, priceHistoryChartContainer);
            spawnedPricePoints.Add(pointObj);
            
            // Position point
            RectTransform pointRect = pointObj.GetComponent<RectTransform>();
            pointRect.anchoredPosition = new Vector2(xPos, yPos);
            
            // Set tooltip
            TooltipTrigger tooltip = pointObj.GetComponent<TooltipTrigger>();
            if (tooltip != null)
            {
                string formattedPrice = MarketplaceManager.Instance.FormatPrice(point.price);
                string formattedDate = DateTimeOffset.FromUnixTimeSeconds(point.timestamp).ToString("MMM dd, yyyy HH:mm");
                tooltip.tooltipText = $"{formattedDate}: {formattedPrice}";
            }
            
            // Connect points with lines if not the first point
            if (i > 0)
            {
                var prevPoint = pricePoints[i - 1];
                float prevXPos = ((prevPoint.timestamp - startTime) / (float)timeRange) * chartWidth;
                BigInteger prevPrice = BigInteger.Parse(prevPoint.price);
                float prevYPos = ((prevPrice - minPrice) / (float)(maxPrice - minPrice)) * chartHeight;
                
                // Create line
                GameObject lineObj = new GameObject("Line");
                lineObj.transform.SetParent(priceHistoryChartContainer, false);
                spawnedPricePoints.Add(lineObj);
                
                LineRenderer line = lineObj.AddComponent<LineRenderer>();
                line.positionCount = 2;
                line.SetPosition(0, new Vector3(prevXPos, prevYPos, 0));
                line.SetPosition(1, new Vector3(xPos, yPos, 0));
                line.startWidth = 2f;
                line.endWidth = 2f;
                line.material = new Material(Shader.Find("Sprites/Default"));
                line.startColor = Color.blue;
                line.endColor = Color.blue;
            }
        }
    }
    
    private void UpdateTopCharacters()
    {
        if (MarketplaceAnalyticsManager.Instance == null)
        {
            return;
        }
        
        // Clear existing cards
        foreach (var card in spawnedTopCards)
        {
            Destroy(card);
        }
        spawnedTopCards.Clear();
        
        // Get top selling characters
        var topSelling = MarketplaceAnalyticsManager.Instance.GetTopSellingCharacters(5);
        foreach (var character in topSelling)
        {
            GameObject cardObj = Instantiate(topCharacterCardPrefab, topSellingContainer);
            spawnedTopCards.Add(cardObj);
            
            TopCharacterCardUI cardUI = cardObj.GetComponent<TopCharacterCardUI>();
            if (cardUI != null)
            {
                cardUI.SetCharacter(character.Key, character.Value, TopCharacterCardUI.CardType.TopSelling);
            }
        }
        
        // Get most valuable characters
        var mostValuable = MarketplaceAnalyticsManager.Instance.GetMostValuableCharacters(5);
        foreach (var character in mostValuable)
        {
            GameObject cardObj = Instantiate(topCharacterCardPrefab, mostValuableContainer);
            spawnedTopCards.Add(cardObj);
            
            TopCharacterCardUI cardUI = cardObj.GetComponent<TopCharacterCardUI>();
            if (cardUI != null)
            {
                cardUI.SetCharacter(character.Key, character.Value, TopCharacterCardUI.CardType.MostValuable);
            }
        }
    }
    
    private void UpdateMyActivity(List<MarketplaceTransaction> transactions)
    {
        if (!Web3Manager.Instance.IsWalletConnected())
        {
            return;
        }
        
        string userAddress = Web3Manager.Instance.GetConnectedAccount();
        
        // Clear existing cards
        foreach (var card in spawnedActivityCards)
        {
            Destroy(card);
        }
        spawnedActivityCards.Clear();
        
        // Get my listings
        var myListings = transactions
            .Where(t => t.seller.ToLower() == userAddress.ToLower() && 
                   (t.type == MarketplaceTransaction.TransactionType.Listing || 
                    t.type == MarketplaceTransaction.TransactionType.PriceUpdate))
            .OrderByDescending(t => t.timestamp)
            .Take(5)
            .ToList();
        
        foreach (var listing in myListings)
        {
            GameObject cardObj = Instantiate(activityCardPrefab, myListingsContainer);
            spawnedActivityCards.Add(cardObj);
            
            ActivityCardUI cardUI = cardObj.GetComponent<ActivityCardUI>();
            if (cardUI != null)
            {
                cardUI.SetTransaction(listing);
            }
        }
        
        // Get my purchases
        var myPurchases = transactions
            .Where(t => t.buyer.ToLower() == userAddress.ToLower() && 
                   t.type == MarketplaceTransaction.TransactionType.Sale)
            .OrderByDescending(t => t.timestamp)
            .Take(5)
            .ToList();
        
        foreach (var purchase in myPurchases)
        {
            GameObject cardObj = Instantiate(activityCardPrefab, myPurchasesContainer);
            spawnedActivityCards.Add(cardObj);
            
            ActivityCardUI cardUI = cardObj.GetComponent<ActivityCardUI>();
            if (cardUI != null)
            {
                cardUI.SetTransaction(purchase);
            }
        }
    }

    private void UpdateRarityStats()
    {
        if (MarketplaceAnalyticsManager.Instance == null)
        {
            return;
        }
        
        // Clear existing stats
        foreach (Transform child in rarityStatsContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Get sales by rarity
        var salesByRarity = MarketplaceAnalyticsManager.Instance.GetSalesByRarity();
        
        // Get average price by rarity
        var avgPriceByRarity = MarketplaceAnalyticsManager.Instance.GetAveragePriceByRarity();
        
        // Create stat items for each rarity
        foreach (RarityTier rarity in Enum.GetValues(typeof(RarityTier)))
        {
            GameObject statObj = Instantiate(rarityStatPrefab, rarityStatsContainer);
            
            // Get components
            TextMeshProUGUI rarityNameText = statObj.transform.Find("RarityName").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI salesCountText = statObj.transform.Find("SalesCount").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI avgPriceText = statObj.transform.Find("AvgPrice").GetComponent<TextMeshProUGUI>();
            Image rarityIcon = statObj.transform.Find("RarityIcon").GetComponent<Image>();
            
            // Set values
            rarityNameText.text = RaritySystem.GetRarityName(rarity);
            rarityNameText.color = RaritySystem.GetRarityColor(rarity);
            rarityIcon.color = RaritySystem.GetRarityColor(rarity);
            
            int salesCount = salesByRarity.ContainsKey(rarity) ? salesByRarity[rarity] : 0;
            salesCountText.text = $"Sales: {salesCount}";
            
            string avgPrice = avgPriceByRarity.ContainsKey(rarity) ? 
                MarketplaceManager.Instance.FormatPrice(avgPriceByRarity[rarity]) : 
                "N/A";
            avgPriceText.text = $"Avg: {avgPrice}";
        }
    }
}
