using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TopCharacterCardUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private TextMeshProUGUI valueText;
    [SerializeField] private Button viewDetailsButton;
    
    private string tokenId;
    
    public enum CardType
    {
        TopSelling,
        MostValuable
    }
    
    private void Awake()
    {
        viewDetailsButton.onClick.AddListener(OnViewDetailsClicked);
    }
    
    public void SetCharacter(string characterTokenId, CharacterPriceHistory history, CardType type)
    {
        tokenId = characterTokenId;
        characterNameText.text = history.characterName;
        
        if (type == CardType.TopSelling)
        {
            statsText.text = $"Total Sales: {history.totalSales}";
            valueText.text = $"Avg: {MarketplaceManager.Instance.FormatPrice(history.averagePrice)}";
        }
        else // MostValuable
        {
            statsText.text = $"Highest Price";
            valueText.text = MarketplaceManager.Instance.FormatPrice(history.highestPrice);
        }
    }
    
    private void OnViewDetailsClicked()
    {
        // Find character in dropdown and select it
        MarketplaceAnalyticsUI analyticsUI = GetComponentInParent<MarketplaceAnalyticsUI>();
        if (analyticsUI != null)
        {
            TMP_Dropdown dropdown = analyticsUI.GetComponentInChildren<TMP_Dropdown>();
            if (dropdown != null)
            {
                for (int i = 0; i < dropdown.options.Count; i++)
                {
                    if (dropdown.options[i].text.Contains($"#{tokenId})"))
                    {
                        dropdown.value = i;
                        dropdown.RefreshShownValue();
                        
                        // Switch to character tab
                        TabGroup tabGroup = analyticsUI.GetComponentInChildren<TabGroup>();
                        if (tabGroup != null)
                        {
                            tabGroup.SelectTab(1); // Assuming Character tab is index 1
                        }
                        
                        break;
                    }
                }
            }
        }
    }
}
