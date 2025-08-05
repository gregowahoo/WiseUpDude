using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace WiseUpDude.Shared.Services
{
    public class AssignmentTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public interface IAssignmentTypeService
    {
        Task<List<AssignmentTypeDto>> GetAllAsync();
        Task<AssignmentTypeDto> GetByIdAsync(int id);
    }

    public class AssignmentTypeService : IAssignmentTypeService
    {
        private readonly HttpClient _httpClient;
        public AssignmentTypeService(HttpClient httpClient)
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
