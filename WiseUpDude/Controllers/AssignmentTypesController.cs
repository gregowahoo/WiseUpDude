using Microsoft.AspNetCore.Mvc;
using WiseUpDude.Shared.Services.Interfaces;
using WiseUpDude.Model;

[ApiController]
[Route("api/[controller]")]
public class AssignmentTypesController : ControllerBase
{
    private readonly IAssignmentTypeService _assignmentTypeService;

    public AssignmentTypesController(IAssignmentTypeService assignmentTypeService)
    {
        _assignmentTypeService = assignmentTypeService;
    }

    [HttpGet]
    public async Task<List<AssignmentType>> GetAll()
    {
        return await _assignmentTypeService.GetAllAsync();
    }

    [HttpGet("{id}")]
    public async Task<AssignmentType?> GetById(int id)
    {
        return await _assignmentTypeService.GetByIdAsync(id);
    }
}