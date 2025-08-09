# ImageGenTool

A .NET global tool for generating images using Google's Gemini API.

## Features

- 🎨 Generate images from text prompts using Google's Gemini API
- 🚀 Easy-to-use command line interface
- 📦 Packaged as a .NET global tool
- 🔧 Built with .NET 8 and C#

## Installation

### From NuGet (when published)
```bash
dotnet tool install --global ImageGenTool
```

### From Source
```bash
git clone https://github.com/Sidoine/ImageGenTool.git
cd ImageGenTool
dotnet pack
dotnet tool install --global --add-source ./bin/Debug ImageGenTool
```

## Usage

```bash
imagegen --prompt "Your image description" --output "path/to/output.png" --api-key "your-gemini-api-key" --size "1024x1024"
```

### Options

- `-p, --prompt` (Required): The text prompt to generate an image from
- `-o, --output` (Required): The output path where the generated image will be saved
- `-k, --api-key` (Required): Your Google Gemini API key
- `-s, --size` (Optional): Image dimensions in WIDTHxHEIGHT format (e.g., 1024x1024, 512x768). Default is 1024x1024
- `-h, --help`: Show help information
- `--version`: Show version information

### Examples

```bash
# Generate a landscape image with default size (1024x1024)
imagegen -p "A serene mountain landscape at sunset" -o "./sunset.png" -k "your-api-key"

# Generate an abstract image with custom dimensions
imagegen -p "Abstract geometric patterns in blue and gold" -o "./abstract.png" -k "your-api-key" --size "512x768"

# Generate a portrait with square dimensions
imagegen -p "Portrait of a friendly robot" -o "./robot.png" -k "your-api-key" -s "1024x1024"

# Generate a wide landscape image
imagegen -p "Panoramic ocean sunset" -o "./ocean.png" -k "your-api-key" --size "1920x1080"
```

## Getting a Gemini API Key

1. Visit the [Google AI Studio](https://makersuite.google.com/app/apikey)
2. Sign in with your Google account
3. Create a new API key
4. Copy the API key and use it with the `--api-key` parameter

## Image Dimensions

The `--size` option allows you to specify custom image dimensions:

- **Format**: `WIDTHxHEIGHT` (case-insensitive, e.g., `1024x1024` or `1024X1024`)
- **Default**: `1024x1024` if not specified
- **Limits**: Width and height must be between 1 and 4096 pixels
- **Common sizes**:
  - `512x512` - Small square image
  - `1024x1024` - Standard square image (default)
  - `1920x1080` - Full HD landscape
  - `1080x1920` - Full HD portrait
  - `512x768` - Portrait aspect ratio
  - `768x512` - Landscape aspect ratio

The dimensions are communicated to the Gemini API as part of the prompt to help generate images at the requested resolution.

## Development

### Prerequisites

- .NET 8.0 SDK or later
- A Google Gemini API key for testing

### Building

```bash
dotnet build
```

### Running from Source

```bash
dotnet run -- --prompt "test prompt" --output "./test.png" --api-key "your-key" --size "1024x1024"
```

### Packaging

```bash
dotnet pack
```

## Architecture

The tool is built with:

- **System.CommandLine**: Modern command line parsing
- **HttpClient**: HTTP communication with the Gemini API
- **System.Text.Json**: JSON serialization for API requests
- **.NET 8**: Latest .NET framework with native AOT support

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

If you encounter any issues or have questions:

1. Check the existing [GitHub Issues](https://github.com/Sidoine/ImageGenTool/issues)
2. Create a new issue with detailed information about your problem
3. Include the command you ran and any error messages

## Roadmap

- [ ] Integration with actual Gemini image generation API when available
- [x] Support for custom image dimensions via --size option
- [ ] Support for different image formats (PNG, JPEG, WebP)
- [ ] Batch processing of multiple prompts
- [ ] Configuration file support
- [ ] Advanced prompt templating
- [ ] Progress indicators for long-running operations