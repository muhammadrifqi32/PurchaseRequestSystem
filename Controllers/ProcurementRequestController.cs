using Microsoft.AspNetCore.Mvc;
using PurchaseRequestSystem.Common;
using PurchaseRequestSystem.DTOs;
using PurchaseRequestSystem.Interfaces;

namespace PurchaseRequestSystem.Controllers;

[ApiController]
[Route("api/procurement-requests")]
public class ProcurementRequestController : ControllerBase
{
    private readonly IProcurementRequestService _procurementRequestService;

    public ProcurementRequestController(IProcurementRequestService procurementRequestService)
    {
        _procurementRequestService = procurementRequestService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ProcurementRequestResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _procurementRequestService.GetAllAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<ProcurementRequestResponseDto>>.Success(result, "Data retrieved successfully"));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ProcurementRequestResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _procurementRequestService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<ProcurementRequestResponseDto>.Success(result, "Data retrieved successfully"));
    }

    [HttpPost("project")]
    [ProducesResponseType(typeof(ApiResponse<ProcurementRequestResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorData>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectProcurementRequestDto dto, CancellationToken cancellationToken)
    {
        var result = await _procurementRequestService.CreateProjectAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.ProcurementRequestId }, ApiResponse<ProcurementRequestResponseDto>.Created(result, "Project procurement request created successfully"));
    }

    [HttpPost("non-project")]
    [ProducesResponseType(typeof(ApiResponse<ProcurementRequestResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorData>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateNonProject([FromBody] CreateNonProjectProcurementRequestDto dto, CancellationToken cancellationToken)
    {
        var result = await _procurementRequestService.CreateNonProjectAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.ProcurementRequestId }, ApiResponse<ProcurementRequestResponseDto>.Created(result, "Non-project procurement request created successfully"));
    }
}
