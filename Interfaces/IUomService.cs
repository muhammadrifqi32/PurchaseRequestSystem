using PurchaseRequestSystem.DTOs;

namespace PurchaseRequestSystem.Interfaces;

public interface IUomService
{
    Task<IReadOnlyList<UomResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<UomResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UomResponseDto> CreateAsync(CreateUomDto dto, CancellationToken cancellationToken = default);
    Task<UomResponseDto> UpdateAsync(Guid id, UpdateUomDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
