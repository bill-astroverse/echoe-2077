using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class BlobStorageManager : MonoBehaviour
{
    public static BlobStorageManager Instance { get; private set; }
    
    [Header("Blob Storage Settings")]
    [SerializeField] private string blobApiEndpoint = "https://your-project-name.vercel.app/api/blob";
    [SerializeField] private bool useLocalEndpointForDevelopment = true;
    
    private string _blobReadWriteToken;
    
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
        
        // Get the blob token from environment variables
        _blobReadWriteToken = Environment.GetEnvironmentVariable("BLOB_READ_WRITE_TOKEN");
        
        // For development, you might want to set this manually
        if (string.IsNullOrEmpty(_blobReadWriteToken) && Application.isEditor)
        {
            Debug.LogWarning("BLOB_READ_WRITE_TOKEN not found in environment variables. Using development token.");
            _blobReadWriteToken = "vercel_blob_rw_YOUR_TOKEN_HERE";
        }
        
        if (useLocalEndpointForDevelopment && Application.isEditor)
        {
            blobApiEndpoint = "http://localhost:3000/api/blob";
        }
    }
    
    /// <summary>
    /// Uploads a texture to Vercel Blob storage
    /// </summary>
    /// <param name="texture">The texture to upload</param>
    /// <param name="fileName">The file name to use (should include extension)</param>
    /// <returns>The URL of the uploaded blob</returns>
    public async Task<string> UploadTextureAsync(Texture2D texture, string fileName)
    {
        try
        {
            byte[] pngData = texture.EncodeToPNG();
            return await UploadBlobAsync(pngData, fileName, "image/png");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to upload texture: {e.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Uploads a JSON object to Vercel Blob storage
    /// </summary>
    /// <param name="jsonObject">The object to serialize to JSON</param>
    /// <param name="fileName">The file name to use (should include extension)</param>
    /// <returns>The URL of the uploaded blob</returns>
    public async Task<string> UploadJsonAsync(object jsonObject, string fileName)
    {
        try
        {
            string jsonString = JsonConvert.SerializeObject(jsonObject);
            byte[] jsonData = Encoding.UTF8.GetBytes(jsonString);
            return await UploadBlobAsync(jsonData, fileName, "application/json");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to upload JSON: {e.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Uploads binary data to Vercel Blob storage
    /// </summary>
    /// <param name="data">The binary data to upload</param>
    /// <param name="fileName">The file name to use</param>
    /// <param name="contentType">The MIME type of the content</param>
    /// <returns>The URL of the uploaded blob</returns>
    public async Task<string> UploadBlobAsync(byte[] data, string fileName, string contentType)
    {
        if (string.IsNullOrEmpty(_blobReadWriteToken))
        {
            Debug.LogError("Blob token is not set. Cannot upload to Vercel Blob.");
            return null;
        }
        
        // Create a unique file name to avoid collisions
        string uniqueFileName = $"{DateTime.UtcNow.Ticks}_{fileName}";
        
        // Create the form data for the request
        WWWForm form = new WWWForm();
        form.AddBinaryData("file", data, uniqueFileName, contentType);
        
        // Create the request
        using (UnityWebRequest request = UnityWebRequest.Post($"{blobApiEndpoint}/upload", form))
        {
            // Add the authorization header
            request.SetRequestHeader("Authorization", $"Bearer {_blobReadWriteToken}");
            
            // Send the request
            var operation = request.SendWebRequest();
            
            // Wait for the request to complete
            while (!operation.isDone)
            {
                await Task.Delay(100);
            }
            
            // Check for errors
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Failed to upload blob: {request.error}");
                return null;
            }
            
            // Parse the response
            string responseText = request.downloadHandler.text;
            BlobUploadResponse response = JsonConvert.DeserializeObject<BlobUploadResponse>(responseText);
            
            return response.url;
        }
    }
    
    /// <summary>
    /// Downloads a blob from Vercel Blob storage
    /// </summary>
    /// <param name="url">The URL of the blob to download</param>
    /// <returns>The downloaded data</returns>
    public async Task<byte[]> DownloadBlobAsync(string url)
    {
        try
        {
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                var operation = request.SendWebRequest();
                
                // Wait for the request to complete
                while (!operation.isDone)
                {
                    await Task.Delay(100);
                }
                
                // Check for errors
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Failed to download blob: {request.error}");
                    return null;
                }
                
                return request.downloadHandler.data;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to download blob: {e.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Downloads a texture from Vercel Blob storage
    /// </summary>
    /// <param name="url">The URL of the texture to download</param>
    /// <returns>The downloaded texture</returns>
    public async Task<Texture2D> DownloadTextureAsync(string url)
    {
        try
        {
            using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
            {
                var operation = request.SendWebRequest();
                
                // Wait for the request to complete
                while (!operation.isDone)
                {
                    await Task.Delay(100);
                }
                
                // Check for errors
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Failed to download texture: {request.error}");
                    return null;
                }
                
                return ((DownloadHandlerTexture)request.downloadHandler).texture;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to download texture: {e.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Deletes a blob from Vercel Blob storage
    /// </summary>
    /// <param name="url">The URL of the blob to delete</param>
    /// <returns>True if the blob was deleted successfully</returns>
    public async Task<bool> DeleteBlobAsync(string url)
    {
        if (string.IsNullOrEmpty(_blobReadWriteToken))
        {
            Debug.LogError("Blob token is not set. Cannot delete from Vercel Blob.");
            return false;
        }
        
        try
        {
            // Extract the blob path from the URL
            Uri uri = new Uri(url);
            string blobPath = uri.PathAndQuery;
            
            // Create the request
            using (UnityWebRequest request = UnityWebRequest.Delete($"{blobApiEndpoint}/delete?url={Uri.EscapeDataString(url)}"))
            {
                // Add the authorization header
                request.SetRequestHeader("Authorization", $"Bearer {_blobReadWriteToken}");
                
                // Send the request
                var operation = request.SendWebRequest();
                
                // Wait for the request to complete
                while (!operation.isDone)
                {
                    await Task.Delay(100);
                }
                
                // Check for errors
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Failed to delete blob: {request.error}");
                    return false;
                }
                
                return true;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to delete blob: {e.Message}");
            return false;
        }
    }
    
    // Response class for blob upload
    [Serializable]
    private class BlobUploadResponse
    {
        public string url;
    }
}
