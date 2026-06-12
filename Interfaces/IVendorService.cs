using PurchaseRequestSystem.DTOs;

namespace PurchaseRequestSystem.Interfaces;

public interface IVendorService
{
    Task<IReadOnlyList<VendorResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<VendorResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<VendorResponseDto> CreateAsync(CreateVendorDto dto, CancellationToken cancellationToken = default);
    Task<VendorResponseDto> UpdateAsync(Guid id, UpdateVendorDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
