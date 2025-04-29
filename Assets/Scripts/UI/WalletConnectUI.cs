using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WalletConnectUI : MonoBehaviour
{
    [SerializeField] private Button connectButton;
    [SerializeField] private Button disconnectButton;
    [SerializeField] private TextMeshProUGUI walletAddressText;
    [SerializeField] private GameObject connectPanel;
    [SerializeField] private GameObject walletPanel;
    
    private void Start()
    {
        connectButton.onClick.AddListener(OnConnectClicked);
        disconnectButton.onClick.AddListener(OnDisconnectClicked);
        
        // Subscribe to wallet events
        Web3Manager.Instance.OnWalletConnected += HandleWalletConnected;
        Web3Manager.Instance.OnWalletDisconnected += HandleWalletDisconnected;
        
        // Initialize UI based on current wallet state
        if (Web3Manager.Instance.IsWalletConnected())
        {
            HandleWalletConnected(Web3Manager.Instance.GetConnectedAccount());
        }
        else
        {
            HandleWalletDisconnected(string.Empty);
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from wallet events
        if (Web3Manager.Instance != null)
        {
            Web3Manager.Instance.OnWalletConnected -= HandleWalletConnected;
            Web3Manager.Instance.OnWalletDisconnected -= HandleWalletDisconnected;
        }
    }
    
    private async void OnConnectClicked()
    {
        bool success = await Web3Manager.Instance.ConnectWallet();
        
        if (!success)
        {
            Debug.LogError("Failed to connect wallet");
        }
    }
    
    private void OnDisconnectClicked()
    {
        Web3Manager.Instance.DisconnectWallet();
    }
    
    private void HandleWalletConnected(string address)
    {
        connectPanel.SetActive(false);
        walletPanel.SetActive(true);
        
        // Format address for display (0x1234...5678)
        string formattedAddress = address;
        if (address.Length > 10)
        {
            formattedAddress = $"{address.Substring(0, 6)}...{address.Substring(address.Length - 4)}";
        }
        
        walletAddressText.text = formattedAddress;
    }
    
    private void HandleWalletDisconnected(string _)
    {
        connectPanel.SetActive(true);
        walletPanel.SetActive(false);
        walletAddressText.text = "";
    }
}
