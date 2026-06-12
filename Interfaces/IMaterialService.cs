using PurchaseRequestSystem.DTOs;

namespace PurchaseRequestSystem.Interfaces;

public interface IMaterialService
{
    Task<IReadOnlyList<MaterialResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<MaterialResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<MaterialResponseDto> CreateAsync(CreateMaterialDto dto, CancellationToken cancellationToken = default);
    Task<MaterialResponseDto> UpdateAsync(Guid id, UpdateMaterialDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
