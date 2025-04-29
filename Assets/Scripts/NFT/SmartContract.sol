// SPDX-License-Identifier: MIT
pragma solidity ^0.8.9;

import "@openzeppelin/contracts/token/ERC721/ERC721.sol";
import "@openzeppelin/contracts/token/ERC721/extensions/ERC721URIStorage.sol";
import "@openzeppelin/contracts/access/Ownable.sol";
import "@openzeppelin/contracts/utils/Counters.sol";

contract GameCharacterNFT is ERC721, ERC721URIStorage, Ownable {
    using Counters for Counters.Counter;

    Counters.Counter private _tokenIdCounter;
    
    // Character stats structure
    struct CharacterStats {
        string name;
        uint8 level;
        uint8 strength;
        uint8 agility;
        uint8 intelligence;
        mapping(string => string) attributes;
    }
    
    // Mapping from token ID to character stats
    mapping(uint256 => CharacterStats) private _characterStats;
    
    // Game studio addresses that are allowed to update character stats
    mapping(address => bool) private _gameStudios;
    
    // Events
    event CharacterMinted(uint256 indexed tokenId, address indexed owner, string name);
    event CharacterLevelUp(uint256 indexed tokenId, uint8 newLevel);
    event CharacterStatsUpdated(uint256 indexed tokenId);
    event GameStudioAdded(address indexed studio);
    event GameStudioRemoved(address indexed studio);

    constructor() ERC721("GameCharacterNFT", "GCNFT") {}
    
    // Modifier to check if caller is an approved game studio
    modifier onlyGameStudio() {
        require(_gameStudios[msg.sender] || owner() == msg.sender, "Not authorized");
        _;
    }
    
    // Add a game studio
    function addGameStudio(address studio) public onlyOwner {
        _gameStudios[studio] = true;
        emit GameStudioAdded(studio);
    }
    
    // Remove a game studio
    function removeGameStudio(address studio) public onlyOwner {
        _gameStudios[studio] = false;
        emit GameStudioRemoved(studio);
    }
    
    // Check if an address is an approved game studio
    function isGameStudio(address studio) public view returns (bool) {
        return _gameStudios[studio];
    }

    // Mint a new character NFT
    function mintCharacter(
        address to,
        string memory name,
        uint8 strength,
        uint8 agility,
        uint8 intelligence,
        string memory uri
    ) public returns (uint256) {
        uint256 tokenId = _tokenIdCounter.current();
        _tokenIdCounter.increment();
        _safeMint(to, tokenId);
        _setTokenURI(tokenId, uri);
        
        // Initialize character stats
        CharacterStats storage stats = _characterStats[tokenId];
        stats.name = name;
        stats.level = 1;
        stats.strength = strength;
        stats.agility = agility;
        stats.intelligence = intelligence;
        
        emit CharacterMinted(tokenId, to, name);
        
        return tokenId;
    }
    
    // Level up a character
    function levelUp(uint256 tokenId) public onlyGameStudio {
        require(_exists(tokenId), "Character does not exist");
        
        CharacterStats storage stats = _characterStats[tokenId];
        stats.level += 1;
        
        emit CharacterLevelUp(tokenId, stats.level);
    }
    
    // Update character stats
    function updateStats(
        uint256 tokenId,
        uint8 strength,
        uint8 agility,
        uint8 intelligence
    ) public onlyGameStudio {
        require(_exists(tokenId), "Character does not exist");
        
        CharacterStats storage stats = _characterStats[tokenId];
        stats.strength = strength;
        stats.agility = agility;
        stats.intelligence = intelligence;
        
        emit CharacterStatsUpdated(tokenId);
    }
    
    // Set a character attribute
    function setAttribute(
        uint256 tokenId,
        string memory key,
        string memory value
    ) public onlyGameStudio {
        require(_exists(tokenId), "Character does not exist");
        
        CharacterStats storage stats = _characterStats[tokenId];
        stats.attributes[key] = value;
    }
    
    // Get character stats
    function getCharacterStats(uint256 tokenId) public view returns (
        string memory name,
        uint8 level,
        uint8 strength,
        uint8 agility,
        uint8 intelligence
    ) {
        require(_exists(tokenId), "Character does not exist");
        
        CharacterStats storage stats = _characterStats[tokenId];
        return (
            stats.name,
            stats.level,
            stats.strength,
            stats.agility,
            stats.intelligence
        );
    }
    
    // Get character attribute
    function getAttribute(uint256 tokenId, string memory key) public view returns (string memory) {
        require(_exists(tokenId), "Character does not exist");
        
        CharacterStats storage stats = _characterStats[tokenId];
        return stats.attributes[key];
    }

    // The following functions are overrides required by Solidity
    function _burn(uint256 tokenId) internal override(ERC721, ERC721URIStorage) {
        super._burn(tokenId);
    }

    function tokenURI(uint256 tokenId) public view override(ERC721, ERC721URIStorage) returns (string memory) {
        return super.tokenURI(tokenId);
    }
}
