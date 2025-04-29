using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ActivityCardUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private TextMeshProUGUI transactionTypeText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TextMeshProUGUI dateText;
    [SerializeField] private Image statusIcon;
    [SerializeField] private Color listingColor;
    [SerializeField] private Color saleColor;
    [SerializeField] private Color updateColor;
    [SerializeField] private Color cancelColor;
    
    public void SetTransaction(MarketplaceTransaction transaction)
    {
        characterNameText.text = transaction.characterName;
        priceText.text = MarketplaceManager.Instance.FormatPrice(transaction.price);
        dateText.text = DateTimeOffset.FromUnixTimeSeconds(transaction.timestamp).ToString("MMM dd, yyyy HH:mm");
        
        switch (transaction.type)
        {
            case MarketplaceTransaction.TransactionType.Listing:
                transactionTypeText.text = "Listed";
                statusIcon.color = listingColor;
                break;
            case MarketplaceTransaction.TransactionType.Sale:
                transactionTypeText.text = "Sold";
                statusIcon.color = saleColor;
                break;
            case MarketplaceTransaction.TransactionType.PriceUpdate:
                transactionTypeText.text = "Updated Price";
                statusIcon.color = updateColor;
                break;
            case MarketplaceTransaction.TransactionType.Cancellation:
                transactionTypeText.text = "Cancelled";
                statusIcon.color = cancelColor;
                break;
        }
    }
}
