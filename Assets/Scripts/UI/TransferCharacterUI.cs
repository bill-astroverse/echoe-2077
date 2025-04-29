using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TransferCharacterUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private TMP_InputField addressInput;
    [SerializeField] private Button transferButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private GameObject loadingPanel;
    
    private NFTCharacter character;
    
    private void Awake()
    {
        transferButton.onClick.AddListener(OnTransferClicked);
        cancelButton.onClick.AddListener(OnCancelClicked);
        
        // Hide loading panel
        loadingPanel.SetActive(false);
    }
    
    public void SetCharacter(NFTCharacter nftCharacter)
    {
        character = nftCharacter;
        characterNameText.text = $"Transfer {character.characterData.name}";
    }
    
    private async void OnTransferClicked()
    {
        string toAddress = addressInput.text;
        
        if (string.IsNullOrEmpty(toAddress) || !toAddress.StartsWith("0x"))
        {
            Debug.LogError("Invalid Ethereum address");
            return;
        }
        
        // Show loading panel
        loadingPanel.SetActive(true);
        
        // Transfer the character
        bool success = await Web3Manager.Instance.TransferCharacter(character.characterData.tokenId, toAddress);
        
        // Hide loading panel
        loadingPanel.SetActive(false);
        
        if (success)
        {
            // Close the dialog
            gameObject.SetActive(false);
            
            // Refresh the inventory
            NFTInventoryUI inventoryUI = FindObjectOfType<NFTInventoryUI>();
            if (inventoryUI != null)
            {
                GameEvents.OnOpenNFTInventory?.Invoke();
            }
        }
        else
        {
            // Show error message
            Debug.LogError("Failed to transfer character");
        }
    }
    
    private void OnCancelClicked()
    {
        gameObject.SetActive(false);
    }
}
