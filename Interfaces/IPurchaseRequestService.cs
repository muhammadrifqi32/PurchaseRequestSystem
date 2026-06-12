using PurchaseRequestSystem.DTOs;

namespace PurchaseRequestSystem.Interfaces;

public interface IPurchaseRequestService
{
    Task<PurchaseRequestResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<SubmitResponseDto> SubmitAsync(Guid id, CancellationToken cancellationToken = default);
}
