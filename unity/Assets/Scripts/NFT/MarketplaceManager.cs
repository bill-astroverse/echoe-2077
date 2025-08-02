using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using UnityEngine;
using Nethereum.Web3;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Newtonsoft.Json;

[Serializable]
public class MarketplaceListing
{
    public string listingId;
    public string nftContract;
    public string tokenId;
    public string seller;
    public string buyer;
    public string price; // Using string to handle large numbers
    public int status; // 0 = Active, 1 = Sold, 2 = Cancelled
    public long createdAt;
    public long updatedAt;
    public NFTCharacterData characterData; // Associated character data
}

public class MarketplaceManager : MonoBehaviour
{
    public static MarketplaceManager Instance { get; private set; }

    private Contract marketplaceContract;
    private bool isInitialized = false;

    // Events
    public event Action<List<MarketplaceListing>> OnListingsLoaded;
    public event Action<MarketplaceListing> OnListingCreated;
    public event Action<MarketplaceListing> OnListingUpdated;
    public event Action<MarketplaceListing> OnListingSold;
    public event Action<MarketplaceListing> OnListingCancelled;

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
        // Delay initialization to ensure Web3Manager is ready
        StartCoroutine(DelayedInitialize());
    }

    private IEnumerator DelayedInitialize()
    {
        // Wait for the end of the frame to ensure all Awake and Start methods have been called
        yield return new WaitForEndOfFrame();
        Initialize();
    }

    private void Initialize()
    {
        try
        {
            if (Web3Manager.Instance != null)
            {
                marketplaceContract = Web3Manager.Instance.marketplaceContract;
                if (marketplaceContract != null)
                {
                    isInitialized = true;
                    Debug.Log("MarketplaceManager initialized successfully");
                }
                else
                {
                    Debug.LogError("Failed to get Marketplace contract from Web3Manager");
                }
            }
            else
            {
                Debug.LogError("Failed to initialize MarketplaceManager: Web3Manager not found");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to initialize MarketplaceManager: {e.Message}");
        }
    }

    #region Marketplace Operations

    public async Task<List<MarketplaceListing>> GetActiveListings(int offset = 0, int limit = 20)
    {
        if (!isInitialized)
        {
            Debug.LogError("MarketplaceManager not initialized");
            return new List<MarketplaceListing>();
        }

        try
        {
            var getActiveListingsFunction = new GetActiveListingsFunction()
            {
                Offset = offset,
                Limit = limit
            };
            var listingsDTO = await marketplaceContract.GetFunction<GetActiveListingsFunction>().CallDeserializingToObjectAsync<List<ListingDTO>>(getActiveListingsFunction);

            var listings = await ConvertToListingData(listingsDTO);
            OnListingsLoaded?.Invoke(listings);
            return listings;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to get active listings: {e.Message}");
            return new List<MarketplaceListing>();
        }
    }

    public async Task<List<MarketplaceListing>> GetMyListings()
    {
        if (!isInitialized || !Web3Manager.Instance.IsWalletConnected())
        {
            Debug.LogError("MarketplaceManager not initialized or wallet not connected");
            return new List<MarketplaceListing>();
        }

        try
        {
            string connectedAccount = Web3Manager.Instance.GetConnectedAccount();
            var getListingsBySellerFunction = new GetListingsBySellerFunction() { Seller = connectedAccount };
            var listingsDTO = await marketplaceContract.GetFunction<GetListingsBySellerFunction>().CallDeserializingToObjectAsync<List<ListingDTO>>(getListingsBySellerFunction);

            var listings = await ConvertToListingData(listingsDTO);
            return listings;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to get my listings: {e.Message}");
            return new List<MarketplaceListing>();
        }
    }

    public async Task<bool> CreateListing(string tokenId, string priceInEth)
    {
        if (!isInitialized || !Web3Manager.Instance.IsWalletConnected())
        {
            Debug.LogError("MarketplaceManager not initialized or wallet not connected");
            return false;
        }

        try
        {
            string connectedAccount = Web3Manager.Instance.GetConnectedAccount();
            var nftContract = Web3Manager.Instance.nftContract;
            var priceInWei = Web3.Convert.ToWei(priceInEth);

            // 1. Approve the marketplace to transfer the NFT
            var approveFunction = new ApproveFunction()
            {
                To = marketplaceContract.Address,
                TokenId = BigInteger.Parse(tokenId),
                FromAddress = connectedAccount
            };
            var approveHandler = Web3Manager.Instance.GetWeb3().Eth.GetContractTransactionHandler<ApproveFunction>();
            var approveReceipt = await approveHandler.SendRequestAndWaitForReceiptAsync(nftContract.Address, approveFunction);
            Debug.Log("Approval transaction receipt: " + approveReceipt.TransactionHash);

            // 2. Create the listing
            var createListingFunction = new CreateListingFunction()
            {
                NftContract = nftContract.Address,
                TokenId = BigInteger.Parse(tokenId),
                Price = priceInWei,
                FromAddress = connectedAccount
            };
            var listingHandler = Web3Manager.Instance.GetWeb3().Eth.GetContractTransactionHandler<CreateListingFunction>();
            var listingReceipt = await listingHandler.SendRequestAndWaitForReceiptAsync(marketplaceContract.Address, createListingFunction);
            Debug.Log("Create listing transaction receipt: " + listingReceipt.TransactionHash);

            // TODO: Decode event from receipt to get the new listing and fire OnListingCreated

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to create listing: {e.Message}");
            return false;
        }
    }

    public async Task<bool> BuyListing(string listingId, string priceInWei)
    {
        if (!isInitialized || !Web3Manager.Instance.IsWalletConnected())
        {
            Debug.LogError("MarketplaceManager not initialized or wallet not connected");
            return false;
        }

        try
        {
            string connectedAccount = Web3Manager.Instance.GetConnectedAccount();
            var buyListingFunction = new BuyListingFunction()
            {
                ListingId = BigInteger.Parse(listingId),
                FromAddress = connectedAccount,
                AmountToSend = new HexBigInteger(priceInWei)
            };

            var buyHandler = Web3Manager.Instance.GetWeb3().Eth.GetContractTransactionHandler<BuyListingFunction>();
            var buyReceipt = await buyHandler.SendRequestAndWaitForReceiptAsync(marketplaceContract.Address, buyListingFunction);
            Debug.Log("Buy listing transaction receipt: " + buyReceipt.TransactionHash);

            // TODO: Decode event from receipt to get the sold listing and fire OnListingSold

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to buy listing: {e.Message}");
            return false;
        }
    }

    #endregion

    #region Helper Methods

    private async Task<List<MarketplaceListing>> ConvertToListingData(List<ListingDTO> dtos)
    {
        var listings = new List<MarketplaceListing>();
        var nftContract = Web3Manager.Instance.nftContract;

        foreach (var dto in dtos)
        {
            // Get character stats
            var getCharacterStatsFunction = new GetCharacterStatsFunction() { TokenId = dto.TokenId };
            var stats = await nftContract.GetFunction<GetCharacterStatsFunction>().CallDeserializingToObjectAsync<CharacterStatsDTO>(getCharacterStatsFunction);

            // Get token URI
            var tokenURIFunction = new TokenURIFunction() { TokenId = dto.TokenId };
            var tokenURI = await nftContract.GetFunction<TokenURIFunction>().CallAsync<string>(tokenURIFunction);

            listings.Add(new MarketplaceListing
            {
                listingId = dto.ListingId.ToString(),
                nftContract = dto.NftContract,
                tokenId = dto.TokenId.ToString(),
                seller = dto.Seller,
                buyer = dto.Buyer,
                price = dto.Price.ToString(),
                status = dto.Status,
                createdAt = (long)dto.CreatedAt,
                updatedAt = (long)dto.UpdatedAt,
                characterData = new NFTCharacterData
                {
                    tokenId = dto.TokenId.ToString(),
                    owner = dto.Seller, // The owner is the seller in a listing
                    name = stats.Name,
                    level = stats.Level,
                    strength = stats.Strength,
                    agility = stats.Agility,
                    intelligence = stats.Intelligence,
                    imageURI = tokenURI
                }
            });
        }
        return listings;
    }

    public string FormatPrice(string weiPrice)
    {
        try
        {
            return Web3.Convert.FromWei(BigInteger.Parse(weiPrice)).ToString("0.###") + " ETH";
        }
        catch
        {
            return "? ETH";
        }
    }

    public string ConvertEthToWei(string ethPrice)
    {
        try
        {
            return Web3.Convert.ToWei(ethPrice).ToString();
        }
        catch
        {
            return "0";
        }
    }

    #endregion
}
