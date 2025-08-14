namespace WiseUpDude.Services.CategoryArt;

public interface ICategoryArtService
{
    Task<byte[]> GeneratePngAsync(string prompt, CancellationToken ct = default);
}
