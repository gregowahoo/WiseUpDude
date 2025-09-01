using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using WiseUpDude.Model;

namespace WiseUpDude.Shared.Services
{
    public class SpecialQuizAssignmentsApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<SpecialQuizAssignmentsApiService> _logger;

        public SpecialQuizAssignmentsApiService(HttpClient httpClient, ILogger<SpecialQuizAssignmentsApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<SpecialQuizAssignment>> GetAllAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<SpecialQuizAssignment>>("api/SpecialQuizAssignments") ?? new();
        }

        public async Task<List<SpecialQuizAssignment>> GetActiveAsync(DateTime? asOfUtc = null)
        {
            var url = "api/SpecialQuizAssignments/active";
            if (asOfUtc.HasValue)
            {
                url += $"?asOf={Uri.EscapeDataString(asOfUtc.Value.ToString("o"))}";
            }
            return await _httpClient.GetFromJsonAsync<List<SpecialQuizAssignment>>(url) ?? new();
        }

        public async Task<List<AssignmentType>> GetTypesAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<AssignmentType>>("api/SpecialQuizAssignments/types") ?? new();
        }

        public async Task UpdateAsync(SpecialQuizAssignment assignment)
        {
            var resp = await _httpClient.PutAsJsonAsync("api/SpecialQuizAssignments", assignment);
            resp.EnsureSuccessStatusCode();
        }

        public async Task DeactivateAsync(int id)
        {
            var resp = await _httpClient.PostAsync($"api/SpecialQuizAssignments/{id}/deactivate", null);
            resp.EnsureSuccessStatusCode();
        }

        public async Task DeleteAsync(int id)
        {
            var resp = await _httpClient.DeleteAsync($"api/SpecialQuizAssignments/{id}");
            resp.EnsureSuccessStatusCode();
        }
    }
}
