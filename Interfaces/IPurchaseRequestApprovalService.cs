using PurchaseRequestSystem.DTOs;

namespace PurchaseRequestSystem.Interfaces;

public interface IPurchaseRequestApprovalService
{
    Task<SubmitResponseDto> SubmitAsync(Guid purchaseRequestId, SubmitPurchaseRequestDto? dto, CancellationToken cancellationToken = default);
    Task<SubmitResponseDto> ApproveByGmAsync(Guid purchaseRequestId, ApprovalActionDto dto, CancellationToken cancellationToken = default);
    Task<SubmitResponseDto> RejectByGmAsync(Guid purchaseRequestId, ApprovalActionDto dto, CancellationToken cancellationToken = default);
    Task<SubmitResponseDto> RequestRevisionByGmAsync(Guid purchaseRequestId, ApprovalActionDto dto, CancellationToken cancellationToken = default);
    Task<SubmitResponseDto> RecordChairmanApprovalAsync(Guid purchaseRequestId, ChairmanConfirmationDto dto, CancellationToken cancellationToken = default);
    Task<SubmitResponseDto> RecordChairmanRejectionAsync(Guid purchaseRequestId, ChairmanConfirmationDto dto, CancellationToken cancellationToken = default);
    Task<SubmitResponseDto> RecordChairmanRevisionAsync(Guid purchaseRequestId, ChairmanConfirmationDto dto, CancellationToken cancellationToken = default);
    Task<List<ApprovalHistoryResponseDto>> GetApprovalHistoryAsync(Guid purchaseRequestId, CancellationToken cancellationToken = default);
}
