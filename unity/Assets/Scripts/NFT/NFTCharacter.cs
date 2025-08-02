// Update the LoadCharacterSprite method to use BlobStorageManager for Vercel Blob URLs
private IEnumerator LoadCharacterSprite(string uri)
{
    if (string.IsNullOrEmpty(uri))
    {
        yield break;
    }
    
    // Check if this is a Vercel Blob URL
    bool isVercelBlob = uri.Contains("vercel-blob.com");
    
    if (isVercelBlob)
    {
        // Use BlobStorageManager to download the texture
        var textureTask = BlobStorageManager.Instance.DownloadTextureAsync(uri);
        
        // Wait for the download to complete
        while (!textureTask.IsCompleted)
        {
            yield return null;
        }
        
        // Check if the download was successful
        if (textureTask.Result != null)
        {
            Texture2D texture = textureTask.Result;
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = sprite;
            }
        }
        else
        {
            Debug.LogError($"Failed to load character sprite from Vercel Blob: {uri}");
        }
    }
    else
    {
        // Handle both IPFS and HTTP URIs
        string fullUri = uri;
        if (uri.StartsWith("ipfs://"))
        {
            fullUri = uri.Replace("ipfs://", "https://ipfs.io/ipfs/");
        }
        
        using (var webRequest = UnityEngine.Networking.UnityWebRequestTexture.GetTexture(fullUri))
        {
            yield return webRequest.SendWebRequest();
            
            if (webRequest.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Texture2D texture = ((UnityEngine.Networking.DownloadHandlerTexture)webRequest.downloadHandler).texture;
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                
                if (spriteRenderer != null)
                {
                    spriteRenderer.sprite = sprite;
                }
            }
            else
            {
                Debug.LogError($"Failed to load character sprite: {webRequest.error}");
            }
        }
    }
}
