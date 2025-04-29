using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RarityStatUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI rarityNameText;
    [SerializeField] private TextMeshProUGUI salesCountText;
    [SerializeField] private TextMeshProUGUI avgPriceText;
    [SerializeField] private Image rarityIcon;
    
    public void SetRarityStat(RarityTier rarity, int salesCount, string avgPrice)
    {
        rarityNameText.text = RaritySystem.GetRarityName(rarity);
        rarityNameText.color = RaritySystem.GetRarityColor(rarity);
        rarityIcon.color = RaritySystem.GetRarityColor(rarity);
        
        salesCountText.text = $"Sales: {salesCount}";
        avgPriceText.text = $"Avg: {avgPrice}";
    }
}
