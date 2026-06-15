using Microsoft.AspNetCore.Mvc;
using PurchaseRequestSystem.Common;
using PurchaseRequestSystem.DTOs;
using PurchaseRequestSystem.Interfaces;

namespace PurchaseRequestSystem.Controllers;

[ApiController]
[Route("api/purchase-orders")]
public class PurchaseOrderController : ControllerBase
{
    private readonly IPurchaseOrderService _purchaseOrderService;

    public PurchaseOrderController(IPurchaseOrderService purchaseOrderService)
    {
        _purchaseOrderService = purchaseOrderService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<PurchaseOrderResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _purchaseOrderService.GetAllAsync(cancellationToken);
        return Ok(ApiResponse<List<PurchaseOrderResponseDto>>.Success(result, "Data retrieved successfully"));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PurchaseOrderResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _purchaseOrderService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<PurchaseOrderResponseDto>.Success(result, "Data retrieved successfully"));
    }

    [HttpGet("by-purchase-request/{purchaseRequestId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PurchaseOrderResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByPurchaseRequestId(Guid purchaseRequestId, CancellationToken cancellationToken)
    {
        var result = await _purchaseOrderService.GetByPurchaseRequestIdAsync(purchaseRequestId, cancellationToken);
        return Ok(ApiResponse<PurchaseOrderResponseDto>.Success(result, "Data retrieved successfully"));
    }

    [HttpPost("from-purchase-request/{purchaseRequestId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PurchaseOrderResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorData>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GenerateFromPurchaseRequest(Guid purchaseRequestId, [FromBody] GeneratePurchaseOrderDto dto, CancellationToken cancellationToken)
    {
        var result = await _purchaseOrderService.GenerateFromPurchaseRequestAsync(purchaseRequestId, dto, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<PurchaseOrderResponseDto>.Created(result, "Purchase Order generated successfully"));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PurchaseOrderResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorData>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePurchaseOrderDto dto, CancellationToken cancellationToken)
    {
        var result = await _purchaseOrderService.UpdateAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<PurchaseOrderResponseDto>.Success(result, "Purchase Order updated successfully"));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorData>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _purchaseOrderService.DeleteAsync(id, cancellationToken);
        return Ok(ApiResponse.Ok("Purchase Order deleted successfully"));
    }
}
