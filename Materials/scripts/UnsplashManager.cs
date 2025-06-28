using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json; // We are using Newtonsoft for more flexible JSON parsing

// --- These classes help parse the JSON response from Unsplash ---
[System.Serializable]
public class UnsplashPhotoUrls
{
    public string raw;
    public string full;
    public string regular; // A good balance of size and quality
    public string small;
    public string thumb;
}

[System.Serializable]
public class UnsplashPhoto
{
    public string id;
    public string description;
    public UnsplashPhotoUrls urls;
}


public class UnsplashManager : MonoBehaviour
{
    // Drag your planes from the Hierarchy into this list in the Inspector.
    public List<Renderer> photoPlanes;

    [Header("Unsplash API Settings")]
    [Tooltip("Get this from the Unsplash Developer Dashboard")]
    public string accessKey = "PASTE_YOUR_UNSPLASH_ACCESS_KEY_HERE";
    
    [Tooltip("The search term to look for on Unsplash")]
    public string searchQuery = "nature"; // Change this to whatever you want!

    void Start()
    {
        // Basic validation to make sure everything is set up.
        if (photoPlanes.Count == 0 || string.IsNullOrEmpty(accessKey) || accessKey == "PASTE_YOUR_UNSPLASH_ACCESS_KEY_HERE")
        {
            Debug.LogError("Please assign planes in the Inspector and provide a valid Unsplash Access Key.");
            return;
        }

        // Start the main process.
        StartCoroutine(FetchAndDisplayPhotos());
    }

    private IEnumerator FetchAndDisplayPhotos()
    {
        // 1. CONSTRUCT THE API URL
        // We'll use the 'random' endpoint to get a collection of photos based on a query.
        // The 'count' parameter should match the number of planes we want to texture.
        int photoCount = photoPlanes.Count;
        string apiUrl = $"https://api.unsplash.com/photos/random?query={searchQuery}&count={photoCount}&client_id={accessKey}";

        // 2. MAKE THE API CALL
        UnityWebRequest request = UnityWebRequest.Get(apiUrl);

        // Unsplash API requires this specific header.
        request.SetRequestHeader("Accept-Version", "v1");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error fetching photo list from Unsplash: " + request.error);
            Debug.LogError("Response: " + request.downloadHandler.text);
            yield break;
        }

        // 3. PARSE THE JSON RESPONSE using Newtonsoft.Json
        string jsonResponse = request.downloadHandler.text;
        List<UnsplashPhoto> photos = JsonConvert.DeserializeObject<List<UnsplashPhoto>>(jsonResponse);

        if (photos == null || photos.Count == 0)
        {
            Debug.LogError("Failed to parse JSON response or no photos found for the query.");
            yield break;
        }

        Debug.Log($"Successfully fetched {photos.Count} photos from Unsplash.");

        // 4. DOWNLOAD AND APPLY EACH IMAGE
        for (int i = 0; i < photos.Count; i++)
        {
            // Make sure we don't try to access a plane that doesn't exist
            if (i < photoPlanes.Count)
            {
                // We'll use the 'regular' size URL for a good quality/performance balance.
                string imageUrl = photos[i].urls.regular;
                StartCoroutine(DownloadAndApplyTexture(imageUrl, photoPlanes[i]));
            }
        }
    }

    private IEnumerator DownloadAndApplyTexture(string imageUrl, Renderer targetPlane)
    {
        UnityWebRequest textureRequest = UnityWebRequestTexture.GetTexture(imageUrl);
        yield return textureRequest.SendWebRequest();

        if (textureRequest.result == UnityWebRequest.Result.Success)
        {
            Texture2D downloadedTexture = DownloadHandlerTexture.GetContent(textureRequest);
            
            // Apply the texture to a unique material instance for this plane.
            targetPlane.material.mainTexture = downloadedTexture;
            Debug.Log($"Successfully applied texture to {targetPlane.name}");
        }
        else
        {
            Debug.LogError($"Failed to download image from {imageUrl}: {textureRequest.error}");
        }
    }
}