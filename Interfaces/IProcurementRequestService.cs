using PurchaseRequestSystem.DTOs;

namespace PurchaseRequestSystem.Interfaces;

public interface IProcurementRequestService
{
    Task<IReadOnlyList<ProcurementRequestResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ProcurementRequestResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProcurementRequestResponseDto> CreateProjectAsync(CreateProjectProcurementRequestDto dto, CancellationToken cancellationToken = default);
    Task<ProcurementRequestResponseDto> CreateNonProjectAsync(CreateNonProjectProcurementRequestDto dto, CancellationToken cancellationToken = default);
}
