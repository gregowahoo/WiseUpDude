namespace WiseUpDude.Services.CategoryArt;

public sealed class OpenAiImageOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string ImageModel { get; set; } = "gpt-image-1";
    public string ImageSize { get; set; } = "1024x1024";
}
