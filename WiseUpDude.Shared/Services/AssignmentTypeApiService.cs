using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using WiseUpDude.Shared.Services;

namespace WiseUpDude.Shared.Services
{
    public class AssignmentTypeApiService : IAssignmentTypeService
    {
        private readonly HttpClient _httpClient;
        public AssignmentTypeApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<List<AssignmentTypeDto>> GetAllAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<AssignmentTypeDto>>("api/SpecialQuizAssignments/types") ?? new List<AssignmentTypeDto>();
        }
        public async Task<AssignmentTypeDto> GetByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<AssignmentTypeDto>($"api/SpecialQuizAssignments/types/{id}");
        }
    }
}
