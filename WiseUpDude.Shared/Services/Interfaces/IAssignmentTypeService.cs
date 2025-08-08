using System.Collections.Generic;
using System.Threading.Tasks;
using WiseUpDude.Model;

namespace WiseUpDude.Shared.Services.Interfaces
{
    public interface IAssignmentTypeService
    {
        Task<List<AssignmentType>> GetAllAsync();
        Task<AssignmentType> GetByIdAsync(int id);
    }
}
