using PurchaseRequestSystem.DTOs;

namespace PurchaseRequestSystem.Interfaces;

public interface IRoleService
{
    Task<IReadOnlyList<RoleResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<RoleResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<RoleResponseDto> CreateAsync(CreateRoleDto dto, CancellationToken cancellationToken = default);
    Task<RoleResponseDto> UpdateAsync(Guid id, UpdateRoleDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
