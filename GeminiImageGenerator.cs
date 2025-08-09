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

    public Task<byte[]> GenerateImageAsync(string prompt)
    {
        try
        {
            // For demonstration purposes, we'll use a placeholder implementation
            // In production, uncomment the API call below and remove the placeholder logic
            
            Console.WriteLine("ℹ️  Note: Using placeholder image generation (Gemini API integration ready)");
            return Task.FromResult(CreatePlaceholderImage(prompt));
            
            /* Uncomment for actual Gemini API integration:
            
            var requestUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-pro-vision:generateContent?key={_apiKey}";
            
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = $"Generate an image based on: {prompt}" }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.4,
                    topK = 32,
                    topP = 1,
                    maxOutputTokens = 4096
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

            // Parse the actual image data from the API response
            var responseContent = await response.Content.ReadAsStringAsync();
            var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(responseContent);
            
            // Extract image data from response (implementation depends on actual API response format)
            // This is where you'd handle the actual image data from Gemini
            return await ProcessGeminiImageResponse(geminiResponse);
            
            */
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to generate image: {ex.Message}", ex);
        }
    }

    private static byte[] CreatePlaceholderImage(string prompt)
    {
        // Create a simple SVG as placeholder
        var svg = $@"<?xml version='1.0' encoding='UTF-8'?>
<svg width='512' height='512' xmlns='http://www.w3.org/2000/svg'>
  <rect width='100%' height='100%' fill='#f0f0f0'/>
  <text x='50%' y='45%' font-family='Arial, sans-serif' font-size='16' text-anchor='middle' fill='#333'>
    Generated Image Placeholder
  </text>
  <text x='50%' y='55%' font-family='Arial, sans-serif' font-size='12' text-anchor='middle' fill='#666'>
    Prompt: {EscapeXml(prompt.Length > 50 ? prompt.Substring(0, 47) + "..." : prompt)}
  </text>
  <text x='50%' y='65%' font-family='Arial, sans-serif' font-size='10' text-anchor='middle' fill='#999'>
    (This is a placeholder - integrate with actual Gemini image generation API)
  </text>
</svg>";

        return System.Text.Encoding.UTF8.GetBytes(svg);
    }

    private static string EscapeXml(string text)
    {
        return text.Replace("&", "&amp;")
                   .Replace("<", "&lt;")
                   .Replace(">", "&gt;")
                   .Replace("\"", "&quot;")
                   .Replace("'", "&apos;");
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}

public class GeminiResponse
{
    [JsonPropertyName("candidates")]
    public GeminiCandidate[]? Candidates { get; set; }
}

public class GeminiCandidate
{
    [JsonPropertyName("content")]
    public GeminiContent? Content { get; set; }
}

public class GeminiContent
{
    [JsonPropertyName("parts")]
    public GeminiPart[]? Parts { get; set; }
}

public class GeminiPart
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }
}