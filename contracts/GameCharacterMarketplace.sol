// SPDX-License-Identifier: MIT
pragma solidity ^0.8.9;

import "@openzeppelin/contracts/token/ERC721/IERC721.sol";
import "@openzeppelin/contracts/security/ReentrancyGuard.sol";
import "@openzeppelin/contracts/access/Ownable.sol";
import "@openzeppelin/contracts/utils/Counters.sol";

contract GameCharacterMarketplace is ReentrancyGuard, Ownable {
    using Counters for Counters.Counter;
    
    // Counter for listing IDs
    Counters.Counter private _listingIds;
    
    // Fee percentage (in basis points, 100 = 1%)
    uint256 public feePercentage = 250; // 2.5% fee
    
    // Listing status enum
    enum ListingStatus { Active, Sold, Cancelled }
    
    // Listing structure
    struct Listing {
        uint256 listingId;
        address nftContract;
        uint256 tokenId;
        address seller;
        address buyer;
        uint256 price;
        ListingStatus status;
        uint256 createdAt;
        uint256 updatedAt;
    }
    
    // Mapping from listing ID to Listing
    mapping(uint256 => Listing) private _listings;
    
    // Mapping from NFT contract address to token ID to listing ID
    mapping(address => mapping(uint256 => uint256)) private _tokenListings;
    
    // Events
    event ListingCreated(
        uint256 indexed listingId,
        address indexed nftContract,
        uint256 indexed tokenId,
        address seller,
        uint256 price
    );
    
    event ListingUpdated(
        uint256 indexed listingId,
        uint256 price
    );
    
    event ListingSold(
        uint256 indexed listingId,
        address indexed nftContract,
        uint256 indexed tokenId,
        address seller,
        address buyer,
        uint256 price
    );
    
    event ListingCancelled(
        uint256 indexed listingId,
        address indexed nftContract,
        uint256 indexed tokenId
    );
    
    event FeePercentageUpdated(
        uint256 feePercentage
    );
    
    // Create a new listing
    function createListing(
        address nftContract,
        uint256 tokenId,
        uint256 price
    ) external nonReentrant returns (uint256) {
        require(price > 0, "Price must be greater than zero");
        
        // Check if token is already listed
        require(_tokenListings[nftContract][tokenId] == 0, "Token already listed");
        
        // Check if seller owns the token
        IERC721 nft = IERC721(nftContract);
        require(nft.ownerOf(tokenId) == msg.sender, "Not the token owner");
        
        // Check if marketplace is approved to transfer the token
        require(
            nft.getApproved(tokenId) == address(this) || 
            nft.isApprovedForAll(msg.sender, address(this)),
            "Marketplace not approved to transfer token"
        );
        
        // Create new listing
        _listingIds.increment();
        uint256 listingId = _listingIds.current();
        
        _listings[listingId] = Listing({
            listingId: listingId,
            nftContract: nftContract,
            tokenId: tokenId,
            seller: msg.sender,
            buyer: address(0),
            price: price,
            status: ListingStatus.Active,
            createdAt: block.timestamp,
            updatedAt: block.timestamp
        });
        
        // Update token listing mapping
        _tokenListings[nftContract][tokenId] = listingId;
        
        emit ListingCreated(listingId, nftContract, tokenId, msg.sender, price);
        
        return listingId;
    }
    
    // Update listing price
    function updateListing(uint256 listingId, uint256 newPrice) external nonReentrant {
        require(newPrice > 0, "Price must be greater than zero");
        
        Listing storage listing = _listings[listingId];
        
        require(listing.listingId == listingId, "Listing does not exist");
        require(listing.seller == msg.sender, "Not the seller");
        require(listing.status == ListingStatus.Active, "Listing not active");
        
        // Update price
        listing.price = newPrice;
        listing.updatedAt = block.timestamp;
        
        emit ListingUpdated(listingId, newPrice);
    }
    
    // Cancel listing
    function cancelListing(uint256 listingId) external nonReentrant {
        Listing storage listing = _listings[listingId];
        
        require(listing.listingId == listingId, "Listing does not exist");
        require(listing.seller == msg.sender || owner() == msg.sender, "Not authorized");
        require(listing.status == ListingStatus.Active, "Listing not active");
        
        // Update listing status
        listing.status = ListingStatus.Cancelled;
        listing.updatedAt = block.timestamp;
        
        // Remove from token listings
        _tokenListings[listing.nftContract][listing.tokenId] = 0;
        
        emit ListingCancelled(listingId, listing.nftContract, listing.tokenId);
    }
    
    // Buy a listed NFT
    function buyListing(uint256 listingId) external payable nonReentrant {
        Listing storage listing = _listings[listingId];
        
        require(listing.listingId == listingId, "Listing does not exist");
        require(listing.status == ListingStatus.Active, "Listing not active");
        require(listing.seller != msg.sender, "Cannot buy your own listing");
        require(msg.value >= listing.price, "Insufficient payment");
        
        // Update listing status
        listing.status = ListingStatus.Sold;
        listing.buyer = msg.sender;
        listing.updatedAt = block.timestamp;
        
        // Remove from token listings
        _tokenListings[listing.nftContract][listing.tokenId] = 0;
        
        // Calculate fee
        uint256 fee = (listing.price * feePercentage) / 10000;
        uint256 sellerAmount = listing.price - fee;
        
        // Transfer NFT to buyer
        IERC721(listing.nftContract).safeTransferFrom(listing.seller, msg.sender, listing.tokenId);
        
        // Transfer payment to seller
        (bool success, ) = payable(listing.seller).call{value: sellerAmount}("");
        require(success, "Failed to send payment to seller");
        
        emit ListingSold(
            listingId,
            listing.nftContract,
            listing.tokenId,
            listing.seller,
            msg.sender,
            listing.price
        );
        
        // Refund excess payment
        if (msg.value > listing.price) {
            (bool refundSuccess, ) = payable(msg.sender).call{value: msg.value - listing.price}("");
            require(refundSuccess, "Failed to refund excess payment");
        }
    }
    
    // Update fee percentage (owner only)
    function setFeePercentage(uint256 newFeePercentage) external onlyOwner {
        require(newFeePercentage <= 1000, "Fee cannot exceed 10%");
        feePercentage = newFeePercentage;
        emit FeePercentageUpdated(newFeePercentage);
    }
    
    // Withdraw accumulated fees (owner only)
    function withdrawFees() external onlyOwner {
        uint256 balance = address(this).balance;
        require(balance > 0, "No fees to withdraw");
        
        (bool success, ) = payable(owner()).call{value: balance}("");
        require(success, "Failed to withdraw fees");
    }
    
    // Get listing by ID
    function getListing(uint256 listingId) external view returns (Listing memory) {
        return _listings[listingId];
    }
    
    // Get listing ID for a token
    function getTokenListingId(address nftContract, uint256 tokenId) external view returns (uint256) {
        return _tokenListings[nftContract][tokenId];
    }
    
    // Check if a token is listed
    function isTokenListed(address nftContract, uint256 tokenId) external view returns (bool) {
        uint256 listingId = _tokenListings[nftContract][tokenId];
        if (listingId == 0) {
            return false;
        }
        
        Listing storage listing = _listings[listingId];
        return listing.status == ListingStatus.Active;
    }
    
    // Get all active listings
    function getActiveListings(uint256 offset, uint256 limit) external view returns (Listing[] memory) {
        uint256 totalListings = _listingIds.current();
        
        // Count active listings
        uint256 activeCount = 0;
        for (uint256 i = 1; i <= totalListings; i++) {
            if (_listings[i].status == ListingStatus.Active) {
                activeCount++;
            }
        }
        
        // Apply offset and limit
        uint256 start = offset + 1; // Listings start at 1
        uint256 end = offset + limit;
        if (end > totalListings) {
            end = totalListings;
        }
        
        // Create result array
        Listing[] memory result = new Listing[](end - start + 1);
        uint256 resultIndex = 0;
        
        // Populate result array
        for (uint256 i = start; i <= end; i++) {
            if (_listings[i].status == ListingStatus.Active) {
                result[resultIndex] = _listings[i];
                resultIndex++;
            }
        }
        
        return result;
    }
    
    // Get listings by seller
    function getListingsBySeller(address seller) external view returns (Listing[] memory) {
        uint256 totalListings = _listingIds.current();
        
        // Count seller's listings
        uint256 sellerListingCount = 0;
        for (uint256 i = 1; i <= totalListings; i++) {
            if (_listings[i].seller == seller) {
                sellerListingCount++;
            }
        }
        
        // Create result array
        Listing[] memory result = new Listing[](sellerListingCount);
        uint256 resultIndex = 0;
        
        // Populate result array
        for (uint256 i = 1; i <= totalListings; i++) {
            if (_listings[i].seller == seller) {
                result[resultIndex] = _listings[i];
                resultIndex++;
            }
        }
        
        return result;
    }
}
