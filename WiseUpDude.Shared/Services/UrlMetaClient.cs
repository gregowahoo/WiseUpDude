using System.Net.Http.Json;
using WiseUpDude.Shared.Model;

namespace WiseUpDude.Shared.Services
{
    public class UrlMetaClient
    {
        private readonly HttpClient _httpClient;

        public UrlMetaClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<UrlMetaResult> GetUrlMetaAsync(string url)
        {
            var response = await _httpClient.GetAsync($"api/urlmeta?url={Uri.EscapeDataString(url)}");
            if (!response.IsSuccessStatusCode)
                return new UrlMetaResult { Title = url, Description = null };
            return await response.Content.ReadFromJsonAsync<UrlMetaResult>() ?? new UrlMetaResult { Title = url, Description = null };
        }
    }
}
