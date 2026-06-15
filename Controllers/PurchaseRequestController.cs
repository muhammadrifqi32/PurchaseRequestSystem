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
    private readonly IPurchaseRequestApprovalService _purchaseRequestApprovalService;

    public PurchaseRequestController(
        IPurchaseRequestService purchaseRequestService,
        IPurchaseRequestApprovalService purchaseRequestApprovalService)
    {
        _purchaseRequestService = purchaseRequestService;
        _purchaseRequestApprovalService = purchaseRequestApprovalService;
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
    public async Task<IActionResult> Submit(Guid id, [FromBody(EmptyBodyBehavior = Microsoft.AspNetCore.Mvc.ModelBinding.EmptyBodyBehavior.Allow)] SubmitPurchaseRequestDto? dto, CancellationToken cancellationToken)
    {
        var result = await _purchaseRequestApprovalService.SubmitAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<SubmitResponseDto>.Success(result, "Purchase Request submitted to GM approval successfully"));
    }

    [HttpPost("{id:guid}/approve-gm")]
    [ProducesResponseType(typeof(ApiResponse<SubmitResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorData>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ApproveByGm(Guid id, [FromBody] ApprovalActionDto dto, CancellationToken cancellationToken)
    {
        var result = await _purchaseRequestApprovalService.ApproveByGmAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<SubmitResponseDto>.Success(result, "Purchase Request approved by GM successfully"));
    }

    [HttpPost("{id:guid}/reject-gm")]
    [ProducesResponseType(typeof(ApiResponse<SubmitResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorData>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RejectByGm(Guid id, [FromBody] ApprovalActionDto dto, CancellationToken cancellationToken)
    {
        var result = await _purchaseRequestApprovalService.RejectByGmAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<SubmitResponseDto>.Success(result, "Purchase Request rejected by GM successfully"));
    }

    [HttpPost("{id:guid}/request-revision-gm")]
    [ProducesResponseType(typeof(ApiResponse<SubmitResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorData>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RequestRevisionByGm(Guid id, [FromBody] ApprovalActionDto dto, CancellationToken cancellationToken)
    {
        var result = await _purchaseRequestApprovalService.RequestRevisionByGmAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<SubmitResponseDto>.Success(result, "Purchase Request revision requested by GM successfully"));
    }

    [HttpPost("{id:guid}/record-chairman-approval")]
    [ProducesResponseType(typeof(ApiResponse<SubmitResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorData>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecordChairmanApproval(Guid id, [FromBody] ChairmanConfirmationDto dto, CancellationToken cancellationToken)
    {
        var result = await _purchaseRequestApprovalService.RecordChairmanApprovalAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<SubmitResponseDto>.Success(result, "Chairman approval recorded successfully"));
    }

    [HttpPost("{id:guid}/record-chairman-rejection")]
    [ProducesResponseType(typeof(ApiResponse<SubmitResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorData>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecordChairmanRejection(Guid id, [FromBody] ChairmanConfirmationDto dto, CancellationToken cancellationToken)
    {
        var result = await _purchaseRequestApprovalService.RecordChairmanRejectionAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<SubmitResponseDto>.Success(result, "Chairman rejection recorded successfully"));
    }

    [HttpPost("{id:guid}/record-chairman-revision")]
    [ProducesResponseType(typeof(ApiResponse<SubmitResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorData>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecordChairmanRevision(Guid id, [FromBody] ChairmanConfirmationDto dto, CancellationToken cancellationToken)
    {
        var result = await _purchaseRequestApprovalService.RecordChairmanRevisionAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<SubmitResponseDto>.Success(result, "Chairman revision request recorded successfully"));
    }

    [HttpGet("{id:guid}/approval-history")]
    [ProducesResponseType(typeof(ApiResponse<List<ApprovalHistoryResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetApprovalHistory(Guid id, CancellationToken cancellationToken)
    {
        var result = await _purchaseRequestApprovalService.GetApprovalHistoryAsync(id, cancellationToken);
        return Ok(ApiResponse<List<ApprovalHistoryResponseDto>>.Success(result, "Approval history retrieved successfully"));
    }
}
