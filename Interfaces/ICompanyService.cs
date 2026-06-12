using PurchaseRequestSystem.DTOs;

namespace PurchaseRequestSystem.Interfaces;

public interface ICompanyService
{
    Task<IReadOnlyList<CompanyResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<CompanyResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CompanyResponseDto> CreateAsync(CreateCompanyDto dto, CancellationToken cancellationToken = default);
    Task<CompanyResponseDto> UpdateAsync(Guid id, UpdateCompanyDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
