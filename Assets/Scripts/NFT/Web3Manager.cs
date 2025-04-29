using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using UnityEngine;
using Nethereum.Web3;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Web3.Accounts;
using Newtonsoft.Json;

public class Web3Manager : MonoBehaviour
{
    public static Web3Manager Instance { get; private set; }
    
    [Header("Blockchain Settings")]
    [SerializeField] private string rpcUrl = "https://polygon-rpc.com/";
    [SerializeField] private string contractAddress = "0x0000000000000000000000000000000000000000"; // Replace with your NFT contract address
    [SerializeField] private string contractABI = ""; // Your contract ABI goes here
    
    private Web3 web3;
    private Contract nftContract;
    private string connectedAccount;
    private bool isInitialized = false;
    
    // Events
    public event Action<string> OnWalletConnected;
    public event Action<string> OnWalletDisconnected;
    public event Action<List<NFTCharacterData>> OnCharactersLoaded;
    public event Action<NFTCharacterData> OnCharacterUpdated;
    
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
            web3 = new Web3(rpcUrl);
            
            if (!string.IsNullOrEmpty(contractAddress) && !string.IsNullOrEmpty(contractABI))
            {
                nftContract = web3.Eth.GetContract(contractABI, contractAddress);
                isInitialized = true;
                Debug.Log("Web3Manager initialized successfully");
            }
            else
            {
                Debug.LogError("Contract address or ABI is missing");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to initialize Web3Manager: {e.Message}");
        }
    }
    
    // Add this method to the Web3Manager class to expose the NFT contract address

    public string GetWeb3()
    {
        return web3;
    }

    public string GetNFTContractAddress()
    {
        return contractAddress;
    }
    
    #region Wallet Connection
    
    public async Task<bool> ConnectWallet()
    {
        try
        {
            // In a real implementation, this would connect to MetaMask or WalletConnect
            // For this example, we'll simulate a connection
            
            // Simulating wallet connection
            connectedAccount = "0x742d35Cc6634C0532925a3b844Bc454e4438f44e"; // Example wallet address
            
            OnWalletConnected?.Invoke(connectedAccount);
            
            // Load the player's characters after connecting
            await LoadPlayerCharacters();
            
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to connect wallet: {e.Message}");
            return false;
        }
    }
    
    public void DisconnectWallet()
    {
        connectedAccount = null;
        OnWalletDisconnected?.Invoke(string.Empty);
    }
    
    public bool IsWalletConnected()
    {
        return !string.IsNullOrEmpty(connectedAccount);
    }
    
    public string GetConnectedAccount()
    {
        return connectedAccount;
    }
    
    #endregion
    
    #region NFT Character Operations
    
    public async Task<List<NFTCharacterData>> LoadPlayerCharacters()
    {
        if (!isInitialized || !IsWalletConnected())
        {
            Debug.LogError("Web3Manager not initialized or wallet not connected");
            return new List<NFTCharacterData>();
        }
        
        try
        {
            // Call the contract to get the player's characters
            // This is a simplified example - in a real implementation, you would call your contract's function
            
            // For this example, we'll create some mock data
            var characters = new List<NFTCharacterData>();
            
            // Mock character 1 - Common
            characters.Add(new NFTCharacterData
            {
                tokenId = "1",
                owner = connectedAccount,
                name = "Warrior",
                level = 5,
                strength = 10,
                agility = 7,
                intelligence = 4,
                imageURI = "https://example.com/warrior.png",
                attributes = new Dictionary<string, string>
                {
                    { "rarity", "Common" },
                    { "class", "warrior" },
                    { "weapon", "sword" },
                    { "armor", "plate" }
                }
            });
            
            // Mock character 2 - Rare
            characters.Add(new NFTCharacterData
            {
                tokenId = "2",
                owner = connectedAccount,
                name = "Mage",
                level = 4,
                strength = 3,
                agility = 6,
                intelligence = 12,
                imageURI = "https://example.com/mage.png",
                attributes = new Dictionary<string, string>
                {
                    { "rarity", "Rare" },
                    { "class", "mage" },
                    { "weapon", "staff" },
                    { "armor", "cloth" },
                    { "element", "fire" },
                    { "special_ability", "teleport" }
                }
            });
            
            // Mock character 3 - Epic
            characters.Add(new NFTCharacterData
            {
                tokenId = "3",
                owner = connectedAccount,
                name = "Shadow Assassin",
                level = 7,
                strength = 8,
                agility = 14,
                intelligence = 9,
                imageURI = "https://example.com/assassin.png",
                attributes = new Dictionary<string, string>
                {
                    { "rarity", "Epic" },
                    { "class", "assassin" },
                    { "weapon", "dagger" },
                    { "armor", "leather" },
                    { "element", "shadow" },
                    { "special_ability", "invisibility" },
                    { "origin", "underworld" }
                }
            });
            
            OnCharactersLoaded?.Invoke(characters);
            return characters;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load player characters: {e.Message}");
            return new List<NFTCharacterData>();
        }
    }
    
    // Update the MintCharacter method to use the metadata generator
    public async Task<bool> MintCharacter(string characterName, int strength, int agility, int intelligence, string imageURI, Dictionary<string, string> specialTraits = null)
    {
        if (!isInitialized || !IsWalletConnected())
        {
            Debug.LogError("Web3Manager not initialized or wallet not connected");
            return false;
        }
        
        try
        {
            // In a real implementation, this would call your contract's mint function
            // For this example, we'll simulate a successful mint
            
            Debug.Log($"Minting character: {characterName}");
            
            // Simulate blockchain delay
            await Task.Delay(2000);
            
            // Create a new character with a random token ID
            string tokenId = UnityEngine.Random.Range(1000, 9999).ToString();
            
            var newCharacter = new NFTCharacterData
            {
                tokenId = tokenId,
                owner = connectedAccount,
                name = characterName,
                level = 1,
                strength = strength,
                agility = agility,
                intelligence = intelligence,
                imageURI = imageURI,
                attributes = new Dictionary<string, string>
                {
                    { "class", "custom" },
                    { "created", DateTime.Now.ToString() }
                }
            };
            
            // Add special traits if provided
            if (specialTraits != null)
            {
                foreach (var trait in specialTraits)
                {
                    newCharacter.attributes[trait.Key] = trait.Value;
                }
            }
            
            // If rarity is not specified, determine it from stats
            if (!newCharacter.attributes.ContainsKey("rarity"))
            {
                RarityTier rarity = RaritySystem.DetermineRarity(strength, agility, intelligence);
                newCharacter.attributes["rarity"] = rarity.ToString();
            }
            
            // Generate and upload metadata
            string metadataUrl = await NFTMetadataGenerator.Instance.GenerateAndUploadMetadataAsync(newCharacter);
            
            // In a real implementation, you would pass this metadata URL to your contract
            Debug.Log($"Character metadata uploaded to: {metadataUrl}");
            
            OnCharacterUpdated?.Invoke(newCharacter);
            
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to mint character: {e.Message}");
            return false;
        }
    }
    
    public async Task<bool> TransferCharacter(string tokenId, string toAddress)
    {
        if (!isInitialized || !IsWalletConnected())
        {
            Debug.LogError("Web3Manager not initialized or wallet not connected");
            return false;
        }
        
        try
        {
            // In a real implementation, this would call your contract's transfer function
            // For this example, we'll simulate a successful transfer
            
            Debug.Log($"Transferring character {tokenId} to {toAddress}");
            
            // Simulate blockchain delay
            await Task.Delay(2000);
            
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to transfer character: {e.Message}");
            return false;
        }
    }
    
    public void UpdateCharacterOnChain(NFTCharacterData characterData)
    {
        if (!isInitialized || !IsWalletConnected())
        {
            Debug.LogError("Web3Manager not initialized or wallet not connected");
            return;
        }
        
        StartCoroutine(UpdateCharacterCoroutine(characterData));
    }
    
    private IEnumerator UpdateCharacterCoroutine(NFTCharacterData characterData)
    {
        Debug.Log($"Updating character {characterData.tokenId} on blockchain");
        
        // Simulate blockchain delay
        yield return new WaitForSeconds(1.5f);
        
        // In a real implementation, this would call your contract's update function
        
        OnCharacterUpdated?.Invoke(characterData);
        
        Debug.Log($"Character {characterData.tokenId} updated successfully");
    }
    
    #endregion
}
