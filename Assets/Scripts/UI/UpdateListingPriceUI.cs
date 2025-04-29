using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpdateListingPriceUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TMP_InputField priceInput;
    [SerializeField] private Button updateButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private GameObject loadingPanel;
    
    private MarketplaceListing listing;
    
    private void Awake()
    {
        updateButton.onClick.AddListener(OnUpdateClicked);
        cancelButton.onClick.AddListener(OnCancelClicked);
        
        // Hide loading panel
        loadingPanel.SetActive(false);
    }
    
    public void SetListing(MarketplaceListing marketplaceListing)
    {
        listing = marketplaceListing;
        
        // Convert wei to ETH for display
        string ethPrice = MarketplaceManager.Instance.FormatPrice(listing.price);
        ethPrice = ethPrice.Replace(" ETH", ""); // Remove the ETH suffix
        
        titleText.text = $"Update Price - {listing.characterData.name}";
        priceInput.text = ethPrice;
    }
    
    private async void OnUpdateClicked()
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
        
        // Update the listing
        bool success = await MarketplaceManager.Instance.UpdateListing(listing.listingId, weiPrice);
        
        // Hide loading panel
        loadingPanel.SetActive(false);
        
        if (success)
        {
            // Close the dialog
            gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("Failed to update listing price");
        }
    }
    
    private void OnCancelClicked()
    {
        gameObject.SetActive(false);
    }
}
