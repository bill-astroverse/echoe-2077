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

// Helper classes for deserializing deployment info
[Serializable]
public class ContractInfo
{
    public string address;
    public string abi;
}

[Serializable]
public class DeploymentInfo
{
    public ContractInfo GameCharacterNFT;
    public ContractInfo GameCharacterMarketplace;
}


public class Web3Manager : MonoBehaviour
{
    public static Web3Manager Instance { get; private set; }

    [Header("Blockchain Settings")]
    [SerializeField] private string rpcUrl = "http://127.0.0.1:8545";

    private Web3 web3;
    public Contract nftContract;
    public Contract marketplaceContract;

    private string nftContractAddress;
    private string marketplaceContractAddress;

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

            TextAsset deploymentInfoJson = Resources.Load<TextAsset>("deployment-info");
            if (deploymentInfoJson != null)
            {
                // Using Newtonsoft.Json to handle complex ABI structure
                var deploymentInfo = JsonConvert.DeserializeObject<Dictionary<string, ContractInfo>>(deploymentInfoJson.text);

                nftContractAddress = deploymentInfo["GameCharacterNFT"].address;
                string nftContractABI = deploymentInfo["GameCharacterNFT"].abi;
                nftContract = web3.Eth.GetContract(nftContractABI, nftContractAddress);

                marketplaceContractAddress = deploymentInfo["GameCharacterMarketplace"].address;
                string marketplaceContractABI = deploymentInfo["GameCharacterMarketplace"].abi;
                marketplaceContract = web3.Eth.GetContract(marketplaceContractABI, marketplaceContractAddress);

                isInitialized = true;
                Debug.Log("Web3Manager initialized successfully");
            }
            else
            {
                Debug.LogError("deployment-info.json not found in Resources folder.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to initialize Web3Manager: {e.Message}");
        }
    }

    public Web3 GetWeb3()
    {
        return web3;
    }

    public string GetNFTContractAddress()
    {
        return nftContractAddress;
    }

    public string GetMarketplaceContractAddress()
    {
        return marketplaceContractAddress;
    }

    public Contract GetMarketplaceContract()
    {
        return marketplaceContract;
    }

    #region Wallet Connection

    public async Task<bool> ConnectWallet()
    {
        try
        {
            // In a real implementation, this would connect to MetaMask or WalletConnect
            // For now, we'll use a hardcoded account from the Hardhat node
            connectedAccount = "0xf39Fd6e51aad88F6F4ce6aB8827279cffFb92266"; // Default Hardhat account 0

            OnWalletConnected?.Invoke(connectedAccount);

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
            Debug.Log("Loading player characters from blockchain...");
            var characters = new List<NFTCharacterData>();
            var balanceOfFunction = new BalanceOfFunction() { Owner = connectedAccount };
            var balance = await nftContract.GetFunction<BalanceOfFunction>().CallAsync<BigInteger>(balanceOfFunction);

            for (var i = 0; i < balance; i++)
            {
                var tokenOfOwnerByIndexFunction = new TokenOfOwnerByIndexFunction()
                {
                    Owner = connectedAccount,
                    Index = i
                };
                var tokenId = await nftContract.GetFunction<TokenOfOwnerByIndexFunction>().CallAsync<BigInteger>(tokenOfOwnerByIndexFunction);

                var getCharacterStatsFunction = new GetCharacterStatsFunction() { TokenId = tokenId };
                var stats = await nftContract.GetFunction<GetCharacterStatsFunction>().CallDeserializingToObjectAsync<CharacterStatsDTO>(getCharacterStatsFunction);

                var tokenURIFunction = new TokenURIFunction() { TokenId = tokenId };
                var tokenURI = await nftContract.GetFunction<TokenURIFunction>().CallAsync<string>(tokenURIFunction);

                var characterData = new NFTCharacterData()
                {
                    tokenId = tokenId.ToString(),
                    owner = connectedAccount,
                    name = stats.Name,
                    level = stats.Level,
                    strength = stats.Strength,
                    agility = stats.Agility,
                    intelligence = stats.Intelligence,
                    imageURI = tokenURI,
                    attributes = new Dictionary<string, string>() // TODO: Parse attributes from URI if needed
                };
                characters.Add(characterData);
            }

            OnCharactersLoaded?.Invoke(characters);
            return characters;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load player characters: {e.Message}");
            return new List<NFTCharacterData>();
        }
    }

    public async Task<bool> MintCharacter(string characterName, int strength, int agility, int intelligence, string imageURI, Dictionary<string, string> specialTraits = null)
    {
        if (!isInitialized || !IsWalletConnected())
        {
            Debug.LogError("Web3Manager not initialized or wallet not connected");
            return false;
        }

        try
        {
            Debug.Log($"Minting character: {characterName}...");
            var mintFunction = new MintCharacterFunction()
            {
                To = connectedAccount,
                Name = characterName,
                Strength = (byte)strength,
                Agility = (byte)agility,
                Intelligence = (byte)intelligence,
                Uri = imageURI,
                FromAddress = connectedAccount
            };

            var transactionHandler = web3.Eth.GetContractTransactionHandler<MintCharacterFunction>();
            var receipt = await transactionHandler.SendRequestAndWaitForReceiptAsync(nftContractAddress, mintFunction);

            Debug.Log("Character minted successfully. Transaction: " + receipt.TransactionHash);
            // Optionally, you can find the new token ID from the receipt events

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
            Debug.Log($"Transferring character {tokenId} to {toAddress}...");
            var transferFunction = new SafeTransferFromFunction()
            {
                From = connectedAccount,
                To = toAddress,
                TokenId = BigInteger.Parse(tokenId),
                FromAddress = connectedAccount
            };

            var transactionHandler = web3.Eth.GetContractTransactionHandler<SafeTransferFromFunction>();
            var receipt = await transactionHandler.SendRequestAndWaitForReceiptAsync(nftContractAddress, transferFunction);

            Debug.Log("Character transferred successfully. Transaction: " + receipt.TransactionHash);
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
        Debug.Log($"Updating character {characterData.tokenId} on blockchain...");
        var updateStatsFunction = new UpdateStatsFunction()
        {
            TokenId = BigInteger.Parse(characterData.tokenId),
            Strength = (byte)characterData.strength,
            Agility = (byte)characterData.agility,
            Intelligence = (byte)characterData.intelligence,
            FromAddress = connectedAccount
        };

        var transactionHandler = web3.Eth.GetContractTransactionHandler<UpdateStatsFunction>();
        var task = transactionHandler.SendRequestAndWaitForReceiptAsync(nftContractAddress, updateStatsFunction);

        yield return new WaitUntil(() => task.IsCompleted);

        if (task.IsFaulted)
        {
            Debug.LogError($"Failed to update character stats: {task.Exception.Message}");
        }
        else
        {
            Debug.Log($"Character {characterData.tokenId} updated successfully. Transaction: {task.Result.TransactionHash}");
            OnCharacterUpdated?.Invoke(characterData);
        }
    }

    #endregion
}

