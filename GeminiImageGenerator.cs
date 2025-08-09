using System.Text.Json;
using System.Text.Json.Serialization;

namespace ImageGenTool;

public class GeminiImageGenerator : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public GeminiImageGenerator(string apiKey)
    {
        _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        _httpClient = new HttpClient();
    }

    public async Task<byte[]> GenerateImageAsync(string prompt)
    {
        Console.WriteLine("🔄 Generating image with Google Gemini API...");
        
        try
        {
            return await CallGeminiApiAsync(prompt);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to generate image with Gemini API: {ex.Message}", ex);
        }
    }

    private async Task<byte[]> CallGeminiApiAsync(string prompt)
    {
        // Using Google Gemini API for image generation
        // Based on https://ai.google.dev/gemini-api/docs/image-generation
        var requestUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash-preview-image-generation:generateContent";
        
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("x-goog-api-key", _apiKey);
        _httpClient.Timeout = TimeSpan.FromSeconds(60);
        
        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            },
            generationConfig = new
            {
                responseModalities = new[] { "TEXT", "IMAGE" }
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync(requestUrl, content);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Gemini API request failed: {response.StatusCode} - {errorContent}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        
        // Parse response to extract base64 image data
        // The response contains the image data in a "data" field that we need to extract
        var base64Image = ExtractImageDataFromResponse(responseContent);
        
        if (string.IsNullOrEmpty(base64Image))
        {
            throw new InvalidOperationException("No image data received from Gemini API response");
        }

        return Convert.FromBase64String(base64Image);
    }

    private string ExtractImageDataFromResponse(string responseContent)
    {
        // Extract image data similar to: grep -o '"data": "[^"]*"' | cut -d'"' -f4
        var dataPattern = "\"data\": \"";
        var dataIndex = responseContent.IndexOf(dataPattern);
        
        if (dataIndex == -1)
        {
            throw new InvalidOperationException("No image data found in Gemini API response");
        }
        
        var startIndex = dataIndex + dataPattern.Length;
        var endIndex = responseContent.IndexOf("\"", startIndex);
        
        if (endIndex == -1)
        {
            throw new InvalidOperationException("Invalid image data format in Gemini API response");
        }
        
        return responseContent.Substring(startIndex, endIndex - startIndex);
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}