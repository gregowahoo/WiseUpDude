using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WiseUpDude.Shared.Services.Interfaces;
using WiseUpDude.Model;

[ApiController]
[Route("api/[controller]")]
public class AssignmentTypesController : ControllerBase
{
    private readonly IAssignmentTypeService _assignmentTypeService;
    private readonly ILogger<AssignmentTypesController> _logger;

    public AssignmentTypesController(IAssignmentTypeService assignmentTypeService, ILogger<AssignmentTypesController> logger)
    {
        _assignmentTypeService = assignmentTypeService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<List<AssignmentType>> GetAll()
    {
        _logger.LogInformation("AssignmentTypes/GetAll called");
        var list = await _assignmentTypeService.GetAllAsync();
        _logger.LogInformation("AssignmentTypes/GetAll returning {Count} items", list?.Count ?? 0);
        return list ?? new List<AssignmentType>();
    }

    [HttpGet("{id}")]
    public async Task<AssignmentType?> GetById(int id)
    {
        _logger.LogInformation("AssignmentTypes/GetById called id={Id}", id);
        var item = await _assignmentTypeService.GetByIdAsync(id);
        if (item == null)
        {
            _logger.LogWarning("AssignmentTypes/GetById not found id={Id}", id);
        }
        return item;
    }
}