using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MintCharacterUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private Slider strengthSlider;
    [SerializeField] private Slider agilitySlider;
    [SerializeField] private Slider intelligenceSlider;
    [SerializeField] private TextMeshProUGUI strengthText;
    [SerializeField] private TextMeshProUGUI agilityText;
    [SerializeField] private TextMeshProUGUI intelligenceText;
    [SerializeField] private TextMeshProUGUI pointsRemainingText;
    [SerializeField] private TextMeshProUGUI rarityText;
    [SerializeField] private Image rarityIcon;
    [SerializeField] private Button mintButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button rollRarityButton;
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private Image characterPreviewImage;
    [SerializeField] private Button generatePreviewButton;
    
    private int totalPoints = 15;
    private int usedPoints = 0;
    private RarityTier currentRarity = RarityTier.Common;
    private bool isRandomRarity = false;
    
    private void Awake()
    {
        // Set up UI events
        strengthSlider.onValueChanged.AddListener(OnStrengthChanged);
        agilitySlider.onValueChanged.AddListener(OnAgilityChanged);
        intelligenceSlider.onValueChanged.AddListener(OnIntelligenceChanged);
        
        mintButton.onClick.AddListener(OnMintClicked);
        cancelButton.onClick.AddListener(OnCancelClicked);
        rollRarityButton.onClick.AddListener(OnRollRarityClicked);
        generatePreviewButton.onClick.AddListener(OnGeneratePreviewClicked);
        
        // Initialize sliders
        strengthSlider.minValue = 1;
        strengthSlider.maxValue = 10;
        strengthSlider.value = 5;
        
        agilitySlider.minValue = 1;
        agilitySlider.maxValue = 10;
        agilitySlider.value = 5;
        
        intelligenceSlider.minValue = 1;
        intelligenceSlider.maxValue = 10;
        intelligenceSlider.value = 5;
        
        // Hide loading panel
        loadingPanel.SetActive(false);
        
        // Update UI
        UpdateUI();
    }
    
    private void OnEnable()
    {
        // Reset values when dialog opens
        nameInput.text = "";
        strengthSlider.value = 5;
        agilitySlider.value = 5;
        intelligenceSlider.value = 5;
        isRandomRarity = false;
        currentRarity = RarityTier.Common;
        UpdateUI();
    }
    
    private void OnStrengthChanged(float value)
    {
        if (!isRandomRarity)
        {
            UpdateRarityFromStats();
        }
        UpdateUI();
    }
    
    private void OnAgilityChanged(float value)
    {
        if (!isRandomRarity)
        {
            UpdateRarityFromStats();
        }
        UpdateUI();
    }
    
    private void OnIntelligenceChanged(float value)
    {
        if (!isRandomRarity)
        {
            UpdateRarityFromStats();
        }
        UpdateUI();
    }
    
    private void UpdateRarityFromStats()
    {
        int strength = Mathf.RoundToInt(strengthSlider.value);
        int agility = Mathf.RoundToInt(agilitySlider.value);
        int intelligence = Mathf.RoundToInt(intelligenceSlider.value);
        
        currentRarity = RaritySystem.DetermineRarity(strength, agility, intelligence);
    }
    
    private void OnRollRarityClicked()
    {
        isRandomRarity = true;
        currentRarity = RaritySystem.RollRandomRarity();
        
        // Visual feedback for the roll
        StartCoroutine(AnimateRarityRoll());
    }
    
    private IEnumerator AnimateRarityRoll()
    {
        // Simple animation to show rarity rolling
        for (int i = 0; i < 10; i++)
        {
            RarityTier randomRarity = (RarityTier)Random.Range(0, System.Enum.GetValues(typeof(RarityTier)).Length);
            rarityText.text = RaritySystem.GetRarityName(randomRarity);
            rarityText.color = RaritySystem.GetRarityColor(randomRarity);
            rarityIcon.color = RaritySystem.GetRarityColor(randomRarity);
            yield return new WaitForSeconds(0.1f);
        }
        
        // Show final result
        rarityText.text = RaritySystem.GetRarityName(currentRarity);
        rarityText.color = RaritySystem.GetRarityColor(currentRarity);
        rarityIcon.color = RaritySystem.GetRarityColor(currentRarity);
        
        // Add some bonus points based on rarity
        int bonusPoints = (int)currentRarity * 2;
        totalPoints = 15 + bonusPoints;
        
        UpdateUI();
    }
    
    private void UpdateUI()
    {
        int strength = Mathf.RoundToInt(strengthSlider.value);
        int agility = Mathf.RoundToInt(agilitySlider.value);
        int intelligence = Mathf.RoundToInt(intelligenceSlider.value);
        
        strengthText.text = $"Strength: {strength}";
        agilityText.text = $"Agility: {agility}";
        intelligenceText.text = $"Intelligence: {intelligence}";
        
        usedPoints = strength + agility + intelligence;
        int remaining = totalPoints - usedPoints;
        
        pointsRemainingText.text = $"Points Remaining: {remaining}";
        
        // Update rarity display
        rarityText.text = RaritySystem.GetRarityName(currentRarity);
        rarityText.color = RaritySystem.GetRarityColor(currentRarity);
        rarityIcon.color = RaritySystem.GetRarityColor(currentRarity);
        
        // Disable mint button if name is empty or points don't add up to total
        mintButton.interactable = !string.IsNullOrEmpty(nameInput.text) && usedPoints <= totalPoints;
    }
    
    private async void OnGeneratePreviewClicked()
    {
        // Create a temporary character data for preview
        var previewData = new NFTCharacterData
        {
            tokenId = "preview",
            owner = Web3Manager.Instance.GetConnectedAccount(),
            name = nameInput.text.Length > 0 ? nameInput.text : "Preview Character",
            level = 1,
            strength = Mathf.RoundToInt(strengthSlider.value),
            agility = Mathf.RoundToInt(agilitySlider.value),
            intelligence = Mathf.RoundToInt(intelligenceSlider.value),
            attributes = new Dictionary<string, string>
            {
                { "rarity", currentRarity.ToString() }
            }
        };
        
        // Generate special traits based on rarity
        Dictionary<string, string> specialTraits = RaritySystem.GenerateSpecialTraits(currentRarity);
        foreach (var trait in specialTraits)
        {
            previewData.attributes[trait.Key] = trait.Value;
        }
        
        // Generate the character image
        Texture2D characterTexture = CharacterImageGenerator.Instance.GenerateCharacterImage(previewData);
        
        // Create a sprite from the texture
        Sprite characterSprite = Sprite.Create(characterTexture, new Rect(0, 0, characterTexture.width, characterTexture.height), new Vector2(0.5f, 0.5f));
        
        // Update the preview image
        characterPreviewImage.sprite = characterSprite;
        characterPreviewImage.preserveAspect = true;
    }
    
    private async void OnMintClicked()
    {
        string characterName = nameInput.text;
        int strength = Mathf.RoundToInt(strengthSlider.value);
        int agility = Mathf.RoundToInt(agilitySlider.value);
        int intelligence = Mathf.RoundToInt(intelligenceSlider.value);
        
        // Show loading panel
        loadingPanel.SetActive(true);
        
        // Generate special traits based on rarity
        Dictionary<string, string> specialTraits = RaritySystem.GenerateSpecialTraits(currentRarity);
        
        // Add rarity to traits
        specialTraits["rarity"] = currentRarity.ToString();
        
        // Create a temporary character data for image generation
        var tempCharData = new NFTCharacterData
        {
            tokenId = "temp",
            owner = Web3Manager.Instance.GetConnectedAccount(),
            name = characterName,
            level = 1,
            strength = strength,
            agility = agility,
            intelligence = intelligence,
            attributes = specialTraits
        };
        
        // Generate and upload the character image
        string imageURI = await CharacterImageGenerator.Instance.GenerateAndUploadCharacterImageAsync(tempCharData);
        
        if (string.IsNullOrEmpty(imageURI))
        {
            Debug.LogError("Failed to generate and upload character image");
            imageURI = "https://example.com/placeholder.png"; // Fallback
        }
        
        // Mint the character
        bool success = await Web3Manager.Instance.MintCharacter(characterName, strength, agility, intelligence, imageURI, specialTraits);
        
        // Hide loading panel
        loadingPanel.SetActive(false);
        
        if (success)
        {
            // Close the dialog
            gameObject.SetActive(false);
        }
        else
        {
            // Show error message
            Debug.LogError("Failed to mint character");
        }
    }
    
    private void OnCancelClicked()
    {
        gameObject.SetActive(false);
    }
}
