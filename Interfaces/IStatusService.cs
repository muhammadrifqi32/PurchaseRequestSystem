using PurchaseRequestSystem.DTOs;

namespace PurchaseRequestSystem.Interfaces;

public interface IStatusService
{
    Task<IReadOnlyList<StatusResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<StatusResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<StatusResponseDto> CreateAsync(CreateStatusDto dto, CancellationToken cancellationToken = default);
    Task<StatusResponseDto> UpdateAsync(Guid id, UpdateStatusDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
