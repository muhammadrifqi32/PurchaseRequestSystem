using PurchaseRequestSystem.DTOs;

namespace PurchaseRequestSystem.Interfaces;

public interface IRequestTypeService
{
    Task<IReadOnlyList<RequestTypeResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<RequestTypeResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<RequestTypeResponseDto> CreateAsync(CreateRequestTypeDto dto, CancellationToken cancellationToken = default);
    Task<RequestTypeResponseDto> UpdateAsync(Guid id, UpdateRequestTypeDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
