using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class CharacterImageGenerator : MonoBehaviour
{
    public static CharacterImageGenerator Instance { get; private set; }
    
    [Header("Character Parts")]
    [SerializeField] private List<Sprite> bodySprites;
    [SerializeField] private List<Sprite> headSprites;
    [SerializeField] private List<Sprite> armorSprites;
    [SerializeField] private List<Sprite> weaponSprites;
    [SerializeField] private List<Sprite> accessorySprites;
    
    [Header("Rarity Visual Effects")]
    [SerializeField] private List<Sprite> rarityFrames;
    [SerializeField] private List<Sprite> rarityBackgrounds;
    [SerializeField] private List<Sprite> rarityEffects;
    
    [Header("Rendering")]
    [SerializeField] private int imageSize = 512;
    [SerializeField] private Camera renderCamera;
    [SerializeField] private Transform characterRoot;
    
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
    
    /// <summary>
    /// Generates a character image based on character data and uploads it to Vercel Blob
    /// </summary>
    /// <param name="characterData">The character data to use for generation</param>
    /// <returns>The URL of the uploaded image</returns>
    public async Task<string> GenerateAndUploadCharacterImageAsync(NFTCharacterData characterData)
    {
        // Generate the character image
        Texture2D characterTexture = GenerateCharacterImage(characterData);
        
        // Upload to Vercel Blob
        string fileName = $"character_{characterData.tokenId}.png";
        string imageUrl = await BlobStorageManager.Instance.UploadTextureAsync(characterTexture, fileName);
        
        // Clean up
        Destroy(characterTexture);
        
        return imageUrl;
    }
    
    /// <summary>
    /// Generates a character image based on character data
    /// </summary>
    /// <param name="characterData">The character data to use for generation</param>
    /// <returns>The generated character image</returns>
    public Texture2D GenerateCharacterImage(NFTCharacterData characterData)
    {
        // Create a render texture
        RenderTexture renderTexture = new RenderTexture(imageSize, imageSize, 24);
        renderCamera.targetTexture = renderTexture;
        
        // Clear the character root
        foreach (Transform child in characterRoot)
        {
            Destroy(child.gameObject);
        }
        
        // Get the character's rarity
        RarityTier rarity = characterData.Rarity;
        
        // Create the character parts based on attributes and stats
        CreateCharacterPart("Body", GetBodySprite(characterData));
        CreateCharacterPart("Head", GetHeadSprite(characterData));
        CreateCharacterPart("Armor", GetArmorSprite(characterData));
        CreateCharacterPart("Weapon", GetWeaponSprite(characterData));
        CreateCharacterPart("Accessory", GetAccessorySprite(characterData));
        
        // Add rarity-specific visuals
        if (rarity > RarityTier.Common)
        {
            // Add background
            if (rarityBackgrounds.Count > (int)rarity && rarityBackgrounds[(int)rarity] != null)
            {
                CreateCharacterPart("Background", rarityBackgrounds[(int)rarity], -1);
            }
            
            // Add frame
            if (rarityFrames.Count > (int)rarity && rarityFrames[(int)rarity] != null)
            {
                CreateCharacterPart("Frame", rarityFrames[(int)rarity], 10);
            }
            
            // Add effects
            if (rarityEffects.Count > (int)rarity && rarityEffects[(int)rarity] != null)
            {
                CreateCharacterPart("Effect", rarityEffects[(int)rarity], 5);
            }
        }
        
        // Render the character
        renderCamera.Render();
        
        // Create a texture from the render texture
        Texture2D characterTexture = new Texture2D(imageSize, imageSize, TextureFormat.RGBA32, false);
        RenderTexture.active = renderTexture;
        characterTexture.ReadPixels(new Rect(0, 0, imageSize, imageSize), 0, 0);
        characterTexture.Apply();
        
        // Clean up
        renderCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(renderTexture);
        
        return characterTexture;
    }
    
    private void CreateCharacterPart(string name, Sprite sprite, int sortingOrder = 0)
    {
        if (sprite == null)
            return;
            
        GameObject partObj = new GameObject(name);
        partObj.transform.SetParent(characterRoot);
        partObj.transform.localPosition = Vector3.zero;
        partObj.transform.localRotation = Quaternion.identity;
        partObj.transform.localScale = Vector3.one;
        
        SpriteRenderer renderer = partObj.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = sortingOrder;
    }
    
    private Sprite GetBodySprite(NFTCharacterData characterData)
    {
        // Use strength to determine body type
        int index = Mathf.Clamp(characterData.strength / 3, 0, bodySprites.Count - 1);
        return bodySprites[index];
    }
    
    private Sprite GetHeadSprite(NFTCharacterData characterData)
    {
        // Use intelligence to determine head type
        int index = Mathf.Clamp(characterData.intelligence / 3, 0, headSprites.Count - 1);
        return headSprites[index];
    }
    
    private Sprite GetArmorSprite(NFTCharacterData characterData)
    {
        // Check if character has armor attribute
        if (characterData.attributes.TryGetValue("armor", out string armorType))
        {
            // Find armor sprite by name
            for (int i = 0; i < armorSprites.Count; i++)
            {
                if (armorSprites[i].name.ToLower().Contains(armorType.ToLower()))
                {
                    return armorSprites[i];
                }
            }
        }
        
        // Default to agility-based armor
        int index = Mathf.Clamp(characterData.agility / 3, 0, armorSprites.Count - 1);
        return armorSprites[index];
    }
    
    private Sprite GetWeaponSprite(NFTCharacterData characterData)
    {
        // Check if character has weapon attribute
        if (characterData.attributes.TryGetValue("weapon", out string weaponType))
        {
            // Find weapon sprite by name
            for (int i = 0; i < weaponSprites.Count; i++)
            {
                if (weaponSprites[i].name.ToLower().Contains(weaponType.ToLower()))
                {
                    return weaponSprites[i];
                }
            }
        }
        
        // Default to strength-based weapon
        int index = Mathf.Clamp(characterData.strength / 3, 0, weaponSprites.Count - 1);
        return weaponSprites[index];
    }
    
    private Sprite GetAccessorySprite(NFTCharacterData characterData)
    {
        // Check if character has special_ability attribute
        if (characterData.attributes.TryGetValue("special_ability", out string abilityType))
        {
            // Find accessory sprite by name
            for (int i = 0; i < accessorySprites.Count; i++)
            {
                if (accessorySprites[i].name.ToLower().Contains(abilityType.ToLower()))
                {
                    return accessorySprites[i];
                }
            }
        }
        
        // Check if character has element attribute
        if (characterData.attributes.TryGetValue("element", out string elementType))
        {
            // Find accessory sprite by name
            for (int i = 0; i < accessorySprites.Count; i++)
            {
                if (accessorySprites[i].name.ToLower().Contains(elementType.ToLower()))
                {
                    return accessorySprites[i];
                }
            }
        }
        
        // Default to rarity-based accessory
        int index = Mathf.Clamp((int)characterData.Rarity, 0, accessorySprites.Count - 1);
        return accessorySprites[index];
    }
}
