using PurchaseRequestSystem.DTOs;

namespace PurchaseRequestSystem.Interfaces;

public interface IApprovalStageService
{
    Task<IReadOnlyList<ApprovalStageResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApprovalStageResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApprovalStageResponseDto> CreateAsync(CreateApprovalStageDto dto, CancellationToken cancellationToken = default);
    Task<ApprovalStageResponseDto> UpdateAsync(Guid id, UpdateApprovalStageDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
