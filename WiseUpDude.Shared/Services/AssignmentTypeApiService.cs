using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using WiseUpDude.Model;
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
        public async Task<List<AssignmentType>> GetAllAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<AssignmentType>>("api/SpecialQuizAssignments/types") ?? new List<AssignmentType>();
        }
        public async Task<AssignmentType> GetByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<AssignmentType>($"api/SpecialQuizAssignments/types/{id}");
        }
    }
}
