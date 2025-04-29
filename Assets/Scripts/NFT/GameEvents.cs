using System;
using System.Collections.Generic;
using UnityEngine;

public static class GameEvents
{
    // NFT Character Events
    public static Action<NFTCharacter> OnNFTCharacterInitialized;
    public static Action<NFTCharacter> OnNFTCharacterSelected;
    public static Action<NFTCharacter> OnNFTCharacterLevelUp;
    
    // Game State Events
    public static Action OnGameStarted;
    public static Action OnGamePaused;
    public static Action OnGameResumed;
    public static Action OnGameOver;
    
    // UI Events
    public static Action OnOpenNFTInventory;
    public static Action OnCloseNFTInventory;

    // Marketplace Events
    public static Action OnOpenMarketplace;
    public static Action OnCloseMarketplace;
    
    // Analytics Events
    public static Action OnOpenMarketplaceAnalytics;
    public static Action OnCloseMarketplaceAnalytics;
}
