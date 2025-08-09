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
        // Using Google's Imagen API (part of Google AI) for image generation
        // Based on the documentation at https://ai.google.dev/gemini-api/docs/image-generation
        var requestUrl = $"https://generativelanguage.googleapis.com/v1beta/models/imagen-3.0-generate-001:generateImage?key={_apiKey}";
        
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.Timeout = TimeSpan.FromSeconds(60);
        
        var requestBody = new
        {
            prompt = prompt,
            config = new
            {
                number_of_images = 1,
                image_format = "PNG",
                aspect_ratio = "1:1",
                safety_filter_level = "block_only_high"
            }
        };

        var json = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync(requestUrl, content);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Gemini API request failed: {response.StatusCode} - {errorContent}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var geminiResponse = JsonSerializer.Deserialize<GeminiImageResponse>(responseContent);
        
        if (geminiResponse?.GeneratedImages == null || geminiResponse.GeneratedImages.Length == 0)
        {
            throw new InvalidOperationException("No image data received from Gemini API");
        }

        var base64Image = geminiResponse.GeneratedImages[0].BytesBase64Encoded;
        if (string.IsNullOrEmpty(base64Image))
        {
            throw new InvalidOperationException("Invalid base64 image data received from Gemini API");
        }

        return Convert.FromBase64String(base64Image);
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}

public class GeminiImageResponse
{
    [JsonPropertyName("generatedImages")]
    public GeminiGeneratedImage[]? GeneratedImages { get; set; }
}

public class GeminiGeneratedImage
{
    [JsonPropertyName("bytesBase64Encoded")]
    public string? BytesBase64Encoded { get; set; }
    
    [JsonPropertyName("mimeType")]
    public string? MimeType { get; set; }
}