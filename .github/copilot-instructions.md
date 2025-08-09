# GitHub Copilot Instructions for ImageGenTool

## Repository Overview

ImageGenTool is a .NET application for generating images using AI services, specifically integrated with Google's Gemini API.

## Development Environment

This repository includes a development container configuration (`.devcontainer/devcontainer.json`) that pre-installs:
- .NET SDK 9.0
- GitHub CLI
- Git
- Essential VS Code extensions for .NET development and GitHub Copilot
- Environment variable configuration for `GEMINI_API_KEY` access

The development container ensures a consistent development environment with all required tools pre-installed and proper environment variable access for the Copilot coding agent.

## Technology Stack

- **Framework**: .NET SDK 9
- **Language**: C#
- **AI Service**: Google Gemini API
- **Platform**: Cross-platform .NET applications

## Development Guidelines

### .NET SDK 9 Requirements

- Always target .NET 9 when creating new projects
- Use the latest C# language features available in .NET 9
- Follow modern .NET conventions and patterns
- Use minimal APIs for web applications when appropriate

### Gemini API Integration

- **API Access**: Use Google's Gemini API for image generation capabilities
- **Authentication**: API keys should be stored in repository secrets and accessed via environment variables
- **Secret Names**: Use `GEMINI_API_KEY` as the standard secret name
- **Error Handling**: Implement proper error handling for API calls
- **Rate Limiting**: Consider implementing rate limiting and retry logic

### Security Best Practices

- **API Keys**: Never hardcode API keys in source code
- **Environment Variables**: Access secrets through `Environment.GetEnvironmentVariable("GEMINI_API_KEY")`
- **Configuration**: Use .NET's configuration system for managing settings
- **Logging**: Be careful not to log sensitive information like API keys

### Copilot Coding Agent Environment

The development container is configured to provide the `GEMINI_API_KEY` environment variable to the Copilot coding agent through the `containerEnv` configuration. This allows Copilot to access the API key when generating code that interacts with the Gemini API, following the GitHub documentation for setting environment variables in Copilot's environment.

### Code Style and Conventions

- Follow standard .NET naming conventions (PascalCase for public members, camelCase for private fields)
- Use async/await patterns for API calls
- Implement proper exception handling
- Add XML documentation comments for public APIs
- Use nullable reference types where appropriate

### Project Structure

- Organize code into appropriate namespaces
- Separate concerns (API clients, models, services, controllers)
- Use dependency injection for service registration
- Follow clean architecture principles

### Testing

- Write unit tests for business logic
- Mock external API calls in tests
- Use xUnit as the testing framework
- Implement integration tests for API endpoints

### Configuration Example

```csharp
// Program.cs or Startup.cs
services.AddHttpClient<IGeminiApiClient, GeminiApiClient>();
services.Configure<GeminiOptions>(options =>
{
    options.ApiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
});
```

### Error Handling Example

```csharp
try
{
    var result = await geminiClient.GenerateImageAsync(prompt);
    return Ok(result);
}
catch (HttpRequestException ex)
{
    logger.LogError(ex, "Failed to call Gemini API");
    return StatusCode(503, "Service temporarily unavailable");
}
catch (ArgumentException ex)
{
    logger.LogWarning(ex, "Invalid input provided");
    return BadRequest("Invalid request parameters");
}
```

## Repository Secrets Required

Ensure the following secrets are configured in the GitHub repository:
- `GEMINI_API_KEY`: Your Google Gemini API key

## When Generating Code

1. Always check for existing patterns in the codebase before creating new ones
2. Ensure API keys are properly secured and accessed from environment variables
3. Implement appropriate logging and error handling
4. Follow async patterns for I/O operations
5. Consider performance and resource usage when working with image generation
6. Add appropriate validation for user inputs
7. Document any new public APIs with XML comments