#region Nethereum Function Messages

// For GameCharacterNFT Contract

[Function("balanceOf", "uint256")]
public class BalanceOfFunction : FunctionMessage
{
    [Parameter("address", "owner", 1)]
    public string Owner { get; set; }
}

[Function("tokenOfOwnerByIndex", "uint256")]
public class TokenOfOwnerByIndexFunction : FunctionMessage
{
    [Parameter("address", "owner", 1)]
    public string Owner { get; set; }
    [Parameter("uint256", "index", 2)]
    public BigInteger Index { get; set; }
}

[Function("tokenURI", "string")]
public class TokenURIFunction : FunctionMessage
{
    [Parameter("uint256", "tokenId", 1)]
    public BigInteger TokenId { get; set; }
}

[Function("getCharacterStats", "tuple")]
public class GetCharacterStatsFunction : FunctionMessage
{
    [Parameter("uint256", "tokenId", 1)]
    public BigInteger TokenId { get; set; }
}

[FunctionOutput]
public class CharacterStatsDTO : IFunctionOutputDTO
{
    [Parameter("string", "name", 1)]
    public string Name { get; set; }
    [Parameter("uint8", "level", 2)]
    public byte Level { get; set; }
    [Parameter("uint8", "strength", 3)]
    public byte Strength { get; set; }
    [Parameter("uint8", "agility", 4)]
    public byte Agility { get; set; }
    [Parameter("uint8", "intelligence", 5)]
    public byte Intelligence { get; set; }
}


[Function("mintCharacter")]
public class MintCharacterFunction : FunctionMessage
{
    [Parameter("address", "to", 1)]
    public string To { get; set; }
    [Parameter("string", "name", 2)]
    public string Name { get; set; }
    [Parameter("uint8", "strength", 3)]
    public byte Strength { get; set; }
    [Parameter("uint8", "agility", 4)]
    public byte Agility { get; set; }
    [Parameter("uint8", "intelligence", 5)]
    public byte Intelligence { get; set; }
    [Parameter("string", "uri", 6)]
    public string Uri { get; set; }
}

[Function("safeTransferFrom", "void")]
public class SafeTransferFromFunction : FunctionMessage
{
    [Parameter("address", "from", 1)]
    public string From { get; set; }
    [Parameter("address", "to", 2)]
    public string To { get; set; }
    [Parameter("uint256", "tokenId", 3)]
    public BigInteger TokenId { get; set; }
}

[Function("updateStats")]
public class UpdateStatsFunction : FunctionMessage
{
    [Parameter("uint256", "tokenId", 1)]
    public BigInteger TokenId { get; set; }
    [Parameter("uint8", "strength", 2)]
    public byte Strength { get; set; }
    [Parameter("uint8", "agility", 3)]
    public byte Agility { get; set; }
    [Parameter("uint8", "intelligence", 4)]
    public byte Intelligence { get; set; }
}

#endregion
