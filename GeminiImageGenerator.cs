using System.Text.Json;
using System.Text.Json.Serialization;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

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

    public async Task<byte[]> GenerateImageAsync(string prompt, int width = 1024, int height = 1024)
    {
        Console.WriteLine($"🔄 Generating image with Google Gemini API ({width}x{height})...");
        
        try
        {
            var rawImageData = await CallGeminiApiAsync(prompt, width, height);
            
            // Ensure the image has the exact requested dimensions
            Console.WriteLine($"🖼️  Processing image to ensure {width}x{height} dimensions...");
            var processedImageData = await EnsureImageDimensionsAsync(rawImageData, width, height);
            
            return processedImageData;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to generate image with Gemini API: {ex.Message}", ex);
        }
    }

    private async Task<byte[]> CallGeminiApiAsync(string prompt, int width, int height)
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
                        new { text = $"{prompt} (generate at {width}x{height} resolution)" }
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

    private async Task<byte[]> EnsureImageDimensionsAsync(byte[] imageData, int targetWidth, int targetHeight)
    {
        using var inputStream = new MemoryStream(imageData);
        using var image = await Image.LoadAsync(inputStream);
        
        var originalWidth = image.Width;
        var originalHeight = image.Height;
        
        // If the image already has the correct dimensions, return as-is
        if (originalWidth == targetWidth && originalHeight == targetHeight)
        {
            Console.WriteLine($"✅ Image already has correct dimensions ({originalWidth}x{originalHeight})");
            return imageData;
        }
        
        Console.WriteLine($"📏 Original image dimensions: {originalWidth}x{originalHeight}");
        Console.WriteLine($"🎯 Target dimensions: {targetWidth}x{targetHeight}");
        
        // Calculate aspect ratios
        var originalAspect = (double)originalWidth / originalHeight;
        var targetAspect = (double)targetWidth / targetHeight;
        
        // Resize and crop to ensure exact target dimensions
        if (Math.Abs(originalAspect - targetAspect) < 0.001) 
        {
            // Aspect ratios match - just resize
            Console.WriteLine("🔄 Resizing image (aspect ratios match)...");
            image.Mutate(x => x.Resize(targetWidth, targetHeight));
        }
        else
        {
            // Aspect ratios don't match - resize to fit and then crop
            int resizeWidth, resizeHeight;
            
            if (originalAspect > targetAspect)
            {
                // Original is wider - fit by height and crop width
                resizeHeight = targetHeight;
                resizeWidth = (int)Math.Round(targetHeight * originalAspect);
            }
            else
            {
                // Original is taller - fit by width and crop height  
                resizeWidth = targetWidth;
                resizeHeight = (int)Math.Round(targetWidth / originalAspect);
            }
            
            Console.WriteLine($"🔄 Resizing to {resizeWidth}x{resizeHeight} then cropping to {targetWidth}x{targetHeight}...");
            
            // Resize while maintaining aspect ratio
            image.Mutate(x => x.Resize(resizeWidth, resizeHeight));
            
            // Center crop to target dimensions
            var cropX = (resizeWidth - targetWidth) / 2;
            var cropY = (resizeHeight - targetHeight) / 2;
            
            image.Mutate(x => x.Crop(new Rectangle(cropX, cropY, targetWidth, targetHeight)));
        }
        
        // Save processed image to memory
        using var outputStream = new MemoryStream();
        await image.SaveAsPngAsync(outputStream);
        
        Console.WriteLine($"✅ Image processed to exact dimensions: {image.Width}x{image.Height}");
        return outputStream.ToArray();
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}