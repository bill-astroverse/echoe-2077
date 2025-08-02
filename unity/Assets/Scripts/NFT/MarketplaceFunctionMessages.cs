using System.Collections.Generic;
using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

#region Nethereum Function Messages for Marketplace

[Function("createListing")]
public class CreateListingFunction : FunctionMessage
{
    [Parameter("address", "nftContract", 1)]
    public string NftContract { get; set; }
    [Parameter("uint256", "tokenId", 2)]
    public BigInteger TokenId { get; set; }
    [Parameter("uint256", "price", 3)]
    public BigInteger Price { get; set; }
}

[Function("updateListing")]
public class UpdateListingFunction : FunctionMessage
{
    [Parameter("uint256", "listingId", 1)]
    public BigInteger ListingId { get; set; }
    [Parameter("uint256", "newPrice", 2)]
    public BigInteger NewPrice { get; set; }
}

[Function("cancelListing")]
public class CancelListingFunction : FunctionMessage
{
    [Parameter("uint256", "listingId", 1)]
    public BigInteger ListingId { get; set; }
}

[Function("buyListing")]
public class BuyListingFunction : FunctionMessage
{
    [Parameter("uint256", "listingId", 1)]
    public BigInteger ListingId { get; set; }
}

[Function("getListing", "tuple")]
public class GetListingFunction : FunctionMessage
{
    [Parameter("uint256", "listingId", 1)]
    public BigInteger ListingId { get; set; }
}

[Function("getActiveListings", "tuple[]")]
public class GetActiveListingsFunction : FunctionMessage
{
    [Parameter("uint256", "offset", 1)]
    public BigInteger Offset { get; set; }
    [Parameter("uint256", "limit", 2)]
    public BigInteger Limit { get; set; }
}

[Function("getListingsBySeller", "tuple[]")]
public class GetListingsBySellerFunction : FunctionMessage
{
    [Parameter("address", "seller", 1)]
    public string Seller { get; set; }
}

[FunctionOutput]
public class ListingDTO : IFunctionOutputDTO
{
    [Parameter("uint256", "listingId", 1)]
    public BigInteger ListingId { get; set; }
    [Parameter("address", "nftContract", 2)]
    public string NftContract { get; set; }
    [Parameter("uint256", "tokenId", 3)]
    public BigInteger TokenId { get; set; }
    [Parameter("address", "seller", 4)]
    public string Seller { get; set; }
    [Parameter("address", "buyer", 5)]
    public string Buyer { get; set; }
    [Parameter("uint256", "price", 6)]
    public BigInteger Price { get; set; }
    [Parameter("uint8", "status", 7)] // Enum is returned as uint8
    public byte Status { get; set; }
    [Parameter("uint256", "createdAt", 8)]
    public BigInteger CreatedAt { get; set; }
    [Parameter("uint256", "updatedAt", 9)]
    public BigInteger UpdatedAt { get; set; }
}

[Function("approve")]
public class ApproveFunction : FunctionMessage
{
    [Parameter("address", "to", 1)]
    public string To { get; set; }
    [Parameter("uint256", "tokenId", 2)]
    public BigInteger TokenId { get; set; }
}

#endregion
