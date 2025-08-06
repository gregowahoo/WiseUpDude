using System.Collections.Generic;
using System.Threading.Tasks;
using WiseUpDude.Model;

namespace WiseUpDude.Shared.Services
{
    //public class AssignmentTypeDto
    //{
    //    public int Id { get; set; }
    //    public string Name { get; set; }
    //    public string Description { get; set; }
    //}

    public interface IAssignmentTypeService
    {
        Task<List<AssignmentType>> GetAllAsync();
        Task<AssignmentType> GetByIdAsync(int id);
    }
}
