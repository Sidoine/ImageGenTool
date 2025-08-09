using System.CommandLine;
using ImageGenTool;

var promptOption = new Option<string>(
    name: "--prompt",
    description: "The text prompt to generate an image from")
{
    IsRequired = true
};
promptOption.AddAlias("-p");

var outputOption = new Option<FileInfo>(
    name: "--output",
    description: "The output path where the generated image will be saved")
{
    IsRequired = true
};
outputOption.AddAlias("-o");

var apiKeyOption = new Option<string>(
    name: "--api-key",
    description: "Your Google Gemini API key for image generation. If not provided, will use GEMINI_API_KEY environment variable")
{
    IsRequired = false
};
apiKeyOption.AddAlias("-k");

var sizeOption = new Option<string>(
    name: "--size",
    description: "Image dimensions in WIDTHxHEIGHT format (e.g., 1024x1024, 512x768). Default is 1024x1024")
{
    IsRequired = false
};
sizeOption.AddAlias("-s");
sizeOption.SetDefaultValue("1024x1024");

var rootCommand = new RootCommand("ImageGenTool - Generate images using Google Gemini API")
{
    promptOption,
    outputOption,
    apiKeyOption,
    sizeOption
};

rootCommand.SetHandler(async (string prompt, FileInfo output, string? apiKey, string size) =>
{
    try
    {
        Console.WriteLine("🎨 ImageGenTool - Generating image using Google Gemini API");
        Console.WriteLine($"📝 Prompt: {prompt}");
        Console.WriteLine($"📁 Output: {output.FullName}");
        Console.WriteLine($"📐 Size: {size}");
        Console.WriteLine();

        // Validate inputs
        if (string.IsNullOrWhiteSpace(prompt))
        {
            Console.Error.WriteLine("❌ Error: Prompt cannot be empty");
            return;
        }

        // Parse and validate size format
        if (!TryParseSize(size, out int width, out int height))
        {
            Console.Error.WriteLine("❌ Error: Invalid size format. Use WIDTHxHEIGHT format (e.g., 1024x1024)");
            return;
        }

        // Get API key from parameter or environment variables
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
        }

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            Console.Error.WriteLine("❌ Error: API key must be provided via --api-key parameter or GEMINI_API_KEY environment variable");
            return;
        }

        Console.WriteLine("🔑 Using API key from " + (apiKey == Environment.GetEnvironmentVariable("GEMINI_API_KEY") ? "GEMINI_API_KEY" : "command line"));

        // Ensure output directory exists
        var outputDir = output.Directory;
        if (outputDir != null && !outputDir.Exists)
        {
            Console.WriteLine($"📂 Creating output directory: {outputDir.FullName}");
            outputDir.Create();
        }

        // Generate image
        Console.WriteLine("🔄 Generating image...");
        using var generator = new GeminiImageGenerator(apiKey);
        var imageData = await generator.GenerateImageAsync(prompt, width, height);

        // Save image
        Console.WriteLine("💾 Saving image...");
        await File.WriteAllBytesAsync(output.FullName, imageData);

        Console.WriteLine();
        Console.WriteLine("✅ Image generated successfully!");
        Console.WriteLine($"📍 Saved to: {output.FullName}");
        Console.WriteLine($"📊 Size: {imageData.Length:N0} bytes");
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"❌ Error: {ex.Message}");
        Environment.ExitCode = 1;
    }
}, promptOption, outputOption, apiKeyOption, sizeOption);

static bool TryParseSize(string sizeString, out int width, out int height)
{
    width = height = 0;
    
    if (string.IsNullOrWhiteSpace(sizeString))
        return false;
    
    var parts = sizeString.Split('x', 'X');
    if (parts.Length != 2)
        return false;
    
    return int.TryParse(parts[0], out width) && 
           int.TryParse(parts[1], out height) && 
           width > 0 && height > 0 &&
           width <= 4096 && height <= 4096; // Reasonable limits
}

return await rootCommand.InvokeAsync(args);
