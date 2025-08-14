using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI.Images;

namespace WiseUpDude.Services.CategoryArt;

public sealed class CategoryArtService : ICategoryArtService
{
    private readonly ImageClient _client;
    private readonly OpenAiImageOptions _opts;
    private readonly ILogger<CategoryArtService> _logger;

    public CategoryArtService(IOptions<OpenAiImageOptions> opts, ILogger<CategoryArtService> logger)
    {
        _opts = opts.Value;
        _logger = logger;
        _client = new ImageClient(_opts.ImageModel, _opts.ApiKey);
        _logger.LogInformation("CategoryArtService initialized. Model={Model} Size={Size}", _opts.ImageModel, _opts.ImageSize);
    }

    public async Task<byte[]> GeneratePngAsync(string prompt, CancellationToken ct = default)
    {
        var preview = prompt?.Length > 96 ? prompt[..96] + "…" : prompt ?? string.Empty;
        _logger.LogInformation("Generating category art. Model={Model} Size={Size} PromptPreview={Preview}", _opts.ImageModel, _opts.ImageSize, preview);

        var options = new ImageGenerationOptions
        {
            // Don't set Quality to avoid sending unsupported values (e.g., 'hd')
            Size = _opts.ImageSize switch
            {
                "256x256" => GeneratedImageSize.W256xH256,
                "512x512" => GeneratedImageSize.W512xH512,
                _ => GeneratedImageSize.W1024xH1024
            }
        };

        try
        {
            var result = await _client.GenerateImageAsync(prompt, options, ct);
            var bytes = result.Value.ImageBytes.ToArray();
            _logger.LogInformation("Image generated successfully. Bytes={Length}", bytes.Length);
            return bytes;
        }
        catch (System.ClientModel.ClientResultException cre)
        {
            _logger.LogError(cre, "OpenAI image generation failed (ClientResult). Status={Status} Message={Message}", (int?)cre.Status, cre.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OpenAI image generation failed unexpectedly.");
            throw;
        }
    }
}
