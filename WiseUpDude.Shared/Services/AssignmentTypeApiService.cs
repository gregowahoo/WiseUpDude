using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WiseUpDude.Model;
using WiseUpDude.Shared.Services.Interfaces;

namespace WiseUpDude.Shared.Services
{
    public class AssignmentTypeApiService : IAssignmentTypeService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AssignmentTypeApiService> _logger;

        public AssignmentTypeApiService(HttpClient httpClient, ILogger<AssignmentTypeApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }
        public async Task<List<AssignmentType>> GetAllAsync()
        {
            var path = "api/assignmenttypes";
            _logger.LogInformation("AssignmentTypeApi: GET {Path} Base={Base}", path, _httpClient.BaseAddress);
            try
            {
                var list = await _httpClient.GetFromJsonAsync<List<AssignmentType>>(path);
                _logger.LogInformation("AssignmentTypeApi: GET {Path} returned {Count} items", path, list?.Count ?? 0);
                return list ?? new List<AssignmentType>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "AssignmentTypeApi: HTTP error during GET {Path}", path);
                throw;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "AssignmentTypeApi: Unexpected error during GET {Path}", path);
                throw;
            }
        }
        public async Task<AssignmentType> GetByIdAsync(int id)
        {
            var path = $"api/assignmenttypes/{id}";
            _logger.LogInformation("AssignmentTypeApi: GET {Path} Base={Base}", path, _httpClient.BaseAddress);
            try
            {
                var item = await _httpClient.GetFromJsonAsync<AssignmentType>(path);
                if (item == null)
                {
                    _logger.LogWarning("AssignmentTypeApi: GET {Path} returned null", path);
                }
                return item!;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "AssignmentTypeApi: HTTP error during GET {Path}", path);
                throw;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "AssignmentTypeApi: Unexpected error during GET {Path}", path);
                throw;
            }
        }
    }
}
