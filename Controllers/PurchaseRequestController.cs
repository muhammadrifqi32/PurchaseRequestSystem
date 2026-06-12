using Microsoft.AspNetCore.Mvc;
using PurchaseRequestSystem.Common;
using PurchaseRequestSystem.DTOs;
using PurchaseRequestSystem.Interfaces;

namespace PurchaseRequestSystem.Controllers;

[ApiController]
[Route("api/purchase-requests")]
public class PurchaseRequestController : ControllerBase
{
    private readonly IPurchaseRequestService _purchaseRequestService;

    public PurchaseRequestController(IPurchaseRequestService purchaseRequestService)
    {
        _purchaseRequestService = purchaseRequestService;
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PurchaseRequestResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _purchaseRequestService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<PurchaseRequestResponseDto>.Success(result, "Data retrieved successfully"));
    }

    [HttpPost("{id:guid}/submit")]
    [ProducesResponseType(typeof(ApiResponse<SubmitResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorData>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Submit(Guid id, CancellationToken cancellationToken)
    {
        var result = await _purchaseRequestService.SubmitAsync(id, cancellationToken);
        return Ok(ApiResponse<SubmitResponseDto>.Success(result, "Purchase request submitted successfully"));
    }
}
