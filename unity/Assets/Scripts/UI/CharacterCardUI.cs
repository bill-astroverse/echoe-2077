using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterCardUI : MonoBehaviour
{
    [SerializeField] private Image characterImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private TextMeshProUGUI rarityText;
    [SerializeField] private Image rarityBorder;
    [SerializeField] private Button cardButton;
    [SerializeField] private Button transferButton;
    [SerializeField] private Button sellButton;
    
    private NFTCharacter character;
    
    public event Action<NFTCharacter> OnCardClicked;
    
    private void Awake()
    {
        cardButton.onClick.AddListener(HandleCardClicked);
        transferButton.onClick.AddListener(HandleTransferClicked);
        sellButton.onClick.AddListener(HandleSellClicked);
    }
    
    public void SetCharacter(NFTCharacter nftCharacter)
    {
        character = nftCharacter;
        
        // Update UI elements
        nameText.text = character.characterData.name;
        levelText.text = $"Level {character.characterData.level}";
        statsText.text = $"STR: {character.characterData.strength} | AGI: {character.characterData.agility} | INT: {character.characterData.intelligence}";
        
        // Set rarity text and color
        if (character.characterData.attributes.TryGetValue("rarity", out string rarityStr) && 
            Enum.TryParse<RarityTier>(rarityStr, out RarityT  out string rarityStr) && 
            Enum.TryParse<RarityTier>(rarityStr, out RarityTier rarity))
        {
            rarityText.text = RaritySystem.GetRarityName(rarity);
            rarityText.color = RaritySystem.GetRarityColor(rarity);
            rarityBorder.color = RaritySystem.GetRarityColor(rarity);
        }
        else
        {
            rarityText.text = "Common";
            rarityText.color = RaritySystem.GetRarityColor(RarityTier.Common);
            rarityBorder.color = RaritySystem.GetRarityColor(RarityTier.Common);
        }
        
        // If the character has a sprite, use it
        if (character.spriteRenderer != null && character.spriteRenderer.sprite != null)
        {
            characterImage.sprite = character.spriteRenderer.sprite;
            characterImage.preserveAspect = true;
        }
    }
    
    private void HandleCardClicked()
    {
        OnCardClicked?.Invoke(character);
    }
    
    private void HandleTransferClicked()
    {
        // Open transfer dialog
        TransferCharacterUI transferUI = FindObjectOfType<TransferCharacterUI>(true);
        if (transferUI != null)
        {
            transferUI.gameObject.SetActive(true);
            transferUI.SetCharacter(character);
        }
    }

    private void HandleSellClicked()
    {
        // Open sell dialog
        CreateListingUI createListingUI = FindObjectOfType<CreateListingUI>(true);
        if (createListingUI != null)
        {
            createListingUI.gameObject.SetActive(true);
            createListingUI.SetCharacter(character);
        }
    }
}
