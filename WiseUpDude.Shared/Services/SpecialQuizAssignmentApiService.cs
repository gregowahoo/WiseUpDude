using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using WiseUpDude.Model;

namespace WiseUpDude.Shared.Services
{
    public class SpecialQuizAssignmentApiService
    {
        private readonly HttpClient _httpClient;
        public SpecialQuizAssignmentApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<List<SpecialQuizAssignment>> GetAllAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<SpecialQuizAssignment>>("api/SpecialQuizAssignments");
        }
    }
}
